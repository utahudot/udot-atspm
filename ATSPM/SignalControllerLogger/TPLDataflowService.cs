using ATSPM.Application.Common;
using ATSPM.Application.Common.EqualityComparers;
using ATSPM.Application.Configuration;
using ATSPM.Application.Exceptions;
using ATSPM.Application.Models;
using ATSPM.Application.Repositories;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Domain.BaseClasses;
using ATSPM.Domain.Common;
using ATSPM.Domain.Exceptions;
using ATSPM.Infrasturcture.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Input;
using Utah.Gov.Udot.PipelineManager;

namespace ATSPM.SignalControllerLogger
{
    public static class DataFlowExtensions
    {
        public static IDisposable PropagateLink<T>(this ISourceBlock<T> source, ITargetBlock<T> target)
        {
            return source.LinkTo(target, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }

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
            //Console.WriteLine($"signal list count: {parameter?.Count}");

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
                var downloader = CreateDownloaderStep(stepOptions);
                var getFiles = CreateGetFilesStep(stepOptions);
                var fileToLogs = CreateFiletoEventLogStep(stepOptions);
                var logArchiveBatch = new BatchBlock<ControllerEventLog>(50000, new GroupingDataflowBlockOptions() { CancellationToken = cancelToken, NameFormat = "Archive Batch" });
                var logsToArchive = CreateLogArchiveStep(stepOptions);
                var saveToRepo = CreateSaveToRepositoryStep(stepOptions);

                //step linking
                signalSender.PropagateLink(downloader);
                downloader.PropagateLink(getFiles);
                getFiles.PropagateLink(fileToLogs);
                fileToLogs.PropagateLink(logArchiveBatch);
                logArchiveBatch.PropagateLink(logsToArchive);
                logsToArchive.PropagateLink(saveToRepo);

                var endResult = new ActionBlock<ControllerLogArchive>(i =>
                {
                    //if (i == null)
                    Console.WriteLine($"Wrote to repor {i.SignalId} - {i.ArchiveDate} - {i.LogData.Count}");
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

                    //_signalSender.Completion.ContinueWith(t => Console.WriteLine($"signalSender: {t.Status}"));
                    //_logArchiveBatch.Completion.ContinueWith(t => Console.WriteLine($"logArchiveBatch: {t.Status}"));
                    //endResult.Completion.ContinueWith(t => Console.WriteLine($"endResult:------------------------------------------ {t.Status}"));

                    signalSender.Complete();

                    await Task.WhenAll(steps.Select(s => s.Completion));

                    return steps.All(t => t.Completion.IsCompletedSuccessfully);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"***********************************{e.Message}*************************************************************");
                }
                finally
                {
                    var dir = Directory.GetDirectories("C:\\ControlLogs").ToList();
                    //Console.WriteLine($"Stopping: {sw.Elapsed} - {dir.Count}/{parameter?.Count} ======================================================================");

                    sw.Stop();
                }

                return false;
            }
            else
            {
                throw new ExecuteException();
            }
        }

