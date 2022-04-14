using ATSPM.Application.Common;
using ATSPM.Application.Common.EqualityComparers;
using ATSPM.Application.Configuration;
using ATSPM.Application.Exceptions;
using ATSPM.Application.Models;
using ATSPM.Application.Repositories;
using ATSPM.Application.Services.SignalControllerProtocols;
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
using Utah.Gov.Udot.PipelineManager;

namespace ATSPM.SignalControllerLogger
{
    public class TPLDataflowService : BackgroundService
    {
        private readonly ILogger _log;
        private readonly IServiceProvider _serviceProvider;
        protected readonly IOptions<SignalControllerLoggerConfiguration> _options;

        private int _bufferSize = 10;

        private IReadOnlyList<Signal> _signalList;

        public TPLDataflowService(ILogger<TPLDataflowService> log, IServiceProvider serviceProvider, IOptions<SignalControllerLoggerConfiguration> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);

        
        private ISignalControllerDownloader DownloadSelector(Signal signal)
        {
            foreach (ISignalControllerDownloader d in _serviceProvider.GetServices<ISignalControllerDownloader>())
            {
                if (d.CanExecute(signal))
                    return d;
            }

            return null;
        }

        private ISignalControllerDecoder DecoderSelector(FileInfo file)
        {
            foreach (ISignalControllerDecoder d in _serviceProvider.GetServices<ISignalControllerDecoder>())
            {
                if (d.CanExecute(file))
                    return d;
            }

            return null;
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() => Console.WriteLine($"-------------------------------------------------------------------------------------------stoppingToken"));

            using (var scope = _serviceProvider.CreateScope())
            {
                _signalList = scope.ServiceProvider.GetService<ISignalRepository>().GetLatestVersionOfAllSignals().Where(w => w.Enabled).Take(500).ToList();
            }





            Console.WriteLine($"signal list count: {_signalList.Count}");



            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            Console.WriteLine($"Starting: {sw.Elapsed}======================================================================");


            


            //create initial buffer to send signals
            var signalSender = new BufferBlock<Signal>(new DataflowBlockOptions() { CancellationToken = stoppingToken, NameFormat = "Signal Buffer" });

            //create downloader step
            //var downloaders = CreateDownloaderBalencer(signalSender, resultAction, _signalList.Count, 10);
            var downloader = CreateDownloaderStep($"Downloader Step", 100, stoppingToken);
            

            //create Get files step
            var files = CreateGetFilesStep("Files Step", 100, stoppingToken);
            

            //create file to controllerlog step
            var fileToLogs = CreateFiletoEventLogStep("FileToLog Step", 100, stoppingToken);
            

            //create save to repo step
            var saveToRepo = CreateSaveToRepositoryStep("Save to Repo Step", 100, stoppingToken);


            //action for empty directories
            //var resultAction = new ActionBlock<DirectoryInfo>(s => Console.WriteLine($"{s?.FullName} is empty"));
            var resultAction = new ActionBlock<DirectoryInfo>(s => s?.Delete());



            //step linking
            signalSender.LinkTo(downloader, new DataflowLinkOptions { PropagateCompletion = true });


            var joinResults = new ActionBlock<FileInfo> (i => 
            {
                if (i == null)
                    Console.WriteLine($"%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%file? {i?.FullName}");
            });
            downloader.LinkTo(joinResults);


            //downloader.LinkTo(files, new DataflowLinkOptions { PropagateCompletion = true }, p => p?.GetFiles().Length > 0);
            //downloader.LinkTo(resultAction);

            //files.LinkTo(fileToLogs, new DataflowLinkOptions { PropagateCompletion = true }, p => p?.Count > 0);


            //fileToLogs.LinkTo(saveToRepo, new DataflowLinkOptions { PropagateCompletion = true });





            //Console.WriteLine($"downloaders count: {downloaders.Count()}");


