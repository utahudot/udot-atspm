using ATSPM.Application.Common.EqualityComparers;
using ATSPM.Application.Exceptions;
using ATSPM.Data.Models;
using ATSPM.Application.Repositories;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Application.ValueObjects;
using ATSPM.Domain.BaseClasses;
using ATSPM.Domain.Common;
using ATSPM.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Input;

namespace ATSPM.Application
{
    public class SignalControllerDataFlow : ServiceObjectBase, IExecuteAsyncWithProgress<IList<Signal>, bool, int>
    {
        public event EventHandler CanExecuteChanged;

        private readonly ILogger _log;
        private readonly IServiceProvider _serviceProvider;

        //private BufferBlock<Signal> _signalSender;
        //private IPropagatorBlock<Signal, DirectoryInfo> _downloader;
        //private IPropagatorBlock<DirectoryInfo, FileInfo> _getFiles;
        //private IPropagatorBlock<FileInfo, ControllerEventLog> _fileToLogs;
        //private BatchBlock<ControllerEventLog> _logArchiveBatch;
        //private IPropagatorBlock<ControllerEventLog[], ControllerLogArchive> _logsToArchive;

        public SignalControllerDataFlow(ILogger<SignalControllerDataFlow> log, IServiceProvider serviceProvider)
        {
            _log = log;
            _serviceProvider = serviceProvider;
        }



        #region IExecuteWithProgress

        public async Task<bool> ExecuteAsync(IList<Signal> parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            _log.LogDebug("ExecuteAsync {signalCount}", parameter?.Count);

            if (CanExecute(parameter))
            {
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                //Console.WriteLine($"Starting: {sw.Elapsed}======================================================================");


                var stepOptions = new ExecutionDataflowBlockOptions()
                {
                    CancellationToken = cancelToken,
                    //NameFormat = blockName,
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    //BoundedCapacity = capcity,
                    SingleProducerConstrained = true,
                    EnsureOrdered = false
                };

                var steps = new List<IDataflowBlock>();

                var signalSender = new BufferBlock<Signal>(new DataflowBlockOptions() { CancellationToken = cancelToken, NameFormat = "Signal Buffer" });
                var downloader = CreateTransformManyStep<Signal, DirectoryInfo>(t => DownloadLogs(t, cancelToken), "DownloadFilesStep", stepOptions);
                var getFiles = CreateTransformManyStep<DirectoryInfo, FileInfo>(t => GetFiles(t), "GetFilesStep", stepOptions);
                var fileToLogs = CreateTransformManyStep<FileInfo, ControllerEventLog>(t => CreateEventLogs(t, cancelToken), "DecodeEventLogsStep", stepOptions);
                var logArchiveBatch = new BatchBlock<ControllerEventLog>(50000, new GroupingDataflowBlockOptions() { CancellationToken = cancelToken, NameFormat = "Archive Batch" });
                var logsToArchive = CreateTransformManyStep<ControllerEventLog[], ControllerLogArchive>(t => ArchiveLogs(t), "ArchiveLogsStep", stepOptions);
                var saveToRepo = CreateTransformManyStep<ControllerLogArchive, ControllerLogArchive>(t => SaveToRepo(t, cancelToken), "SaveToRepo", stepOptions);

                //step linking
                signalSender.LinkTo(downloader, new DataflowLinkOptions() { PropagateCompletion = true });
                downloader.LinkTo(getFiles, new DataflowLinkOptions() { PropagateCompletion = true });
                getFiles.LinkTo(fileToLogs, new DataflowLinkOptions() { PropagateCompletion = true });
                fileToLogs.LinkTo(logArchiveBatch, new DataflowLinkOptions() { PropagateCompletion = true });
                logArchiveBatch.LinkTo(logsToArchive, new DataflowLinkOptions() { PropagateCompletion = true });
                logsToArchive.LinkTo(saveToRepo, new DataflowLinkOptions() { PropagateCompletion = true });

                var endResult = new ActionBlock<ControllerLogArchive>(i =>
                {
                    //if (i == null)
                    Console.WriteLine($"-----------------------------------------Wrote to repo {i.SignalId} - {i.ArchiveDate} - {i.LogData.Count}");

                }, stepOptions);

                saveToRepo.LinkTo(endResult, new DataflowLinkOptions() { PropagateCompletion = true });














                steps.Add(signalSender);
                steps.Add(downloader);
                steps.Add(getFiles);
                steps.Add(fileToLogs);
                steps.Add(logArchiveBatch);
                steps.Add(logsToArchive);
                steps.Add(saveToRepo);
                steps.Add(endResult);

                try
                {
                    foreach (var signal in parameter)
                    {
                        await signalSender.SendAsync(signal);
                    }

                    signalSender.Complete();

                    await Task.WhenAll(steps.Select(s => s.Completion));

                    return steps.All(t => t.Completion.IsCompletedSuccessfully);
                }
                catch (Exception e)
                {
                    _log.LogError(e, "ExecuteAsync exception");
                }
                finally
                {
                    sw.Stop();
                }

                return false;
            }
            else
            {
                throw new ExecuteException();
            }
        }

        public virtual bool CanExecute(IList<Signal> parameter)
        {
            return parameter?.Count > 0;
        }

        public async Task<bool> ExecuteAsync(IList<Signal> parameter, CancellationToken cancelToken = default)
        {
            if (parameter is IList<Signal> p)
                return await ExecuteAsync(p, default, cancelToken);
            else
                return false;
        }

