#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - %Namespace%/CompressedSignalControllerLogger.cs
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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks.Dataflow;

namespace Utah.Udot.Atspm.Infrastructure.Services.SignalControllerLoggers
{
    public class CompressedLocationControllerLogger : LocationControllerLoggerBase
    {
        //private readonly ILogger _log;
        private readonly IOptions<LocationControllerLoggerConfiguration> _options;
        private readonly IServiceProvider _serviceProvider;

        public CompressedLocationControllerLogger(ILogger<CompressedLocationControllerLogger> log, IOptions<LocationControllerLoggerConfiguration> options, IServiceProvider serviceProvider) : base(log)
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
                SingleProducerConstrained = true,
                EnsureOrdered = false
            };

            //create steps
            var downloader = CreateTransformManyStep<Location, DirectoryInfo>(t => DownloadLogs(t, token), "DownloadFilesStep", stepOptions);
            var getFiles = CreateTransformManyStep<DirectoryInfo, FileInfo>(t => GetFiles(t), "GetFilesStep", stepOptions);
            var fileToLogs = CreateTransformManyStep<FileInfo, ControllerEventLog>(t => CreateEventLogs(t, token), "DecodeEventLogsStep", stepOptions);
            var logArchiveBatch = new BatchBlock<ControllerEventLog>(_options.Value.SaveToDatabaseBatchSize, new GroupingDataflowBlockOptions() { CancellationToken = token, NameFormat = "Archive Batch" });
            var logsToArchive = CreateTransformManyStep<ControllerEventLog[], ControllerLogArchive>(t => ArchiveLogs(t), "ArchiveLogsStep", stepOptions);
            var saveToRepo = CreateTransformManyStep<ControllerLogArchive, ControllerLogArchive>(t => SaveToRepo(t, token), "SaveToRepo", stepOptions);
            var endResult = CreateActionStep<ControllerLogArchive>(t => { }, "EndResultStep", stepOptions);

            //step linking
            downloader.LinkTo(getFiles, new DataflowLinkOptions() { PropagateCompletion = true });
            getFiles.LinkTo(fileToLogs, new DataflowLinkOptions() { PropagateCompletion = true });
            fileToLogs.LinkTo(logArchiveBatch, new DataflowLinkOptions() { PropagateCompletion = true });
            logArchiveBatch.LinkTo(logsToArchive, new DataflowLinkOptions() { PropagateCompletion = true });
            logsToArchive.LinkTo(saveToRepo, new DataflowLinkOptions() { PropagateCompletion = true });
            saveToRepo.LinkTo(endResult, new DataflowLinkOptions() { PropagateCompletion = true });

            return base.Initialize();
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
            //HashSet<ControllerEventLog> logList = new HashSet<ControllerEventLog>(new IndianaEventEqualityComparer());

            //using (var scope = _serviceProvider.CreateScope())
            //{
            //    //var decoder = scope.ServiceProvider.GetServices<IEventLogDecoder<IndianaEvent>>().First(c => c.CanExecute(file));
            //    //logList = await decoder.ExecuteAsync(file, cancellationToken);
            //}

            //return logList;

            return default;
        }

        protected virtual IEnumerable<ControllerLogArchive> ArchiveLogs(ControllerEventLog[] logs)
        {
            //    HashSet<ControllerEventLog> uniqueLogs = new HashSet<ControllerEventLog>(logs, new IndianaEventEqualityComparer());

            //    return uniqueLogs.GroupBy(g => (g.Timestamp.Date, g.SignalIdentifier)).Select(s => new ControllerLogArchive() { SignalIdentifier = s.Key.SignalIdentifier, ArchiveDate = s.Key.Date, LogData = s.ToList() });

            return default;
        }

        protected async virtual Task<IEnumerable<ControllerLogArchive>> SaveToRepo(ControllerLogArchive archive, CancellationToken cancellationToken = default)
        {
            //List<ControllerLogArchive> result = new List<ControllerLogArchive>();

            //using (var scope = _serviceProvider.CreateScope())
            //{
            //    IControllerEventLogRepository EventLogArchive = scope.ServiceProvider.GetService<IControllerEventLogRepository>();
            //    var searchLog = await EventLogArchive.LookupAsync(archive);

            //    if (searchLog != null)
            //    {
            //        var eventLogs = new HashSet<ControllerEventLog>(Enumerable.Union(searchLog.LogData, archive.LogData), new IndianaEventEqualityComparer());
            //        searchLog.LogData = eventLogs.ToList();

            //        await EventLogArchive.UpdateAsync(searchLog);

            //        result.Add(searchLog);
            //    }
            //    else
            //    {
            //        await EventLogArchive.AddAsync(archive);
            //        result.Add(archive);
            //    }
            //}

            //return result;

            return default;
        }
    }
}