            try
            {
                foreach (var signal in _signalList)
                {
                    //await signalSender.SendAsync(signal);
                    signalSender.Post(signal);
                }



                signalSender.Completion.ContinueWith(t => Console.WriteLine($"signalSender: {t.Status}"));

                downloader.Completion.ContinueWith(t => Console.WriteLine($"downloader: {t.Status}"));

                files.Completion.ContinueWith(t => Console.WriteLine($"files: {t.Status}"));

                fileToLogs.Completion.ContinueWith(t => Console.WriteLine($"fileToLogs: {t.Status}"));

                saveToRepo.Completion.ContinueWith(t => Console.WriteLine($"saveToRepo: {t.Status}"));





                signalSender.Complete();



                await signalSender.Completion;
                await downloader.Completion;
                //await files.Completion;
                //await fileToLogs.Completion;
                //await saveToRepo.Completion;

                //await Task.WhenAll(downloaders.Select(s => s.Completion).ToArray()).ContinueWith(t => resultAction.Complete(), stoppingToken);

                //await resultAction.Completion.ContinueWith(t => Console.WriteLine($"done?: {sw.Elapsed}"));
            }
            catch (Exception e)
            {
                Console.WriteLine($"***********************************{e.Message}*************************************************************");
            }
            finally
            {
                var dir = Directory.GetDirectories("C:\\ControlLogs").ToList();
                Console.WriteLine($"Stopping: {sw.Elapsed} - {dir.Count}/{_signalList.Count} ======================================================================");

                sw.Stop();
            }

            

            

            // TODO: dispose array of await tasks for flow stemps
            // TODO: run GC
        }

        private IPropagatorBlock<Signal, FileInfo> CreateDownloaderStep(string blockName, int capcity = -1, CancellationToken cancellationToken = default)
        {
            var block = new TransformManyBlock<Signal, FileInfo>(async s =>
            {
                if (int.TryParse(s.SignalId, out int id))
                {
                    //_log.LogDebug(new EventId(Convert.ToInt32(s.SignalId)), "Starting step {step} on {signal}", blockName, s);

                    var fileList = new List<FileInfo>();

                    try
                    {
                        //var progress = new Progress<ControllerDownloadProgress>(p => _log.LogInformation(new EventId(Convert.ToInt32(s.SignalId)), "{progress}", p));

                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var downloader = scope.ServiceProvider.GetServices<ISignalControllerDownloader>().First(c => c.CanExecute(s));

                            //await foreach (var file in downloader.Execute(s, progress, cancellationToken))
                            await foreach (var file in downloader.Execute(s, cancellationToken))
                            {
                                fileList.Add(file);
                            }
                        }

                        //_log.LogDebug(new EventId(Convert.ToInt32(s.SignalId)), "Completing step {step} on {signal}, downloaded {fileCount} files", blockName, s, fileList.Count);
                    }
                    catch (InvalidSignalControllerIpAddressException e)
                    {
                        //_log.LogError(new EventId(Convert.ToInt32(s.SignalId)), e, "{error}", e.Message);
                    }
                    catch (ArgumentNullException e)
                    {
                        _log.LogError(new EventId(Convert.ToInt32(s.SignalId)), e, "{error}", e.Message);
                    }
                    catch (Exception e)
                    {
                        _log.LogError(new EventId(Convert.ToInt32(s.SignalId)), e, "Unexpected exception caught on step {step}", blockName);
                    }

                    return fileList;
                }
                
                else
                {
                    Console.WriteLine($"###########################################################   {s}   ###########################################################");
                    return null;
                }

                

            }, new ExecutionDataflowBlockOptions()
            {
                CancellationToken = cancellationToken,
                NameFormat = blockName,
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                //BoundedCapacity = capcity,
                SingleProducerConstrained = true
            });

