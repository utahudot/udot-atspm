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
using Microsoft.EntityFrameworkCore;
using FluentFTP;
using System.Net;
using FluentFTP.Rules;
using System.Net.Sockets;
using System.Diagnostics.CodeAnalysis;
using ControllerLogger.Domain.Extensions;
using ControllerLogger.Application.Common.EqualityComparers;
using ControllerLogger.Application.Services;
using ControllerLogger.Application.Enums;
using ControllerLogger.Application.Configuration;
using ControllerLogger.Domain.BaseClasses;

namespace ControllerLogger.Services
{
    public class ControllerLoggerHostService : IHostedService
    {
        public ControllerLoggerHostService(ILogger<FileETLHostedService> log, IServiceProvider serviceProvider, IOptions<FileETLSettings> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);

        #region Fields

        private readonly ILogger _log;
        private readonly IServiceProvider _serviceProvider;
        protected readonly IOptions<FileETLSettings> _options;

        //private MOEContext _db;
        private List<Signal> _signalList;
        //private List<ControllerLogArchive> _archiveList;

        #endregion

        #region IHostedService

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //_log.LogInformation("object: {Object} method: {Method}", MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
            Console.WriteLine($"Starting======================================================================{cancellationToken.GetHashCode()}");

            //TODO: this will really pull in the repository
            
            using (var scope = _serviceProvider.CreateScope())
            {
                //signalList = _serviceProvider.GetService<MOEContext>().Signals.Where(i => i.Enabled == true).Include(i => i.ControllerType).ToList();
                var db = scope.ServiceProvider.GetRequiredService<MOEContext>();
                _signalList = db.Signals.Where(v => v.VersionActionId != 3).Include(i => i.ControllerType).AsNoTracking().AsEnumerable().GroupBy(r => r.SignalId).Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault()).ToList();
            }

            //var downloader = _serviceProvider.GetService<ISignalControllerDownloader>();


            IPipelineExecute<Signal, DirectoryInfo> TestSelector(ISignalControllerDownloader input) => input switch
            {
                ISignalControllerDownloader c when c.ControllerType == SignalControllerType.ASC3 => c,
                ISignalControllerDownloader c when c.ControllerType == SignalControllerType.ASC32070 => c,
                ISignalControllerDownloader c when c.ControllerType == SignalControllerType.Cobalt => c,
                ISignalControllerDownloader c when c.ControllerType == SignalControllerType.EOS => c,
                ISignalControllerDownloader c when c.ControllerType == SignalControllerType.MaxTime => c,
                ISignalControllerDownloader c when c.ControllerType == SignalControllerType.McCainATCEX => c,
                ISignalControllerDownloader c when c.ControllerType == SignalControllerType.Peek => c,
                ISignalControllerDownloader c when c.ControllerType == SignalControllerType.SiemensSEPAC => c,
                ISignalControllerDownloader c when c.ControllerType == SignalControllerType.Trafficware => c,
                ISignalControllerDownloader c when c.ControllerType == SignalControllerType.Unknown => c,
                _ => throw new NotImplementedException(),
            };

            var taskGroup = new PipelineTaskGroup<Signal, DirectoryInfo>(TaskGroupType.RunFirst);

            foreach (var d in _serviceProvider.GetServices<ISignalControllerDownloader>())
            {
                taskGroup.AddTask(d);
            }

            Console.WriteLine($"TaskGroup Count: {taskGroup.Count}");

            //_archiveList = await _db.ControllerLogArchives.Where(l => l.ArchiveDate == DateTime.Now.Date || l.ArchiveDate == DateTime.Now.AddDays(-1).Date).ToListAsync();

            //Progress<FtpProgress> FTPProgress = new Progress<FtpProgress>(p =>
            //{
            //    Console.WriteLine($"FTPProgress: Path:{p.LocalPath} - RemotePath:{p.RemotePath} - FileCount:{p.FileCount} - FileIndex:{p.FileIndex} - Progress:{p.Progress}");
            //});

            Progress<PipelineProgress> stepOneProgress = new Progress<PipelineProgress>(p => Console.WriteLine($"StepOneProgress: {p}"));
            Progress<PipelineProgress> stepTwoProgress = new Progress<PipelineProgress>(p => Console.WriteLine($"stepTwoProgress: {p}"));
            //Progress<PipelineProgress> stepThreeProgress = new Progress<PipelineProgress>(p => Console.WriteLine($"stepThreeProgress: {p}"));
            //Progress<PipelineProgress> setpFourProgress = new Progress<PipelineProgress>(p => Console.WriteLine($"setpFourProgress: {p}"));

