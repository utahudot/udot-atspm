using ATSPM.Application.Common.EqualityComparers;
using ATSPM.Application.Configuration;
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

namespace ATSPM.SignalControllerLogger
{
    public class ControllerLoggerBackgroundService : BackgroundService
    {
        private readonly ILogger _log;
        private readonly IServiceProvider _serviceProvider;
        protected readonly IOptions<SignalControllerLoggerConfiguration> _options;

        PipelineManager _pipelineManager;

        private IReadOnlyList<Signal> _signalList;

        public ControllerLoggerBackgroundService(ILogger<ControllerLoggerBackgroundService> log, IServiceProvider serviceProvider, IOptions<SignalControllerLoggerConfiguration> options) =>
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
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                //var db = scope.ServiceProvider.GetRequiredService<DbContext>();
                //_signalList = db.Set<Signal>().Where(v => v.VersionActionId != 3).Include(i => i.ControllerType).AsNoTracking().AsEnumerable().GroupBy(r => r.SignalId).Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault()).ToList();

                _signalList = scope.ServiceProvider.GetService<ISignalRepository>().GetLatestVersionOfAllSignals();
            }

            Progress<PipelineProgress> progress = new Progress<PipelineProgress>(p => Console.Write($"{p}"));

            _pipelineManager = new PipelineManager(stoppingToken);

            //add steps
            //_pipelineManager.AddStep<Signal, DirectoryInfo>("FTPToDirectory",(i, c) => DownloadSelector(i).ExecuteAsync(i,c), i => true, i => true);
            _pipelineManager.AddStep<DirectoryInfo, List<FileInfo>>("GetFilesFromDirectory", (i,c) => GetFilesFromDirectory(i), i => i.Parent.Name == "ControlLogs", i => i.Count > 0);
            _pipelineManager.AddStep<List<FileInfo>, HashSet<ControllerEventLog>> ("DecodeFiles", (i,c) => ConvertFilesToEventLogs(i, c), i => i.Count > 0, i => true);
            _pipelineManager.AddStep<HashSet<ControllerEventLog>, List<ControllerLogArchive>>("CombineEventLogs", (i, c) => CombineEventLogs(i, c), i => true, i => true);

            //_pipelineManager.AddStep<List<ControllerLogArchive>, DirectoryInfo>("CleanupDirectories", (i, c) => CleanupDirectories(i, c), i => true, i => true);

            //add pipes
            _pipelineManager.AddPipe<Signal>("SeedPipe", 0);
            _pipelineManager.AddPipe<DirectoryInfo>("FTPToDirectoryOutput");
            //_pipelineManager.AddPipe<DirectoryInfo>("FTPToDirectoryPostFail");
            _pipelineManager.AddPipe<List<FileInfo>>("FileListToDecoding");
            _pipelineManager.AddPipe<HashSet<ControllerEventLog>>("EventLogsToMerge");
            _pipelineManager.AddPipe<List<ControllerLogArchive>>("MergedControllerLogArchive");
            _pipelineManager.AddPipe<DirectoryInfo>("DeletedDirectories");

            //connect pipes
            _pipelineManager["FTPToDirectory"].Input = _pipelineManager.Pipes["SeedPipe"];
            _pipelineManager["FTPToDirectory"].Output = _pipelineManager.Pipes["FTPToDirectoryOutput"];

            _pipelineManager["GetFilesFromDirectory"].Input = _pipelineManager.Pipes["FTPToDirectoryOutput"];
            _pipelineManager["GetFilesFromDirectory"].Output = _pipelineManager.Pipes["FileListToDecoding"];

            _pipelineManager["DecodeFiles"].Input = _pipelineManager.Pipes["FileListToDecoding"];
            _pipelineManager["DecodeFiles"].Output = _pipelineManager.Pipes["EventLogsToMerge"];

            _pipelineManager["CombineEventLogs"].Input = _pipelineManager.Pipes["EventLogsToMerge"];
            _pipelineManager["CombineEventLogs"].Output = _pipelineManager.Pipes["MergedControllerLogArchive"];