            return block;
        }

        private IPropagatorBlock<DirectoryInfo, List<FileInfo>> CreateGetFilesStep(string blockName, int capcity = -1, CancellationToken cancellationToken = default)
        {
            var block = new TransformBlock<DirectoryInfo, List<FileInfo>>(s =>
            {
                return s?.GetFiles("*.*", SearchOption.AllDirectories).ToList();

                ////Console.WriteLine($"trying to download: {s.SignalId}|{s.ControllerType.ControllerTypeId}");

                ////Console.WriteLine($"{blockName} is processing {s?.FullName}");

                //try
                //{
                //    List<FileInfo> list = new List<FileInfo>();

                //    if (s != null && s.Exists)
                //    {
                //        list = s.GetFiles("*.*", SearchOption.AllDirectories).ToList();
                //    }


                //    return await Task.FromResult(list);
                //}
                //catch (Exception ex)
                //{
                //    //Console.WriteLine($"--------------------------------------------{blockName} catch: {ex}");
                //}

                //return await Task.FromResult<List<FileInfo>>(null);

            }, new ExecutionDataflowBlockOptions()
            {
                CancellationToken = cancellationToken,
                NameFormat = blockName,
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                //BoundedCapacity = capcity,
                SingleProducerConstrained = true
            });

            return block;
        }

        private IPropagatorBlock<List<FileInfo>, HashSet<ControllerEventLog>> CreateFiletoEventLogStep(string blockName, int capcity = -1, CancellationToken cancellationToken = default)
        {
            var block = new TransformBlock<List<FileInfo>, HashSet<ControllerEventLog>>(async s =>
            {
                //Console.WriteLine($"trying to download: {s.SignalId}|{s.ControllerType.ControllerTypeId}");

                //Console.WriteLine($"{blockName} is processing {s.Count}");


                HashSet<ControllerEventLog> logList = new HashSet<ControllerEventLog>(new ControllerEventLogEqualityComparer());

                try
                {
                    foreach (var value in s)
                    {


                        var temp = await DecoderSelector(value).ExecuteAsync(value, cancellationToken).ConfigureAwait(false);

                        logList = new HashSet<ControllerEventLog>(Enumerable.Union(logList, temp), new ControllerEventLogEqualityComparer());

                        value.Delete();
                    }

                    return logList;

                    //_log.LogDebug("Loglist {Count}", logList.Count);
                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"--------------------------------------------{blockName} catch: {ex}");
                }

                return logList;

            }, new ExecutionDataflowBlockOptions()
            {
                CancellationToken = cancellationToken,
                NameFormat = blockName,
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                //BoundedCapacity = capcity,
                SingleProducerConstrained = true
            });

            return block;
        }

        private ITargetBlock<HashSet<ControllerEventLog>> CreateSaveToRepositoryStep(string blockName, int capcity = -1, CancellationToken cancellationToken = default)
        {
            var block = new ActionBlock<HashSet<ControllerEventLog>>(async s =>
            {
                List<ControllerLogArchive> result = new List<ControllerLogArchive>();

                var archiveDate = s.GroupBy(g => (g.Timestamp.Date, g.SignalId)).Select(s => new ControllerLogArchive() { SignalId = s.Key.SignalId, ArchiveDate = s.Key.Date, LogData = s.ToList() });

                try
                {
                    foreach (ControllerLogArchive archive in archiveDate)
                    {
                        //see if there is already an entry in the db
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            IControllerEventLogRepository EventLogArchive = scope.ServiceProvider.GetService<IControllerEventLogRepository>();
                            var searchLog = await EventLogArchive.LookupAsync(archive);

                            if (searchLog != null)
                            {
                                var eventLogs = new HashSet<ControllerEventLog>(Enumerable.Union(searchLog.LogData, archive.LogData), new ControllerEventLogEqualityComparer());

                                //Console.WriteLine($"Union logs: {eventLogs.Count} - {searchLog.LogData.Count()} = {archive.LogData.Count()}");

                                searchLog.LogData = eventLogs.ToList();

                                DbContext db = scope.ServiceProvider.GetService<DbContext>();
                                await db.SaveChangesAsync(cancellationToken);

                                result.Add(searchLog);
                            }
                            else
                            {
                                //add to database
                                await EventLogArchive.AddAsync(archive);
                                result.Add(archive);
                            }
                        }
                    }
                }
                catch (Exception)
                {

                }

                //Console.WriteLine($"{blockName} is processing {result.Count}");

                //_log.LogInformation($"result count: {result.Count}");

            }, new ExecutionDataflowBlockOptions()
            {
                CancellationToken = cancellationToken,
                NameFormat = blockName,
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                //BoundedCapacity = capcity,
                SingleProducerConstrained = true
            });

            return block;
        }

    }
}
