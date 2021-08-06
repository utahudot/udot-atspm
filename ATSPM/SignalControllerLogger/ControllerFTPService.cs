using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Threading.Channels;
using ControllerLogger.Models;
using ControllerLogger.Data;
using System.Text.Json;
using ControllerLogger.Helpers;
using System.Diagnostics;
using ControllerLogger.Configuration;
using FluentFTP;
using System.Net;
using FluentFTP.Rules;
using System.Threading.Tasks.Dataflow;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using System.Net.Sockets;
using System.Linq.Expressions;

namespace ControllerLogger.Services
{
    public class ControllerFTPService : BackgroundService
    {
        private readonly ILogger _log;
        private readonly IServiceProvider _serviceProvider;
        protected readonly IOptions<ControllerFTPSettings> _options;

        public ControllerFTPService(ILogger<ControllerFTPService> log, IServiceProvider serviceProvider, IOptions<ControllerFTPSettings> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _log.LogInformation("object: {Object} method: {Method}", MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);

            List<Signal> signalList;

            var e = new Exception("this is the exception message!");

            using (var scope = _serviceProvider.CreateScope())
            {
                //signalList = _serviceProvider.GetService<MOEContext>().Signals.Where(i => i.Enabled == true).Include(i => i.ControllerType).ToList();
                var db = _serviceProvider.GetRequiredService<MOEContext>();
                signalList = await db.Signals.Where(v => v.VersionActionId != 3).Include(i => i.ControllerType).ToListAsync();
            }

            TempReader(StepOne(signalList, async s => await FTPConnectAsync1(s, stoppingToken), s => s.Ipaddress.IsValidIPAddress(true), stoppingToken));
        }

        private async Task PipelineStarter(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {


                //await Task.Delay(TimeSpan.FromMilliseconds(_options.Value.TimeOffset), stoppingToken);


                await Task.Delay(TimeSpan.FromMinutes(_options.Value.PollTime), stoppingToken);
            }
        }