            //add pipeline manager
            PipelineManager plm = new PipelineManager(cancellationToken);

            //add steps
            //plm.AddStep<DirectoryInfo, bool>("HandleBadDirectories", i => HandleBadDirectories(i), i => true, i => true);

            plm.AddStep<Signal, DirectoryInfo>("FTPToDirectory1", taskGroup, i => taskGroup.CanExecute(i), i => true, stepOneProgress);
            plm.AddStep<Signal, DirectoryInfo>("FTPToDirectory2", taskGroup, i => taskGroup.CanExecute(i), i => true, stepTwoProgress);

            //plm.AddStep<Signal, DirectoryInfo>("FTPToDirectory", downloader, i => true, i => true, stepOneProgress);

            //plm.AddStep<DirectoryInfo, List<FileInfo>>("GetFilesFromDirectory", i => GetFilesFromDirectory(i), i => true, i => true);
            //plm.AddStep<List<FileInfo>, HashSet<ControllerEventLog>>("ConvertFilesToEventLogs", i => ConvertFilesToEventLogs(i), i => true, i => i.Count > 0);
            //plm.AddStep<HashSet<ControllerEventLog>, ControllerLogArchive>("CombineEventLogs", async i => await CombineEventLogs(i), i => true, i => true);

            //add pipes
            plm.AddPipe<Signal>("StarterPipe");
            plm.AddPipe<DirectoryInfo>("FTPToDirectoryOutput");
            plm.AddPipe<DirectoryInfo>("FTPToDirectoryPostFail");
            //plm.AddPipe<List<FileInfo>>("FileListToDecoding");
            //plm.AddPipe<HashSet<ControllerEventLog>>("EventLogsToMerge");
            //plm.AddPipe<ControllerLogArchive>("MergedControllerLogArchive");

            //connect pipes
            plm["FTPToDirectory1"].Input = plm.Pipes["StarterPipe"];
            plm["FTPToDirectory1"].Output = plm.Pipes["FTPToDirectoryOutput"];
            plm["FTPToDirectory1"].PostFailOutput = plm.Pipes["FTPToDirectoryPostFail"];

            plm["FTPToDirectory2"].Input = plm.Pipes["StarterPipe"];
            plm["FTPToDirectory2"].Output = plm.Pipes["FTPToDirectoryOutput"];
            plm["FTPToDirectory2"].PostFailOutput = plm.Pipes["FTPToDirectoryPostFail"];

            //plm["GetFilesFromDirectory"].Input = plm.Pipes["FTPToDirectoryOutput"];
            //plm["GetFilesFromDirectory"].Output = plm.Pipes["FileListToDecoding"];

            //plm["ConvertFilesToEventLogs"].Input = plm.Pipes["FileListToDecoding"];
            //plm["ConvertFilesToEventLogs"].Output = plm.Pipes["EventLogsToMerge"];

            //plm["CombineEventLogs"].Input = plm.Pipes["EventLogsToMerge"];
            //plm["CombineEventLogs"].Output = plm.Pipes["MergedControllerLogArchive"];

            if (plm.Pipes["StarterPipe"] is PipelinePipe<Signal> writer)
            {
                foreach (Signal value in _signalList)
                {
                    await writer.Writer.WriteAsync(value);
                }

                writer.Writer.TryComplete();
            }


            //plm["HandleBadDirectories"].Input = plm.Pipes["FTPToDirectoryPostFail"];



            //OutputReader((PipelinePipe<List<ControllerEventLog>>)plm.Pipes["EventLogsToMerge"]).FireAndForget();
            //PreFailReader<int>((PipelinePipe<int>)plm.Pipes["prefailpipe"]);
            //PostFaileReader((PipelinePipe<DirectoryInfo>)plm.Pipes["FTPToDirectoryPostFail"]).FireAndForget();