        public async Task ExecuteAsync(object parameter)
        {
            if (parameter is IList<Signal> p)
                await ExecuteAsync(p, default, default);
        }

        bool ICommand.CanExecute(object parameter)
        {
            if (parameter is IList<Signal> p)
                return CanExecute(p);
            return false;
        }

        void ICommand.Execute(object parameter)
        {
            if (parameter is IList<Signal> p)
                Task.Run(() => ExecuteAsync(p, default, default));
        }

        #endregion

        private IPropagatorBlock<T1, T2> CreateTransformManyStep<T1, T2>(Func<T1, Task<IEnumerable<T2>>> process, string processName, ExecutionDataflowBlockOptions options = default)
        {
            options.NameFormat = processName;

            var block = new TransformManyBlock<T1, T2>(process, options);

            block.Completion.ContinueWith(t => _log.LogInformation(t.Exception, "{block} has completed: {status}", block.ToString(), t.Status), options.CancellationToken);

            return block;
        }

        private IPropagatorBlock<T1, T2> CreateTransformManyStep<T1, T2>(Func<T1, IEnumerable<T2>> process, string processName, ExecutionDataflowBlockOptions options = default)
        {
            options.NameFormat = processName;

            var block = new TransformManyBlock<T1, T2>(process, options);

            block.Completion.ContinueWith(t => _log.LogInformation(t.Exception, "{block} has completed: {status}", block.ToString(), t.Status), options.CancellationToken);

            return block;
        }

        protected async virtual Task<IEnumerable<DirectoryInfo>> DownloadLogs(Signal signal, CancellationToken cancellationToken = default)
        {
            var fileList = new List<FileInfo>();

            try
            {
                //var progress = new Progress<ControllerDownloadProgress>(p => _log.LogInformation(new EventId(Convert.ToInt32(s.SignalId)), "{progress}", p));

                using (var scope = _serviceProvider.CreateScope())
                {
                    var downloader = scope.ServiceProvider.GetServices<ISignalControllerDownloader>().First(c => c.CanExecute(signal));

                    //await foreach (var file in downloader.Execute(s, progress, cancellationToken))
                    await foreach (var file in downloader.Execute(signal, cancellationToken))
                    {
                        fileList.Add(file);
                    }
                }

                //_log.LogDebug(new EventId(Convert.ToInt32(s.SignalId)), "Completing step {step} on {signal}, downloaded {fileCount} files", blockName, s, fileList.Count);
            }
            catch (ExecuteException e)
            {
                //_log.LogError(new EventId(Convert.ToInt32(s.SignalId)), e, "{error}", e.Message);
            }
            catch (InvalidSignalControllerIpAddressException e)
            {
                _log.LogError(e, e.Message);
            }
            catch (ArgumentNullException e)
            {
                //_log.LogError(new EventId(Convert.ToInt32(s.SignalId)), e, "{error}", e.Message);
            }
            catch (Exception e)
            {
                //_log.LogError(new EventId(Convert.ToInt32(s.SignalId)), e, "Unexpected exception caught on step {step}", blockName);
            }

            return fileList.Select(s => s.Directory).Distinct(new LambdaEqualityComparer<DirectoryInfo>((x, y) => x.FullName == y.FullName));
        }

        protected virtual IEnumerable<FileInfo> GetFiles(DirectoryInfo directoryInfo)
        {
            var files = new List<FileInfo>();

            try
            {
                files = directoryInfo?.GetFiles("*.*", SearchOption.AllDirectories).ToList();
            }
            catch (Exception e)
            {
                _log.LogError(e, e.Message);
            }

            return files;
        }

        protected async virtual Task<IEnumerable<ControllerEventLog>> CreateEventLogs(FileInfo file, CancellationToken cancellationToken = default)
        {
            HashSet<ControllerEventLog> logList = new HashSet<ControllerEventLog>(new ControllerEventLogEqualityComparer());

            try
            {

                using (var scope = _serviceProvider.CreateScope())
                {
                    var decoder = scope.ServiceProvider.GetServices<ISignalControllerDecoder>().First(c => c.CanExecute(file));
                    logList = await decoder.ExecuteAsync(file, cancellationToken);
                }
            }
            catch (ExecuteException e)
            {
                //Console.WriteLine($"ExecuteException--------------------------------------------{blockName} catch: {e}");
            }
            catch (FileNotFoundException e)
            {
                //Console.WriteLine($"FileNotFoundException--------------------------------------------{blockName} catch: {e}");
            }
            catch (ArgumentNullException e)
            {
                //Console.WriteLine($"ArgumentNullException--------------------------------------------{blockName} catch: {e}");
            }
            catch (Exception e)
            {
                //Console.WriteLine($"Exception--------------------------------------------{blockName} catch: {e}");
            }

            return logList;
        }

        //TODO: Move into extension method
        protected virtual IEnumerable<ControllerLogArchive> ArchiveLogs(ControllerEventLog[] logs)
        {
            return logs.GroupBy(g => (g.Timestamp.Date, g.SignalId)).Select(s => new ControllerLogArchive() { SignalId = s.Key.SignalId, ArchiveDate = s.Key.Date, LogData = s.ToList() });
        }

        protected async virtual Task<IEnumerable<ControllerLogArchive>> SaveToRepo(ControllerLogArchive archive, CancellationToken cancellationToken = default)
        {
            List<ControllerLogArchive> result = new List<ControllerLogArchive>();

            try
            {
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
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e}");
            }

            return result;
        }
    }
}
