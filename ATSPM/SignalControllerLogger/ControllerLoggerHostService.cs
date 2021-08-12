using ATSPM.Application.Common.EqualityComparers;
using ATSPM.Application.Configuration;
using ATSPM.Application.Enums;
using ATSPM.Application.Models;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Domain.Extensions;
using ATSPM.Infrasturcture.Data;
using ATSPM.Infrasturcture.Services.ControllerDecoders;
using FluentFTP;
using FluentFTP.Rules;
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
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Utah.Gov.Udot.PipelineManager;

namespace ATSPM.SignalControllerLogger
{
    public class ControllerLoggerHostService : IHostedService
    {
        public ControllerLoggerHostService(ILogger<ControllerLoggerHostService> log, IServiceProvider serviceProvider, IOptions<FileETLSettings> options) =>
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

            var downloader = _serviceProvider.GetService<ISignalControllerDownloader>();


            //IPipelineExecute<Signal, DirectoryInfo> TestSelector(ISignalControllerDownloader input) => input switch
            //{
            //    ISignalControllerDownloader c when c.ControllerType == SignalControllerType.ASC3 => c,
            //    ISignalControllerDownloader c when c.ControllerType == SignalControllerType.ASC32070 => c,
            //    ISignalControllerDownloader c when c.ControllerType == SignalControllerType.Cobalt => c,
            //    ISignalControllerDownloader c when c.ControllerType == SignalControllerType.EOS => c,
            //    ISignalControllerDownloader c when c.ControllerType == SignalControllerType.MaxTime => c,
            //    ISignalControllerDownloader c when c.ControllerType == SignalControllerType.McCainATCEX => c,
            //    ISignalControllerDownloader c when c.ControllerType == SignalControllerType.Peek => c,
            //    ISignalControllerDownloader c when c.ControllerType == SignalControllerType.SiemensSEPAC => c,
            //    ISignalControllerDownloader c when c.ControllerType == SignalControllerType.Trafficware => c,
            //    ISignalControllerDownloader c when c.ControllerType == SignalControllerType.Unknown => c,
            //    _ => throw new NotImplementedException(),
            //};

            //var taskGroup = new PipelineTaskGroup<Signal, DirectoryInfo>(TaskGroupType.RunFirst);

            //foreach (var d in _serviceProvider.GetServices<ISignalControllerDownloader>())
            //{
            //    taskGroup.AddTask(d);
            //}

            //Console.WriteLine($"TaskGroup Count: {taskGroup.Count}");

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

            plm.AddStep<Signal, DirectoryInfo>("FTPToDirectory", downloader, i => true, i => true);
            plm.AddStep<DirectoryInfo, List<FileInfo>>("GetFilesFromDirectory", i => GetFilesFromDirectory(i), i => true, i => true);

            //plm.AddStep<List<FileInfo>, HashSet<ControllerEventLog>>("ConvertFilesToEventLogs", i => ConvertFilesToEventLogs(i), i => true, i => i.Count > 0);
            //plm.AddStep<HashSet<ControllerEventLog>, ControllerLogArchive>("CombineEventLogs", async i => await CombineEventLogs(i), i => true, i => true);

            //add pipes
            plm.AddPipe<Signal>("StarterPipe", 0);
            plm.AddPipe<DirectoryInfo>("FTPToDirectoryOutput");
            //plm.AddPipe<DirectoryInfo>("FTPToDirectoryPostFail");
            plm.AddPipe<List<FileInfo>>("FileListToDecoding");
            //plm.AddPipe<HashSet<ControllerEventLog>>("EventLogsToMerge");
            //plm.AddPipe<ControllerLogArchive>("MergedControllerLogArchive");

            //connect pipes
            plm["FTPToDirectory"].Input = plm.Pipes["StarterPipe"];
            plm["FTPToDirectory"].Output = plm.Pipes["FTPToDirectoryOutput"];
            //plm["FTPToDirectory1"].PostFailOutput = plm.Pipes["FTPToDirectoryPostFail"];

            plm["GetFilesFromDirectory"].Input = plm.Pipes["FTPToDirectoryOutput"];
            plm["GetFilesFromDirectory"].Output = plm.Pipes["FileListToDecoding"];

            //plm["ConvertFilesToEventLogs"].Input = plm.Pipes["FileListToDecoding"];
            //plm["ConvertFilesToEventLogs"].Output = plm.Pipes["EventLogsToMerge"];

            //plm["CombineEventLogs"].Input = plm.Pipes["EventLogsToMerge"];
            //plm["CombineEventLogs"].Output = plm.Pipes["MergedControllerLogArchive"];


            


            //plm["HandleBadDirectories"].Input = plm.Pipes["FTPToDirectoryPostFail"];



            OutputReader((PipelinePipe<List<FileInfo>>)plm.Pipes["FileListToDecoding"]).FireAndForget();
            //PreFailReader<int>((PipelinePipe<int>)plm.Pipes["prefailpipe"]);
            //PostFaileReader((PipelinePipe<DirectoryInfo>)plm.Pipes["FTPToDirectoryPostFail"]).FireAndForget();


            //execute pipeline
            plm.Execute(null);

            if (plm.Pipes["StarterPipe"] is PipelinePipe<Signal> writer)
            {
                foreach (Signal value in _signalList)
                {
                    //if (writer.Writer.TryWrite(value))
                    //{
                    //    //await writer.Writer.WriteAsync(value);
                    //}
                    if (await writer.Writer.WaitToWriteAsync(cancellationToken))
                    {
                        await writer.Writer.WriteAsync(value);
                    }
                }

                writer.Writer.TryComplete();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            //_log.LogInformation("object: {Object} method: {Method}", MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
            Console.WriteLine($"Stopping======================================================================{cancellationToken.GetHashCode()}");
            return Task.CompletedTask;
        }

        #endregion

        #region Methods

        public async Task<DirectoryInfo> TestProcess(Signal input)
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

            if (dir != null && dir.Exists)
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
                    //logList.AddRange(Asc3Decoder.DecodeAsc3File(value.FullName, value.Directory.Name, _options.Value.EarliestAcceptableDate));

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
                    using var scope = _serviceProvider.CreateScope();
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
}