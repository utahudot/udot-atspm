using ATSPM.Application.Common.EqualityComparers;
using ATSPM.Application.Configuration;
using ATSPM.Application.Exceptions;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Data;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using ATSPM.Domain.Exceptions;
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

namespace ATSPM.Infrasturcture.Services.SignalControllerLoggers
{
    public class LegacySignalControllerLogger : SignalControllerLoggerBase
    {
        public event EventHandler CanExecuteChanged;

        private readonly ILogger _log;
        private readonly IOptions<SignalControllerLoggerConfiguration> _options;
        private readonly IServiceProvider _serviceProvider;

        public LegacySignalControllerLogger(ILogger<LegacySignalControllerLogger> log, IOptions<SignalControllerLoggerConfiguration> options, IServiceProvider serviceProvider) : base(log)
        {
            _log = log;
            _options = options;
            _serviceProvider = serviceProvider;
        }

        #region IExecuteWithProgress

        public override async Task<bool> ExecuteAsync(IList<Signal> parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            _log.LogDebug("ExecuteAsync {signalCount}", parameter?.Count);

            if (CanExecute(parameter))
            {
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                Console.WriteLine($"Starting: {sw.Elapsed}======================================================================");

                var stepOptions = new ExecutionDataflowBlockOptions()
                {
                    CancellationToken = cancelToken,
                    //NameFormat = blockName,
                    //MaxDegreeOfParallelism = Environment.ProcessorCount,
                    MaxDegreeOfParallelism = _options.Value.MaxDegreeOfParallelism,
                    //BoundedCapacity = capcity,
                    SingleProducerConstrained = true,
                    EnsureOrdered = false
                };

                //create steps
                var signalSender = new BufferBlock<Signal>(new DataflowBlockOptions() { CancellationToken = cancelToken, NameFormat = "Signal Buffer" });
                var downloader = CreateTransformManyStep<Signal, DirectoryInfo>(t => DownloadLogs(t, cancelToken), "DownloadFilesStep", stepOptions);
                var getFiles = CreateTransformManyStep<DirectoryInfo, FileInfo>(t => GetFiles(t), "GetFilesStep", stepOptions);
                var fileToLogs = CreateTransformManyStep<FileInfo, ControllerEventLog>(t => CreateEventLogs(t, cancelToken), "DecodeEventLogsStep", stepOptions);
                var logArchiveBatch = new BatchBlock<ControllerEventLog>(_options.Value.SaveToDatabaseBatchSize, new GroupingDataflowBlockOptions() { CancellationToken = cancelToken, NameFormat = "Archive Batch" });
                var saveToRepoTemp = CreateTransformManyStep<ControllerEventLog[], ControllerEventLog>(t => SaveToRepo(t, cancelToken), "SaveToRepo", stepOptions);
                var endResult = CreateActionStep<ControllerEventLog>(t => { }, "EndResultStep", stepOptions);

                //step linking
                signalSender.LinkTo(downloader, new DataflowLinkOptions() { PropagateCompletion = true });
                downloader.LinkTo(getFiles, new DataflowLinkOptions() { PropagateCompletion = true });
                getFiles.LinkTo(fileToLogs, new DataflowLinkOptions() { PropagateCompletion = true });
                fileToLogs.LinkTo(logArchiveBatch, new DataflowLinkOptions() { PropagateCompletion = true });
                logArchiveBatch.LinkTo(saveToRepoTemp, new DataflowLinkOptions() { PropagateCompletion = true });
                saveToRepoTemp.LinkTo(endResult, new DataflowLinkOptions() { PropagateCompletion = true });

                //group taks
                var steps = new List<IDataflowBlock>();

                steps.Add(signalSender);
                steps.Add(downloader);
                steps.Add(getFiles);
                steps.Add(fileToLogs);
                steps.Add(logArchiveBatch);
                steps.Add(saveToRepoTemp);
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
                    Console.WriteLine($"Completed: {sw.Elapsed}======================================================================");
                    sw.Stop();
                }

                return false;
            }
            else
            {
                throw new ExecuteException();
            }
        }

        #endregion

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

                //Console.WriteLine($"downloaded files: {signal.SignalId} - {signal.Ipaddress} - {fileList.Count}");

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

            //Console.WriteLine($"files in directory: {files.Count}");

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

            //Console.WriteLine($"Log Count: {logList.Count}");

            return logList;
        }

        protected async virtual Task<IEnumerable<ControllerEventLog>> SaveToRepo(ControllerEventLog[] logs, CancellationToken cancellationToken = default)
        {
            HashSet<ControllerEventLog> result = new HashSet<ControllerEventLog>(logs, new ControllerEventLogEqualityComparer());

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetService<EventLogContext>();


                    //await db.ControllerEventLogs.AddRangeAsync(result);
                    //var count = await db.SaveChangesAsync();

                    await db.BulkInsertOrUpdateAsync(result.ToList(),
                        new BulkConfig()
                        {
                            SqlBulkCopyOptions = Microsoft.Data.SqlClient.SqlBulkCopyOptions.CheckConstraints,
                            OmitClauseExistsExcept = true
                        },
                        null,
                        null,
                        cancellationToken);

                    //Console.WriteLine($"-------------------Write to database: incoming-{logs.Length} outgoing-{result.Count}");


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