            //execute pipeline
            await plm.Execute(null);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            //_log.LogInformation("object: {Object} method: {Method}", MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
            Console.WriteLine($"Stopping======================================================================{cancellationToken.GetHashCode()}");
            return Task.CompletedTask;
        }

        #endregion

        public async Task<DirectoryInfo> FTPConnectAsync(Signal s, CancellationToken cancel = default, IProgress<FtpProgress> progress = default)
        {

            //return directory
            DirectoryInfo dir = null;

            if (s.ControllerType.Ftpdirectory == null)
                return dir;

            using FtpClient client = new FtpClient(s.Ipaddress);
            client.Credentials = new NetworkCredential(s.ControllerType.UserName, s.ControllerType.Password);
            client.ConnectTimeout = 1000;
            client.ReadTimeout = 1000;
            //client.CheckCapabilities = true;
            if (s.ControllerType.ActiveFtp)
            {
                client.DataConnectionType = FtpDataConnectionType.AutoActive;
            }

            //all results go into this list
            List<FtpResult> results = new List<FtpResult>();

            try
            {
                // begin connecting to the server
                //await client.ConnectAsync(cancel);
                await client?.AutoConnectAsync(cancel);


            }
            //catch (FtpAuthenticationException e) when (e.LogE()) { }
            catch (FtpAuthenticationException)
            {
                dir ??= new DirectoryInfo(Path.Combine(_options.Value.RootPath, "FtpAuthenticationException", s.SignalId));
            }
            //catch (FtpCommandException e) when (e.LogE()) { }
            //catch (FtpException e) when (e.LogE()) { }
            //catch (SocketException e) when (e.LogE()) { }
            //catch (IOException e) when (e.LogE()) { }
            catch (TimeoutException)
            {
                //this keeps triggering on each event for some reason
            }
            catch (Exception e)
            {
                dir ??= new DirectoryInfo(Path.Combine(_options.Value.RootPath, e.GetType().ToString(), s.SignalId));
            }
            //catch (Exception e)
            //{
            //    _log.LogError(new EventId(Convert.ToInt32(s.SignalId)), e, e.Message);

            //    throw;
            //}
            //finally
            //{
            //    if (client.IsConnected)
            //        await client.DisconnectAsync();
            //}

            try
            {
                if (client.IsConnected && await client.DirectoryExistsAsync(s.ControllerType.Ftpdirectory))
                {
                    //make sure there are valid files
                    var items = await client.GetListingAsync(s.ControllerType.Ftpdirectory, cancel);
                    if (items.Where(i => i.Name.Contains("dat")).Count() > 0)
                    {
                        //download directory with filter
                        var rules = new List<FtpRule> { new FtpFileExtensionRule(true, new List<string> { "dat", "datz" }) };
                        results = await client.DownloadDirectoryAsync(Path.Combine(_options.Value.RootPath, s.SignalId), s.ControllerType.Ftpdirectory, FtpFolderSyncMode.Update, FtpLocalExists.Overwrite, FtpVerify.Retry, rules, progress, cancel);
                    }
                }

                //HACK:
                //else
                //{
                //    dir = new DirectoryInfo(Path.Combine(_options.Value.RootPath, "NoDirectory", s.SignalId));
                //}
            }
            catch (Exception e)
            {

                Console.WriteLine($"CONNECTED EXCEPTIONS: {e.GetType()}");
            }

            //close connection
            if (client.IsConnected)
                await client.DisconnectAsync();

            if (results.Count > 0)
            {
                //process results
                foreach (FtpResult r in results)
                {
                    if (r.IsSuccess && r.IsDownload && !r.IsSkippedByRule)
                    {
                        //_log.LogInformation(new EventId(Convert.ToInt32(s.SignalId)), r.Exception, "Success: file:{file} downloaded:{Downloaded} failed:{Failed} skipped:{skipped} success:{success}", r.Name, r.IsDownload, r.IsFailed, r.IsSkipped, r.IsSuccess);
                    }

                    if (r.IsFailed)
                    {
                        //_log.LogWarning(new EventId(Convert.ToInt32(s.SignalId)), r.Exception, "Failed: file:{file} downloaded:{Downloaded} failed:{Failed} skipped:{skipped} success:{success}", r.Name, r.IsDownload, r.IsFailed, r.IsSkipped, r.IsSuccess);
                    }

                    if (r.Exception != null)
                    {
                        //r.Exception.LogE(LogLevel.Error);
                        //if (r.Exception.InnerException != null)
                    }

                    //else
                    //{
                    //    _log.LogError(new EventId(Convert.ToInt32(s.SignalId)), r.Exception, "WHAT?:{file} downloaded:{Downloaded} failed:{Failed} skipped:{skipped} success:{success}", r.Name, r.IsDownload, r.IsFailed, r.IsSkipped, r.IsSuccess);
                    //}
                }

                //at least one file had a failure
                if (results.Any(r => r.IsFailed || r.Exception != null))
                {
                    dir ??= new DirectoryInfo(Path.Combine(_options.Value.RootPath, "ErrorOrFailedException", s.SignalId));
                }

                //all files were return successfully
                else
                {
                    dir ??= new DirectoryInfo(Path.Combine(_options.Value.RootPath, s.SignalId));
                }
            }
            //else
            //{
            //    dir = new DirectoryInfo(Path.Combine(_options.Value.RootPath, "NoUsableFiles", s.SignalId));
            //}

            //could not successfully connect and download required files
            dir ??= new DirectoryInfo(Path.Combine(_options.Value.RootPath, "NoUsableFilesException", s.SignalId));

            if (!dir.Exists)
                dir.Create();

            return dir;
        }

        #region Methods

        public async Task<DirectoryInfo> TestProcess(int input)
        {
            Random rnd = new Random();
            await Task.Delay(TimeSpan.FromSeconds(rnd.Next(1, 5)));

            //if (input == 77)
            //    cts.Cancel();

            //token.ThrowIfCancellationRequested();

            Console.WriteLine($"Executing Task - {input}");


            return new DirectoryInfo(_options.Value.RootPath);
        }

        public async Task OutputReader<T>(PipelinePipe<T> reader)
        {
            await foreach (var item in reader.Reader.ReadAllAsync())
            {
                Console.WriteLine($"-------------------------------------------------------------------------{nameof(OutputReader)} - {item}");
            }
        }

        public async Task PreFailReader<T>(PipelinePipe<T> reader)
        {
            await foreach (var item in reader.Reader.ReadAllAsync())
            {
                Console.WriteLine($"-------------------------------------------------------------------------{nameof(PreFailReader)} - {item}");
            }
        }

        public async Task PostFaileReader<T>(PipelinePipe<T> reader)
        {
            await foreach (var item in reader.Reader.ReadAllAsync())
            {
                Console.WriteLine($"-------------------------------------------------------------------------{nameof(PostFaileReader)} - {item}");
            }
        }

        public Task<DirectoryInfo> FTPConnectAsyncTest(Signal s)
        {
            DirectoryInfo dir = new DirectoryInfo(_options.Value.RootPath);
            DirectoryInfo d = new DirectoryInfo(Path.Combine(dir.FullName, s.SignalId));

            var folders = dir.GetDirectories("", SearchOption.AllDirectories);

            if (!folders.Contains(d))
            {
                d.Create();
            }

            return d.AsTask();
        }

        public Task<List<FileInfo>> GetFilesFromDirectory(DirectoryInfo dir)
        {
            List<FileInfo> list = new List<FileInfo>();

            if (dir.Exists)
            {
                list = dir.GetFiles("*.*", SearchOption.AllDirectories).ToList();
            }

            return list.AsTask();
        }

        public Task<HashSet<ControllerEventLog>> ConvertFilesToEventLogs(List<FileInfo> input, CancellationToken cancel = default)
        {
            var di = Directory.CreateDirectory(Path.Combine(_options.Value.RootPath, "DecodedFiles"));
            List<ControllerEventLog> logList = new List<ControllerEventLog>();
            
            foreach (var value in input)
            {
                try
                {
                    logList.AddRange(Asc3Decoder.DecodeAsc3File(value.FullName, value.Directory.Name, _options.Value.EarliestAcceptableDate));

                    value.MoveTo(Path.Combine(di.FullName, value.Name));
                }
                catch (Exception)
                {

                    throw;
                }
            }

            return new HashSet<ControllerEventLog>(logList, new ControllerEventLogEqualityComparer()).AsTask();
        }

        public async Task<ControllerLogArchive> CombineEventLogs(HashSet<ControllerEventLog> input)
        {
            //make sure list isn't empty
            if (input.Count > 0)
            {
                //get and validate signalid
                string signalId = input.First().SignalId;
                if (!string.IsNullOrEmpty(signalId))
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        //signalList = _serviceProvider.GetService<MOEContext>().Signals.Where(i => i.Enabled == true).Include(i => i.ControllerType).ToList();
                        var db = scope.ServiceProvider.GetRequiredService<MOEContext>();

                        //group all logs by day
                        foreach (var date in input.GroupBy(g => g.Timestamp.Date))
                        {
                            //see if there is already an entry in the db
                            var archive = await db.ControllerLogArchives.FindAsync(signalId, date.Key.Date);

                            //var archiveList = _archiveList.Where(l => l.SignalId == signalId && l.ArchiveDate == date.Key.Date).ToList();
                            Console.WriteLine($"----------------retrieved archive logs: {archive?.SignalId} - {archive?.ArchiveDate}");

                            if (archive != null)
                            {
                                HashSet<ControllerEventLog> eventLogs = new HashSet<ControllerEventLog>(JsonSerializer.Deserialize<List<ControllerEventLog>>(archive.LogData.GZipDecompressToString()), new ControllerEventLogEqualityComparer());

                                HashSet<ControllerEventLog> union = new HashSet<ControllerEventLog>(Enumerable.Union(eventLogs, date.ToHashSet(new ControllerEventLogEqualityComparer()), new ControllerEventLogEqualityComparer()));

                                Console.WriteLine($"----------------decoded event logs: {eventLogs.Count}-{date.AsEnumerable().Count()} = {union.Count()} - huh?{union.Except(date.AsEnumerable()).FirstOrDefault()}");



                                archive.LogData = JsonSerializer.Serialize(union.Select(s => new { s.Timestamp, s.EventCode, s.EventParam }).AsEnumerable().Select(s => new Tuple<DateTime, int, int>(s.Timestamp, s.EventCode, s.EventParam)).ToList()).GZipCompressToByte();

                                try
                                {
                                    await db.SaveChangesAsync();
                                }
                                catch (InvalidOperationException)
                                {

                                    Console.WriteLine($"InvalidOperationException-----------------------------------------{archive.SignalId}");
                                }


                                return archive;
                            }
                            else
                            {
                                ControllerLogArchive cla = new ControllerLogArchive()
                                {
                                    SignalId = signalId,
                                    ArchiveDate = date.Key,
                                    LogData = JsonSerializer.Serialize(date.Select(s => new { s.Timestamp, s.EventCode, s.EventParam }).AsEnumerable().Select(s => new Tuple<DateTime, int, int>(s.Timestamp, s.EventCode, s.EventParam)).ToList()).GZipCompressToByte()
                                };

                                await db.ControllerLogArchives.AddAsync(cla);
                                await db.SaveChangesAsync();
                            }
                        }
                    }
                }
            }


            return await new ControllerLogArchive().AsTask();
        }

        public Task<bool> HandleBadDirectories(DirectoryInfo dir)
        {
            var di = Directory.CreateDirectory(Path.Combine(_options.Value.RootPath, "DeletedItems"));

            if (dir.Exists)
            {
                dir.MoveTo(Path.Combine(di.FullName, dir.Name));

                //dir.Delete();

                return true.AsTask();
            }

            //var parent = dir.Parent;
            //if (parent.GetDirectories().Count() == 0)
            //    di.Delete();


            return false.AsTask();
        }

        #endregion
    }

    public class SignalControllerDownloaderTestA : ISignalControllerDownloader
    {
        private readonly ILogger _log;
        private readonly IServiceProvider _serviceProvider;
        protected readonly IOptions<SignalControllerDownloaderConfiguration> _options;


        public SignalControllerDownloaderTestA(ILogger<SignalControllerDownloaderTestA> log, IServiceProvider serviceProvider, IOptions<SignalControllerDownloaderConfiguration> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);


        public SignalControllerType ControllerType => SignalControllerType.EOS;

        public bool CanExecute(Signal value)
        {
            //_log.LogWarning($"{value.ControllerType.ControllerTypeId} - {Convert.ToInt32(ControllerType)} - {value.ControllerType.ControllerTypeId == Convert.ToInt32(ControllerType)}");
            return (value.ControllerType.ControllerTypeId == Convert.ToInt32(ControllerType));
        }

        public Task<DirectoryInfo> Execute(Signal input, CancellationToken cancelToken = default, IProgress<PipelineProgress> progress = null)
        {
            _log.LogWarning(new EventId(Convert.ToInt32(input.SignalId)), $"Controller Type: {input.ControllerType.Description} - {input.ControllerType.Ftpdirectory}");
            DirectoryInfo dir = null;
            return dir.AsTask();
        }

        bool IPipelineExecute.CanExecute(object param)
        {
            if (param is Signal p)
                return CanExecute(p);
            return false;
        }

        Task IPipelineExecute.Execute(object param)
        {
            if (param is Signal p)
                return Execute(p);
            return default;
        }
    }

    public class ASCSignalControllerDownloader : ServiceObjectBase, ISignalControllerDownloader
    {
        #region Fields

        private readonly ILogger _log;
        private readonly IServiceProvider _serviceProvider;
        protected readonly IOptions<SignalControllerDownloaderConfiguration> _options;

        #endregion

        public ASCSignalControllerDownloader(ILogger<ASCSignalControllerDownloader> log, IServiceProvider serviceProvider, IOptions<SignalControllerDownloaderConfiguration> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);

        #region Properties

        public SignalControllerType ControllerType => SignalControllerType.ASC3;

        #endregion

        #region Methods

        public override void Initialize()
        {
            //using FtpClient client = new FtpClient(s.Ipaddress);
            //client.Credentials = new NetworkCredential(s.ControllerType.UserName, s.ControllerType.Password);
            //client.ConnectTimeout = 1000;
            //client.ReadTimeout = 1000;
            //if (s.ControllerType.ActiveFtp)
            //{
            //    client.DataConnectionType = FtpDataConnectionType.AutoActive;
            //}
        }

        #region IPipelineExecute<Tin, Tout>

        public bool CanExecute(Signal value)
        {
            //check valid controller type
            return (value.ControllerType.ControllerTypeId == Convert.ToInt32(ControllerType))

            //check directory
            && !string.IsNullOrEmpty(value.ControllerType.Ftpdirectory)

            //check valid ipaddress
            && value.Ipaddress.IsValidIPAddress(true);
        }

        public async Task<DirectoryInfo> Execute(Signal input, CancellationToken cancelToken = default, IProgress<PipelineProgress> progress = null)
        {
            //return directory
            DirectoryInfo dir = null;

            _log.LogInformation(new EventId(Convert.ToInt32(input.SignalId)), $"Controller Type: {input.ControllerType.Description} - {input.ControllerType.Ftpdirectory}");

            if (CanExecute(input))
            {
                using FtpClient client = new FtpClient(input.Ipaddress);
                client.Credentials = new NetworkCredential(input.ControllerType.UserName, input.ControllerType.Password);
                client.ConnectTimeout = 1000;
                client.ReadTimeout = 1000;
                if (input.ControllerType.ActiveFtp)
                {
                    client.DataConnectionType = FtpDataConnectionType.AutoActive;
                }

                //all results go into this list
                List<FtpResult> results = new List<FtpResult>();

                try
                {
                    await client?.AutoConnectAsync(cancelToken);
                }
                //catch (FtpAuthenticationException e) when (e.LogE()) { }
                catch (FtpAuthenticationException)
                {
                    dir ??= new DirectoryInfo(Path.Combine(_options.Value.LocalPath, "FtpAuthenticationException", input.SignalId));
                }
                //catch (FtpCommandException e) when (e.LogE()) { }
                //catch (FtpException e) when (e.LogE()) { }
                //catch (SocketException e) when (e.LogE()) { }
                //catch (IOException e) when (e.LogE()) { }
                catch (TimeoutException e)
                {
                    _log.LogError(new EventId(Convert.ToInt32(input.SignalId)), e, "TimeoutException");
                }
                catch (Exception e)
                {
                    dir ??= new DirectoryInfo(Path.Combine(_options.Value.LocalPath, e.GetType().ToString(), input.SignalId));
                }
                //catch (Exception e)
                //{
                //    _log.LogError(new EventId(Convert.ToInt32(s.SignalId)), e, e.Message);

                //    throw;
                //}
                finally
                {
                    if (client.IsConnected)
                        await client.DisconnectAsync();
                }
            }

            return dir;
        }

        #endregion

        #region IPipelineExecute
        bool IPipelineExecute.CanExecute(object param)
        {
            if (param is Signal p)
                return CanExecute(p);
            return false;
        }

        Task IPipelineExecute.Execute(object param)
        {
            if (param is Signal p)
                return Execute(p);
            return default;
        }
        #endregion

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}