        public bool CanExecute(IList<Signal> parameter)
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
            return default;
        }

        void ICommand.Execute(object parameter)
        {
            if (parameter is IList<Signal> p)
                Task.Run(() => ExecuteAsync(p, default, default));
        }

        #endregion

        private IPropagatorBlock<Signal, DirectoryInfo> CreateDownloaderStep(ExecutionDataflowBlockOptions options)
        {
            options.NameFormat = System.Reflection.MethodBase.GetCurrentMethod().Name;

            var block = new TransformManyBlock<Signal, DirectoryInfo>(async s =>
            {
                var fileList = new List<FileInfo>();

                try
                {
                    //var progress = new Progress<ControllerDownloadProgress>(p => _log.LogInformation(new EventId(Convert.ToInt32(s.SignalId)), "{progress}", p));

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var downloader = scope.ServiceProvider.GetServices<ISignalControllerDownloader>().First(c => c.CanExecute(s));

                        //await foreach (var file in downloader.Execute(s, progress, cancellationToken))
                        await foreach (var file in downloader.Execute(s, options.CancellationToken))
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
                    //_log.LogError(new EventId(Convert.ToInt32(s.SignalId)), e, "{error}", e.Message);
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

            }, options);

            block.Completion.ContinueWith(t => _log.LogInformation(t.Exception, "{block} has completed: {status}", block.ToString(), t.Status), options.CancellationToken);

            return block;
        }

        private IPropagatorBlock<DirectoryInfo, FileInfo> CreateGetFilesStep(ExecutionDataflowBlockOptions options)
        {
            options.NameFormat = System.Reflection.MethodBase.GetCurrentMethod().Name;

            var block = new TransformManyBlock<DirectoryInfo, FileInfo>(s =>
            {
                var files = new List<FileInfo>();

                try
                {
                    files = s?.GetFiles("*.*", SearchOption.AllDirectories).ToList();
                }
                catch (Exception e)
                {
                    _log.LogError("", e);
                }

                return files;

            }, options);

            block.Completion.ContinueWith(t => _log.LogInformation(t.Exception, "{block} has completed: {status}", block.ToString(), t.Status), options.CancellationToken);

            return block;
        }

        private IPropagatorBlock<FileInfo, ControllerEventLog> CreateFiletoEventLogStep(ExecutionDataflowBlockOptions options)
        {
            options.NameFormat = System.Reflection.MethodBase.GetCurrentMethod().Name;

            var block = new TransformManyBlock<FileInfo, ControllerEventLog>(async s =>
            {
                //Console.WriteLine($"trying to download: {s.SignalId}|{s.ControllerType.ControllerTypeId}");

                //Console.WriteLine($"{blockName} is processing {s.Count}");


                HashSet<ControllerEventLog> logList = new HashSet<ControllerEventLog>(new ControllerEventLogEqualityComparer());

                try
                {

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var decoder = scope.ServiceProvider.GetServices<ISignalControllerDecoder>().First(c => c.CanExecute(s));
                        logList = await decoder.ExecuteAsync(s, options.CancellationToken);
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

            }, options);

            block.Completion.ContinueWith(t => _log.LogInformation(t.Exception, "{block} has completed: {status}", block.ToString(), t.Status), options.CancellationToken);

            return block;
        }

        private IPropagatorBlock<ControllerEventLog[], ControllerLogArchive> CreateLogArchiveStep(ExecutionDataflowBlockOptions options)
        {
            options.NameFormat = System.Reflection.MethodBase.GetCurrentMethod().Name;

            var block = new TransformManyBlock<ControllerEventLog[], ControllerLogArchive>(s =>
            {
                return s.GroupBy(g => (g.Timestamp.Date, g.SignalId)).Select(s => new ControllerLogArchive() { SignalId = s.Key.SignalId, ArchiveDate = s.Key.Date, LogData = s.ToList() });

            }, options);

            block.Completion.ContinueWith(t => _log.LogInformation(t.Exception, "{block} has completed: {status}", block.ToString(), t.Status), options.CancellationToken);

            return block;
        }

        private IPropagatorBlock<ControllerLogArchive, ControllerLogArchive> CreateSaveToRepositoryStep(ExecutionDataflowBlockOptions options)
        {
            options.NameFormat = System.Reflection.MethodBase.GetCurrentMethod().Name;

            var block = new TransformManyBlock<ControllerLogArchive, ControllerLogArchive>(async s =>
            {
                List<ControllerLogArchive> result = new List<ControllerLogArchive>();

                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        IControllerEventLogRepository EventLogArchive = scope.ServiceProvider.GetService<IControllerEventLogRepository>();
                        var searchLog = await EventLogArchive.LookupAsync(s);

                        if (searchLog != null)
                        {
                            var eventLogs = new HashSet<ControllerEventLog>(Enumerable.Union(searchLog.LogData, s.LogData), new ControllerEventLogEqualityComparer());

                            //Console.WriteLine($"Union logs: {eventLogs.Count} - {searchLog.LogData.Count()} = {archive.LogData.Count()}");

                            searchLog.LogData = eventLogs.ToList();

                            await EventLogArchive.UpdateAsync(searchLog);

                            //DbContext db = scope.ServiceProvider.GetService<DbContext>();
                            //await db.SaveChangesAsync(options.CancellationToken);

                            result.Add(searchLog);
                        }
                        else
                        {
                            //add to database
                            await EventLogArchive.AddAsync(s);
                            result.Add(s);
                        }
                    }
                }
                catch (Exception)
                {

                }

                return result;

                //Console.WriteLine($"{blockName} is processing {result.Count}");

                //_log.LogInformation($"result count: {result.Count}");

            }, options);

            block.Completion.ContinueWith(t => _log.LogInformation(t.Exception, "{block} has completed: {status}", block.ToString(), t.Status), options.CancellationToken);

            return block;
        }

        //public IEnumerable<IPropagatorBlock<Signal, DirectoryInfo>> CreateDownloaderBalencer(ISourceBlock<Signal> source, ITargetBlock<DirectoryInfo> destination, int totalCount, int loadBalance)
        //{
        //    List<IPropagatorBlock<Signal, DirectoryInfo>> result = new List<IPropagatorBlock<Signal, DirectoryInfo>>();

        //    int blocksRequired = (totalCount / (loadBalance > 0 ? loadBalance : 1)) + (loadBalance > 0 ? ((totalCount % loadBalance) > 0 ? 1 : 0) : 0);

        //    for (int i = 1; i <= blocksRequired; i++)
        //    {
        //        var downloader = CreateDownloaderStep($"DownloaderIstance{i}", loadBalance == 0 ? -1 : loadBalance);

        //        source.LinkTo(downloader, new DataflowLinkOptions { PropagateCompletion = true });
        //        downloader.LinkTo(destination);

        //        result.Add(downloader);
        //    }

        //    return result;
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //_signalSender = null;
                //_downloader = null;
                //_getFiles = null;
                //_fileToLogs = null;
                //_logArchiveBatch = null;
                //_logsToArchive = null;
            }

            base.Dispose(disposing);
        }
    }

    public class TPLDataflowService : BackgroundService
    {
        private readonly ILogger _log;
        private readonly IServiceProvider _serviceProvider;
        protected readonly IOptions<SignalControllerLoggerConfiguration> _options;

        //private IReadOnlyList<Signal> _signalList;

        public TPLDataflowService(ILogger<TPLDataflowService> log, IServiceProvider serviceProvider, IOptions<SignalControllerLoggerConfiguration> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);

        
        

        

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //stoppingToken.Register(() => Console.WriteLine($"-------------------------------------------------------------------------------------------stoppingToken"));


            while(!stoppingToken.IsCancellationRequested)
            {
                //var testSignals = new List<string>() { "9704", "9705", "9721", "9741", "9709" };

                IReadOnlyList<Signal> _signalList;

                using (var scope = _serviceProvider.CreateScope())
                {
                    _signalList = scope.ServiceProvider.GetService<ISignalRepository>().GetLatestVersionOfAllSignals().Where(w => w.Enabled).Take(50).ToList();
                    //_signalList = scope.ServiceProvider.GetService<ISignalRepository>().GetLatestVersionOfAllSignals().Where(w => w.Enabled && w.SignalId == "9704").ToList();
                }

                using (var process = new SignalControllerDataFlow(_serviceProvider.GetService<ILogger<SignalControllerDataFlow>>(), _serviceProvider))
                {
                    //await process.ExecuteAsync(_signalList).ContinueWith(t => Console.WriteLine($"-----------------------it's done!!! {t.Status} --- {System.Diagnostics.Process.GetCurrentProcess().WorkingSet64}"));
                    await process.ExecuteAsync(_signalList);
                }

                //var process = new SignalControllerDataFlow(_serviceProvider.GetService<ILogger<SignalControllerDataFlow>>(), _serviceProvider);
                //await process.ExecuteAsync(_signalList);

                Console.WriteLine($"PRE: {System.Diagnostics.Process.GetCurrentProcess().WorkingSet64} - {System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64} - {GC.GetTotalMemory(false)}");

                GC.Collect();

                Console.WriteLine($"POST: {System.Diagnostics.Process.GetCurrentProcess().WorkingSet64} - {System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64} - {GC.GetTotalMemory(false)}");

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }


            // TODO: dispose array of await tasks for flow stemps
            // TODO: run GC
        }
    }
}
