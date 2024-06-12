#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Services.LocationControllerLoggers/LegacySignalControllerLogger.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using ATSPM.Application.Common.EqualityComparers;
using ATSPM.Application.Configuration;
using ATSPM.Application.Services;
using ATSPM.Data;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using ATSPM.Domain.Common;
using EFCore.BulkExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Infrastructure.Services.LocationControllerLoggers
{
    public class LegacyLocationControllerLogger : LocationControllerLoggerBase
    {
        private readonly IOptions<LocationControllerLoggerConfiguration> _options;
        private readonly IServiceProvider _serviceProvider;

        public LegacyLocationControllerLogger(ILogger<LegacyLocationControllerLogger> log, IOptions<LocationControllerLoggerConfiguration> options, IServiceProvider serviceProvider) : base(log)
        {
            _options = options;
            _serviceProvider = serviceProvider;
        }

        public override Task Initialize()
        {
            var stepOptions = new ExecutionDataflowBlockOptions()
            {
                CancellationToken = token,
                //NameFormat = blockName,
                MaxDegreeOfParallelism = _options.Value.MaxDegreeOfParallelism,
                //BoundedCapacity = capcity,
                //MaxMessagesPerTask = ?,
                SingleProducerConstrained = true,
                EnsureOrdered = false
            };

            //create steps
            var downloader = CreateTransformManyStep<Location, DirectoryInfo>(t => DownloadLogs(t, token), "DownloadFilesStep", stepOptions);
            var getFiles = CreateTransformManyStep<DirectoryInfo, FileInfo>(t => GetFiles(t), "GetFilesStep", stepOptions);
            var fileToLogs = CreateTransformManyStep<FileInfo, ControllerEventLog>(t => CreateEventLogs(t, token), "DecodeEventLogsStep", stepOptions);
            var logArchiveBatch = new BatchBlock<ControllerEventLog>(_options.Value.SaveToDatabaseBatchSize, new GroupingDataflowBlockOptions() { CancellationToken = token, NameFormat = "Archive Batch" });
            var saveToRepoTemp = CreateTransformManyStep<ControllerEventLog[], ControllerEventLog>(t => SaveToRepo(t, token), "SaveToRepo", stepOptions);
            var endResult = CreateActionStep<ControllerEventLog>(t => { }, "EndResultStep", stepOptions);

            //step linking
            downloader.LinkTo(getFiles, new DataflowLinkOptions() { PropagateCompletion = true });
            getFiles.LinkTo(fileToLogs, new DataflowLinkOptions() { PropagateCompletion = true });
            fileToLogs.LinkTo(logArchiveBatch, new DataflowLinkOptions() { PropagateCompletion = true });
            logArchiveBatch.LinkTo(saveToRepoTemp, new DataflowLinkOptions() { PropagateCompletion = true });
            saveToRepoTemp.LinkTo(endResult, new DataflowLinkOptions() { PropagateCompletion = true });

            //base.Initialize();

            return Task.CompletedTask;
        }

        protected async virtual Task<IEnumerable<DirectoryInfo>> DownloadLogs(Location Location, CancellationToken cancellationToken = default)
        {
            var fileList = new List<FileInfo>();

            using (var scope = _serviceProvider.CreateScope())
            {
                var downloader = scope.ServiceProvider.GetServices<IDeviceDownloader>().First(c => c.CanExecute(Location));

                //await foreach (var file in downloader.Execute(Location, cancellationToken))
                //{
                //    fileList.Add(file);
                //}
            }

            return fileList.Select(s => s.Directory).Distinct(new LambdaEqualityComparer<DirectoryInfo>((x, y) => x.FullName == y.FullName));
        }

        protected virtual IEnumerable<FileInfo> GetFiles(DirectoryInfo directoryInfo)
        {
            var files = new List<FileInfo>();

            files = directoryInfo?.GetFiles("*.*", SearchOption.AllDirectories).ToList();

            return files;
        }

        protected async virtual Task<IEnumerable<ControllerEventLog>> CreateEventLogs(FileInfo file, CancellationToken cancellationToken = default)
        {
            HashSet<ControllerEventLog> logList = new HashSet<ControllerEventLog>(new ControllerEventLogEqualityComparer());

            using (var scope = _serviceProvider.CreateScope())
            {
                var decoder = scope.ServiceProvider.GetServices<ILocationControllerDecoder<IndianaEvent>>().First(c => c.CanExecute(file));
                //logList = await decoder.ExecuteAsync(file, cancellationToken);
            }

            return logList;
        }

        protected async virtual Task<IEnumerable<ControllerEventLog>> SaveToRepo(ControllerEventLog[] logs, CancellationToken cancellationToken = default)
        {
            HashSet<ControllerEventLog> result = new HashSet<ControllerEventLog>(logs, new ControllerEventLogEqualityComparer());

            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetService<LegacyEventLogContext>();

                await db.BulkInsertOrUpdateAsync(result.ToList(),
                    new BulkConfig()
                    {
                        SqlBulkCopyOptions = Microsoft.Data.SqlClient.SqlBulkCopyOptions.CheckConstraints,
                        OmitClauseExistsExcept = true,
                        BulkCopyTimeout = _options.Value.BulkCopyTimeout
                    },
                    null,
                    null,
                    cancellationToken);
            }

            return result;
        }
    }
}
