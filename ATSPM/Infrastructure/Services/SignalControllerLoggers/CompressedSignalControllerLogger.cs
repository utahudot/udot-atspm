using ATSPM.Application.Common.EqualityComparers;
using ATSPM.Application.Configuration;
using ATSPM.Application.Repositories;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using ATSPM.Domain.Exceptions;
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

namespace ATSPM.Infrastructure.Services.SignalControllerLoggers
{
    public class CompressedSignalControllerLogger : SignalControllerLoggerBase
    {
        //private readonly ILogger _log;
        private readonly IOptions<SignalControllerLoggerConfiguration> _options;
        private readonly IServiceProvider _serviceProvider;

        public CompressedSignalControllerLogger(ILogger<CompressedSignalControllerLogger> log, IOptions<SignalControllerLoggerConfiguration> options, IServiceProvider serviceProvider) : base(log)
        {
            _options = options;
            _serviceProvider = serviceProvider;
        }

        public override void Initialize()
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
            var downloader = CreateTransformManyStep<Signal, DirectoryInfo>(t => DownloadLogs(t, token), "DownloadFilesStep", stepOptions);
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

            base.Initialize();
        }

        #region IExecuteWithProgress

        //public override async Task<bool> ExecuteAsync(IList<Signal> parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        //{
        //    if (CanExecute(parameter))
        //    {
        //        var sw = new System.Diagnostics.Stopwatch();
        //        sw.Start();

        //        //logMessages.LoggerStartedMessage(DateTime.Now, parameter.Count);

        //        var stepOptions = new ExecutionDataflowBlockOptions()
        //        {
        //            CancellationToken = cancelToken,
        //            //NameFormat = blockName,
        //            //MaxDegreeOfParallelism = Environment.ProcessorCount,
        //            MaxDegreeOfParallelism = _options.Value.MaxDegreeOfParallelism,
        //            //BoundedCapacity = capcity,
        //            SingleProducerConstrained = true,
        //            EnsureOrdered = false
        //        };

        //        //create steps
        //        var signalSender = new BufferBlock<Signal>(new DataflowBlockOptions() { CancellationToken = cancelToken, NameFormat = "Signal Buffer" });
        //        var downloader = CreateTransformManyStep<Signal, DirectoryInfo>(t => DownloadLogs(t, cancelToken), "DownloadFilesStep", stepOptions);
        //        var getFiles = CreateTransformManyStep<DirectoryInfo, FileInfo>(t => GetFiles(t), "GetFilesStep", stepOptions);
        //        var fileToLogs = CreateTransformManyStep<FileInfo, ControllerEventLog>(t => CreateEventLogs(t, cancelToken), "DecodeEventLogsStep", stepOptions);
        //        var logArchiveBatch = new BatchBlock<ControllerEventLog>(_options.Value.SaveToDatabaseBatchSize, new GroupingDataflowBlockOptions() { CancellationToken = cancelToken, NameFormat = "Archive Batch" });
        //        var logsToArchive = CreateTransformManyStep<ControllerEventLog[], ControllerLogArchive>(t => ArchiveLogs(t), "ArchiveLogsStep", stepOptions);
        //        var saveToRepo = CreateTransformManyStep<ControllerLogArchive, ControllerLogArchive>(t => SaveToRepo(t, cancelToken), "SaveToRepo", stepOptions);
        //        var endResult = CreateActionStep<ControllerLogArchive>(t => Console.WriteLine($"Saved Logs!: {t}"), "EndResultStep", stepOptions);

        //        //step linking
        //        signalSender.LinkTo(downloader, new DataflowLinkOptions() { PropagateCompletion = true });
        //        downloader.LinkTo(getFiles, new DataflowLinkOptions() { PropagateCompletion = true });
        //        getFiles.LinkTo(fileToLogs, new DataflowLinkOptions() { PropagateCompletion = true });
        //        fileToLogs.LinkTo(logArchiveBatch, new DataflowLinkOptions() { PropagateCompletion = true });
        //        logArchiveBatch.LinkTo(logsToArchive, new DataflowLinkOptions() { PropagateCompletion = true });
        //        logsToArchive.LinkTo(saveToRepo, new DataflowLinkOptions() { PropagateCompletion = true });
        //        saveToRepo.LinkTo(endResult, new DataflowLinkOptions() { PropagateCompletion = true });