        private ChannelReader<Tout> StepOne<Tin, Tout>(IList<Tin> input, Func<Tin, Task<Tout>> work, Predicate<Tin> predicate = default, CancellationToken stoppingToken = default, IProgress<double> progress = null)
        {
            //var options = new BoundedChannelOptions(100) { FullMode = BoundedChannelFullMode.Wait };
            //fromControllerPipe = Channel.CreateBounded<DirectoryInfo>(options);

            var output = Channel.CreateUnbounded<Tout>();

            //set predicate
            if (predicate == null) predicate = p => true;

            async Task Work()
            {
                //set progress
                progress?.Report(0);

                //set parallel options
                var opt = new ParallelOptions { MaxDegreeOfParallelism = 8, CancellationToken = stoppingToken };

                ParallelLoopResult result = default;

                try
                {
                    //run parallel loop with optional predicate
                    result = Parallel.ForEach(input, opt, async s =>
                    {
                        if (predicate != null && predicate(s))
                        {
                            //invoke delegate work function
                            var result = await work?.Invoke(s);

                            //write to output
                            if (await output.Writer.WaitToWriteAsync(stoppingToken) && result != null)
                                await output.Writer.WriteAsync(result);
                        }
                    });
                }
                catch (OperationCanceledException e) when (e.LogE()) { }
                catch (ArgumentNullException e) when (e.LogE()) { }
                catch (AggregateException e) when (e.LogE()) { }
                catch (ObjectDisposedException e) when (e.LogE()) { }
                finally
                {
                    _log.LogInformation("ParallelLoopResult: Complete-{Complete} Breakpoint-{Breakpoint}", result.IsCompleted, result.LowestBreakIteration);
                }

                //set progress
                progress?.Report(100);
            }

            Task.Run(async () =>
            {
                await Work();

                //complete writer
                output.Writer.TryComplete();

                _log.LogInformation("Method: { Object}method: { Method} is Complete", MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
            });

            //return output
            return output;
        }

        //https://github.com/robinrodricks/FluentFTP/wiki/Quick-Start-Example
        public ChannelReader<Tout> FTPConnectAsync<Tin, Tout>(Func<Task<Tout>> work, CancellationToken cancel = new CancellationToken())
        {
            var output = Channel.CreateUnbounded<Tout>();



            Task.Run(async () =>
            {
                await work.Invoke();
                output.Writer.Complete();
                //_log.LogInformation($"Step Complete: {nameof(FTPConnectAsync)}");
            });

            return output;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _log.LogError("object: {Object} method: {Method}", MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);

            await base.StopAsync(cancellationToken);
        }

        public async Task<DirectoryInfo> FTPConnectAsync1(Signal s, CancellationToken cancel)
        {
            using FtpClient client = new FtpClient(s.Ipaddress);
            // specify the login credentials, unless you want to use the "anonymous" user account
            client.Credentials = new NetworkCredential(s.ControllerType.UserName, s.ControllerType.Password);
            client.ConnectTimeout = 1000;
            client.ReadTimeout = 1000;
            //client.CheckCapabilities = true;
            if (s.ControllerType.ActiveFtp)
            {
                client.DataConnectionType = FtpDataConnectionType.AutoActive;
            }

            try
            {
                // begin connecting to the server
                //await client.ConnectAsync(cancel);
                await client.AutoConnectAsync(cancel);

                if (client.IsConnected && await client.DirectoryExistsAsync(s.ControllerType.Ftpdirectory))
                {
                    //foreach (var c in client.Capabilities)
                    //{
                    //    _log.LogInformation($"capabilities: {c}");
                    //}
                    
                    var rules = new List<FtpRule> { new FtpFileExtensionRule(true, new List<string> { "dat", "datz" }) };
                    //var progress = new Progress<FtpProgress>(p => _log.LogInformation($"log progress: {p.RemotePath}-{p.FileIndex}/{p.FileCount} - {p.Progress}"));
                    var results = await client.DownloadDirectoryAsync(Path.Combine(_options.Value.RootPath, s.SignalId), s.ControllerType.Ftpdirectory, FtpFolderSyncMode.Update, FtpLocalExists.Skip, FtpVerify.None, rules);

                    foreach (FtpResult r in results)
                    {
                        if (r.IsSuccess && r.IsDownload && !r.IsSkippedByRule)
                        {
                            _log.LogInformation(new EventId(Convert.ToInt32(s.SignalId)), r.Exception, "Success: file:{file} downloaded:{Downloaded} failed:{Failed} skipped:{skipped} success:{success}", r.Name, r.IsDownload, r.IsFailed, r.IsSkipped, r.IsSuccess);
                        }

                        if(r.IsFailed)
                        {
                            _log.LogWarning(new EventId(Convert.ToInt32(s.SignalId)), r.Exception, "Failed: file:{file} downloaded:{Downloaded} failed:{Failed} skipped:{skipped} success:{success}", r.Name, r.IsDownload, r.IsFailed, r.IsSkipped, r.IsSuccess);
                        }
                        
                        if (r.Exception != null)
                        {
                            r.Exception.LogE();
                        }
                        
                    }
                }
            }
            catch (FtpAuthenticationException e)
            {
                //_log.LogWarning(new EventId(Convert.ToInt32(s.SignalId)), e, e.Message);
            }
            catch (FtpCommandException e)
            {
                //_log.LogWarning(new EventId(Convert.ToInt32(s.SignalId)), e, e.Message);
            }
            catch (FtpException e)
            {
                //_log.LogWarning(new EventId(Convert.ToInt32(s.SignalId)), e, e.Message);
            }
            catch (SocketException e)
            {
                //_log.LogWarning(new EventId(Convert.ToInt32(s.SignalId)), e, e.Message);
            }
            catch (IOException e)
            {
                //_log.LogWarning(new EventId(Convert.ToInt32(s.SignalId)), e, e.Message);
            }
            catch (TimeoutException e)
            {
                //_log.LogWarning(new EventId(Convert.ToInt32(s.SignalId)), e, e.Message);
            }
            catch (Exception e)
            {
                _log.LogError(new EventId(Convert.ToInt32(s.SignalId)), e, e.Message);

                //throw;
            }
            finally
            {
                if (client.IsConnected)
                    await client.DisconnectAsync();
            }

            //get signal directory and verify that it exists
            var dir = new DirectoryInfo(Path.Combine(_options.Value.RootPath, s.SignalId));

            if (dir.Exists)
                return dir;
            else
                return null;
        }

        public void TempReader<T>(ChannelReader<T> reader)
        {
            async Task Work()
            {
                await foreach (var item in reader.ReadAllAsync())
                {
                    //_log.LogWarning($"Reading: {item}");
                    //await Task.Delay(TimeSpan.FromMilliseconds(500));
                }
            }

            Task.Run(async () =>
            {
                await Work();
                
                _log.LogInformation($"Step Complete: {nameof(TempReader)}");
            });
        }

        

        public ChannelReader<KeyValuePair<Signal, List<ControllerEventLog>>> DecodeStep(ChannelReader<KeyValuePair<Signal, FtpListItem>> reader)
        {
            var output = Channel.CreateUnbounded<KeyValuePair<Signal, List<ControllerEventLog>>>();

            async Task Work()
            {
                await foreach (var item in reader.ReadAllAsync())
                {
                    List<ControllerEventLog> logList = new List<ControllerEventLog>();

                    try
                    {
                        logList.AddRange(Asc3Decoder.DecodeAsc3File(item.Value.FullName, item.Key.SignalId, new DateTime(1979, 7, 29)));
                    }
                    catch (Exception)
                    {

                        throw;
                    }


                    if (logList.Count > 0)
                        await output.Writer.WriteAsync(new KeyValuePair<Signal, List<ControllerEventLog>>(item.Key, logList));
                }
            }

            Task.Run(async () =>
            {
                await Work();
                output.Writer.Complete();
                _log.LogInformation($"Step Complete: {nameof(DecodeStep)}");
            });

            return output;
        }
    }
}
