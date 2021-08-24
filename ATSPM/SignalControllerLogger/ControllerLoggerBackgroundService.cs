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
using Utah.Gov.Udot.PipelineManager;

namespace ATSPM.SignalControllerLogger
{
    public class ControllerLoggerBackgroundService : BackgroundService
    {
        private readonly ILogger _log;
        private readonly IServiceProvider _serviceProvider;
        protected readonly IOptions<FileETLSettings> _options;

        PipelineManager _pipelineManager;

        private List<Signal> _signalList;

        public ControllerLoggerBackgroundService(ILogger<ControllerLoggerBackgroundService> log, IServiceProvider serviceProvider, IOptions<FileETLSettings> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<MOEContext>();
                _signalList = db.Signals.Take(10).Where(v => v.VersionActionId != 3).Include(i => i.ControllerType).AsNoTracking().AsEnumerable().GroupBy(r => r.SignalId).Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault()).ToList();
            }

            var downloader = _serviceProvider.GetService<ISignalControllerDownloader>();

            Progress<PipelineProgress> progress = new Progress<PipelineProgress>(p => Console.Write($"{p}"));

            _pipelineManager = new PipelineManager(stoppingToken);

            //add steps
            _pipelineManager.AddStep<Signal, DirectoryInfo>("FTPToDirectory", downloader, i => true, i => true);
            _pipelineManager.AddStep<DirectoryInfo, List<FileInfo>>("GetFilesFromDirectory", (i,c) => GetFilesFromDirectory(i), i => true, i => true);
            _pipelineManager.AddStep<List<FileInfo>, HashSet<ControllerEventLog>> ("DecodeFiles", (i,c) => ConvertFilesToEventLogs(i, c), i => i.Count > 0, i => true, progress);
            _pipelineManager.AddStep<HashSet<ControllerEventLog>, ControllerLogArchive>("CombineEventLogs", (i, c) => CombineEventLogs(i), i => true, i => true);

            //add pipes
            _pipelineManager.AddPipe<Signal>("SeedPipe", 0);
            _pipelineManager.AddPipe<DirectoryInfo>("FTPToDirectoryOutput");
            //_pipelineManager.AddPipe<DirectoryInfo>("FTPToDirectoryPostFail");
            _pipelineManager.AddPipe<List<FileInfo>>("FileListToDecoding");
            _pipelineManager.AddPipe<HashSet<ControllerEventLog>>("EventLogsToMerge");
            _pipelineManager.AddPipe<ControllerLogArchive>("MergedControllerLogArchive");

            //connect pipes
            _pipelineManager["FTPToDirectory"].Input = _pipelineManager.Pipes["SeedPipe"];
            _pipelineManager["FTPToDirectory"].Output = _pipelineManager.Pipes["FTPToDirectoryOutput"];
            //plm["FTPToDirectory1"].PostFailOutput = plm.Pipes["FTPToDirectoryPostFail"];

            _pipelineManager["GetFilesFromDirectory"].Input = _pipelineManager.Pipes["FTPToDirectoryOutput"];
            _pipelineManager["GetFilesFromDirectory"].Output = _pipelineManager.Pipes["FileListToDecoding"];

            _pipelineManager["DecodeFiles"].Input = _pipelineManager.Pipes["FileListToDecoding"];
            _pipelineManager["DecodeFiles"].Output = _pipelineManager.Pipes["EventLogsToMerge"];

            _pipelineManager["CombineEventLogs"].Input = _pipelineManager.Pipes["EventLogsToMerge"];
            _pipelineManager["CombineEventLogs"].Output = _pipelineManager.Pipes["MergedControllerLogArchive"];

            await SeedPipeline((PipelinePipe<Signal>)_pipelineManager.Pipes["SeedPipe"], _signalList, stoppingToken);

            await _pipelineManager.ExecuteAsync(null);

            Console.WriteLine($"Stopping======================================================================");
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

            //_log.LogDebug("Getting {FileCount} from Directory {Directory}", list.Count.ToString(), dir.Name);

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


                //value.MoveTo(Path.Combine(di.FullName, value.Name));
            }

            _log.LogDebug("--------------------------------------------Decoding {FileCount} - {LogCount} from Directory", input.Count.ToString(), logList.Count());

            return logList;
        }

        private Task<ControllerLogArchive> CombineEventLogs(HashSet<ControllerEventLog> input)
        {
            ControllerLogArchive result = null;

            HashSet<ControllerEventLog> eventLogs = new HashSet<ControllerEventLog>(new ControllerEventLogEqualityComparer());

            //IControllerEventLogRepository EventLogArchive = _serviceProvider.GetService<IControllerEventLogRepository>();

            //get and validate signalid
            string signalId = input.First().SignalId;
            if (!string.IsNullOrEmpty(signalId))
            {
                //using var scope = _serviceProvider.CreateScope();
                //var db = scope.ServiceProvider.GetRequiredService<MOEContext>();

                //group all logs by day
                foreach (var date in input.GroupBy(g => g.Timestamp.Date))
                {
                    //_log.LogWarning("eventLogs {SignalId} count {Count}", signalId, eventLogs.Count);
                    
                    
                    eventLogs = (HashSet<ControllerEventLog>)Enumerable.Union(eventLogs, date.ToHashSet(new ControllerEventLogEqualityComparer()));


                    ////see if there is already an entry in the db
                    //result = await db.ControllerLogArchives.FindAsync(signalId, date.Key.Date);


                    //////var archiveList = _archiveList.Where(l => l.SignalId == signalId && l.ArchiveDate == date.Key.Date).ToList();
                    //Console.WriteLine($"----------------retrieved archive logs: {result?.SignalId} - {result?.ArchiveDate}");

                    //if (result != null)
                    //{
                    //    HashSet<ControllerEventLog> eventLogs = new HashSet<ControllerEventLog>(result.LogData, new ControllerEventLogEqualityComparer());

                    //    HashSet<ControllerEventLog> union = new HashSet<ControllerEventLog>(Enumerable.Union(eventLogs, date.ToHashSet(new ControllerEventLogEqualityComparer()), new ControllerEventLogEqualityComparer()));

                    //    Console.WriteLine($"----------------decoded event logs: {eventLogs.Count}-{date.AsEnumerable().Count()} = {union.Count()} - huh?{union.Except(date.AsEnumerable()).FirstOrDefault()}");



                    //    result.LogData = union.ToList();

                    //    try
                    //    {
                    //        await db.SaveChangesAsync();
                    //    }
                    //    catch (InvalidOperationException)
                    //    {

                    //        Console.WriteLine($"InvalidOperationException-----------------------------------------{result.SignalId}");
                    //    }
                    //}
                    //else
                    //{
                    //    result = new ControllerLogArchive()
                    //    {
                    //        SignalId = signalId,
                    //        ArchiveDate = date.Key,
                    //        LogData = date.ToList()
                    //    };

                    //    await db.ControllerLogArchives.AddAsync(result);
                    //    await db.SaveChangesAsync();
                    //}
                }
            }

            return Task.FromResult(result);
        }
    }
}