            //_pipelineManager["CleanupDirectories"].Input = _pipelineManager.Pipes["MergedControllerLogArchive"];
            //_pipelineManager["CleanupDirectories"].Output = _pipelineManager.Pipes["DeletedDirectories"];

            await SeedPipeline((PipelinePipe<Signal>)_pipelineManager.Pipes["SeedPipe"], _signalList, stoppingToken);


            var sw = new System.Diagnostics.Stopwatch();

            sw.Start();

            Console.WriteLine($"Starting: {sw.Elapsed}======================================================================");

            await _pipelineManager.ExecuteAsync(null);

            Console.WriteLine($"Stopping: {sw.Elapsed}======================================================================");

            sw.Stop();
        }

        private async Task<bool> SeedPipeline<T>(PipelinePipe<T> pipe, IEnumerable<T> seeds, CancellationToken cancellationToken = default)
        {
            foreach (T seed in seeds)
            {
                if (await pipe.Writer.WaitToWriteAsync(cancellationToken))
                {
                    await pipe.Writer.WriteAsync(seed);

                    //_log.LogDebug("Seeding pipeling: {Seed}", seed);
                }
            }

            //_log.LogDebug("Seeding complete");
            return pipe.Writer.TryComplete();
        }

        private Task<List<FileInfo>> GetFilesFromDirectory(DirectoryInfo dir)
        {
            List<FileInfo> list = new List<FileInfo>();

            if (dir != null && dir.Exists)
            {
                list = dir.GetFiles("*.*", SearchOption.AllDirectories).ToList();
            }

            _log.LogDebug("Getting {FileCount} from Directory {Directory}", list.Count.ToString(), dir.FullName);

            return Task.FromResult(list);
        }

        private async Task<HashSet<ControllerEventLog>> ConvertFilesToEventLogs(List<FileInfo> input, CancellationToken cancellationToken = default)
        {
            //var di = Directory.CreateDirectory(Path.Combine(_options.Value.RootPath, "DecodedFiles"));
            HashSet<ControllerEventLog> logList = new HashSet<ControllerEventLog>(new ControllerEventLogEqualityComparer());
            var decoder = _serviceProvider.GetService<ISignalControllerDecoder>();

            foreach (var value in input)
            {
                var temp = await decoder.ExecuteAsync(value, cancellationToken);

                logList = new HashSet<ControllerEventLog>(Enumerable.Union(logList, temp), new ControllerEventLogEqualityComparer());

                value.Delete();
            }

            _log.LogDebug("Loglist {Count}", logList.Count);

            return logList;
        }

        private async Task<List<ControllerLogArchive>> CombineEventLogs(HashSet<ControllerEventLog> input, CancellationToken cancellationToken = default)
        {
            List<ControllerLogArchive> result = new List<ControllerLogArchive>();

            var archiveDate = input.GroupBy(g => (g.Timestamp.Date, g.SignalId)).Select(s => new ControllerLogArchive() { SignalId = s.Key.SignalId, ArchiveDate = s.Key.Date, LogData = s.ToList() });

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

            _log.LogInformation($"result count: {result.Count}");

            return result;
        }

        //private Task<DirectoryInfo> CleanupDirectories(List<ControllerLogArchive> input, CancellationToken cancellationToken = default)
        //{
        //    DirectoryInfo dir = null;

        //    if (input.Count > 0)
        //    {
        //        _log.LogWarning($"Directory: {_options.Value.RootPath} - {input.FirstOrDefault().SignalId}");

        //        dir = new DirectoryInfo(Path.Combine(_options.Value.RootPath, input.FirstOrDefault().SignalId));

        //        //_log.LogWarning($"Directory: {dir.FullName}");

        //        if (dir.Exists && dir.GetFiles().Count() == 0)
        //        {
        //            dir.Delete();

        //            //dir.MoveTo(Path.Combine(_options.Value.RootPath, "DELETED", dir.Name));
        //        }
        //    }
            
        //    return Task.FromResult(dir);
        //}
    }
}