        //        //group taks
        //        var steps = new List<IDataflowBlock>();

        //        steps.Add(signalSender);
        //        steps.Add(downloader);
        //        steps.Add(getFiles);
        //        steps.Add(fileToLogs);
        //        steps.Add(logArchiveBatch);
        //        steps.Add(logsToArchive);
        //        steps.Add(saveToRepo);
        //        steps.Add(endResult);

        //        try
        //        {
        //            foreach (var signal in parameter)
        //            {
        //                await signalSender.SendAsync(signal);
        //            }

        //            signalSender.Complete();

        //            await Task.WhenAll(steps.Select(s => s.Completion));

        //            return steps.All(t => t.Completion.IsCompletedSuccessfully);
        //        }
        //        catch (Exception e)
        //        {
        //            //logMessages.LoggerExecutionException(new ControllerLoggerExecutionException(this, null, e));
        //        }
        //        finally
        //        {
        //            //logMessages.LoggerCompletedMessage(DateTime.Now, sw.Elapsed);
        //            sw.Stop();
        //        }

        //        return false;
        //    }
        //    else
        //    {
        //        throw new ExecuteException();
        //    }
        //}

        #endregion

        protected async virtual Task<IEnumerable<DirectoryInfo>> DownloadLogs(Signal signal, CancellationToken cancellationToken = default)
        {
            var fileList = new List<FileInfo>();

            using (var scope = _serviceProvider.CreateScope())
            {
                var downloader = scope.ServiceProvider.GetServices<ISignalControllerDownloader>().First(c => c.CanExecute(signal));

                //await foreach (var file in downloader.Execute(s, progress, cancellationToken))
                await foreach (var file in downloader.Execute(signal, cancellationToken))
                {
                    fileList.Add(file);
                }
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
                var decoder = scope.ServiceProvider.GetServices<ISignalControllerDecoder>().First(c => c.CanExecute(file));
                logList = await decoder.ExecuteAsync(file, cancellationToken);
            }

            return logList;
        }

        //TODO: Move into extension method
        protected virtual IEnumerable<ControllerLogArchive> ArchiveLogs(ControllerEventLog[] logs)
        {
            HashSet<ControllerEventLog> uniqueLogs = new HashSet<ControllerEventLog>(logs, new ControllerEventLogEqualityComparer());

            return uniqueLogs.GroupBy(g => (g.Timestamp.Date, g.SignalId)).Select(s => new ControllerLogArchive() { SignalId = s.Key.SignalId, ArchiveDate = s.Key.Date, LogData = s.ToList() });
        }

        protected async virtual Task<IEnumerable<ControllerLogArchive>> SaveToRepo(ControllerLogArchive archive, CancellationToken cancellationToken = default)
        {
            List<ControllerLogArchive> result = new List<ControllerLogArchive>();

            using (var scope = _serviceProvider.CreateScope())
            {
                IControllerEventLogRepository EventLogArchive = scope.ServiceProvider.GetService<IControllerEventLogRepository>();
                var searchLog = await EventLogArchive.LookupAsync(archive);

                if (searchLog != null)
                {
                    var eventLogs = new HashSet<ControllerEventLog>(Enumerable.Union(searchLog.LogData, archive.LogData), new ControllerEventLogEqualityComparer());
                    searchLog.LogData = eventLogs.ToList();

                    await EventLogArchive.UpdateAsync(searchLog);

                    result.Add(searchLog);
                }
                else
                {
                    await EventLogArchive.AddAsync(archive);
                    result.Add(archive);
                }
            }

            return result;
        }
    }
}
