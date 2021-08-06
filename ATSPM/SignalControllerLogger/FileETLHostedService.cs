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
using System.Diagnostics;
using ControllerLogger.Configuration;
using ControllerLogger.Domain.Extensions;

namespace ControllerLogger.Services
{
    public class FileETLHostedService : IHostedService
    {
        private readonly ILogger _log;
        private readonly IServiceProvider _serviceProvider;
        protected readonly IOptions<FileETLSettings> _options;

        private Stopwatch sw = new Stopwatch();

        private Channel<DirectoryInfo> DeleteChannel = Channel.CreateUnbounded<DirectoryInfo>();
        private Channel<KeyValuePair<DirectoryInfo, List<FileInfo>>> UnprocessedChannel = Channel.CreateUnbounded<KeyValuePair<DirectoryInfo, List<FileInfo>>>();

        private MOEContext db;

        public FileETLHostedService(ILogger<FileETLHostedService> log, IServiceProvider serviceProvider, IOptions<FileETLSettings> options) => 
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);

        public Task StartAsync(CancellationToken cancellationToken)
        {
            sw.Start();

            _log.LogInformation("object: {Object} method: {Method}", MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);

            db = _serviceProvider.GetRequiredService<MOEContext>();

            //setup delete and unproccessed
            DeleteDirectories(DeleteChannel);
            UnprocessedItems(UnprocessedChannel);

            //start pipline
            StepFour(StepThree(StepTwo(StepOne(DeleteChannel), UnprocessedChannel)), DeleteChannel);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            //_log.LogInformation("object: {Object} method: {Method}", MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);
            return Task.CompletedTask;
        }


        public void DeleteDirectories(ChannelReader<DirectoryInfo> deleteChannel)
        {
            var dir = Directory.CreateDirectory(Path.Combine(_options.Value.RootPath, "DeletedItems"));

            async Task Work()
            {
                await foreach (var item in deleteChannel.ReadAllAsync())
                {
                    //_log.LogWarning($"Delete Directory: {item.Name}");

                    item.MoveTo(Path.Combine(dir.FullName,item.Name));
                }
            };

            Task.Run(async () =>
            {
                await Work();
                _log.LogInformation($"Step Complete: {nameof(DeleteDirectories)}");
            });
        }

        public void UnprocessedItems(ChannelReader<KeyValuePair<DirectoryInfo, List<FileInfo>>> unprocessedChannel)
        {
            var dir = Directory.CreateDirectory(Path.Combine(_options.Value.RootPath, "UnProcessedItems"));

            async Task Work()
            {
                await foreach (var item in unprocessedChannel.ReadAllAsync())
                {
                    //_log.LogWarning($"Unprocessed Directory: {item.Key.Name}");

                    item.Key.MoveTo(Path.Combine(dir.FullName, item.Key.Name));
                }
            };

            Task.Run(async () =>
            {
                await Work();
                _log.LogInformation($"Step Complete: {nameof(UnprocessedItems)}");
            });
        }



        //TODO: make sure directory name is actually a valid signal id before its sent down the pipe
        //Step One: get all correct files in directory and put them in queue
        //https://deniskyashif.com/2020/01/07/csharp-channels-part-3/
        public ChannelReader<KeyValuePair<DirectoryInfo, List<FileInfo>>> StepOne(ChannelWriter<DirectoryInfo> deleteChannel)
        {
            var output = Channel.CreateUnbounded<KeyValuePair<DirectoryInfo, List<FileInfo>>>();
            DirectoryInfo dir = new DirectoryInfo(_options.Value.RootPath);

            var folders = dir.GetDirectories("", SearchOption.AllDirectories);

            async Task Work()
            {
                foreach (DirectoryInfo d in folders)
                {
                    if (int.TryParse(d.Name, out _))
                    {
                        KeyValuePair<DirectoryInfo, List<FileInfo>> kvp = new KeyValuePair<DirectoryInfo, List<FileInfo>>(d, d.GetFiles("*.*", SearchOption.AllDirectories).ToList());

                        //if directory is empty, delete. otherwise write to output
                        if (kvp.Value.Count > 0)
                            await output.Writer.WriteAsync(kvp);
                        else
                            await deleteChannel.WriteAsync(kvp.Key);
                    }
                }
            }

            Task.Run(async () =>
            {
                await Work();
                output.Writer.Complete();
                _log.LogInformation($"Step Complete: {nameof(StepOne)}");
            });

            return output;
        }

        
        public ChannelReader<KeyValuePair<DirectoryInfo, List<ControllerEventLog>>> StepTwo(ChannelReader<KeyValuePair<DirectoryInfo, List<FileInfo>>> reader, ChannelWriter<KeyValuePair<DirectoryInfo, List<FileInfo>>> unprocessedChannel)
        {
            var output = Channel.CreateUnbounded<KeyValuePair<DirectoryInfo, List<ControllerEventLog>>>();

            async Task Work()
            {
                await foreach (var item in reader.ReadAllAsync())
                {
                    List<ControllerEventLog> logList = new List<ControllerEventLog>();

                    //_log.LogWarning($"item: {item.Key.Name}-{item.Value.Count}");

                    foreach (var value in item.Value)
                    {
                        try
                        {
                            logList.AddRange(Asc3Decoder.DecodeAsc3File(value.FullName, item.Key.Name, _options.Value.EarliestAcceptableDate));
                        }
                        catch (Exception)
                        {

                            throw;
                        }
                        //_log.LogWarning($"decoded logs: {item.Key.Name}-{logs.Count}");
                    }

                    
                    if (logList.Count > 0)
                        await output.Writer.WriteAsync(new KeyValuePair<DirectoryInfo, List<ControllerEventLog>>(item.Key, logList));
                    else
                        await unprocessedChannel.WriteAsync(item);
                }
            }

            Task.Run(async () =>
            {
                await Work();
                output.Writer.Complete();
                unprocessedChannel.TryComplete();
                _log.LogInformation($"Step Complete: {nameof(StepTwo)}");
            });

            return output;
        }

        public ChannelReader<KeyValuePair<DirectoryInfo,ControllerLogArchive>> StepThree(ChannelReader<KeyValuePair<DirectoryInfo, List<ControllerEventLog>>> reader)
        {
            var output = Channel.CreateUnbounded<KeyValuePair<DirectoryInfo, ControllerLogArchive>>();

            var db = _serviceProvider.GetRequiredService<MOEContext>();

            async Task Work()
            {
                await foreach (var item in reader.ReadAllAsync())
                {
                    //_log.LogWarning($"item: {item.Key.Name}-{item.Value.Count}");
                    foreach (var date in item.Value.GroupBy(g => g.Timestamp.Date))
                    {
                        //ControllerLogArchive cla = db.ControllerLogArchives.Where(s => s.SignalId == item.Key.Name && s.ArchiveDate == date.Key).First();

                        ControllerLogArchive cla;

                        //if (cla != null)
                        //_log.LogWarning($"cla: {cla.SignalId} - {cla.ArchiveDate}");
                        //else
                        //{
                        cla = new ControllerLogArchive()
                            {
                                SignalId = item.Key.Name,
                                ArchiveDate = date.Key,
                                LogData = JsonSerializer.Serialize(date.Select(s => new { s.Timestamp, s.EventCode, s.EventParam }).AsEnumerable().Select(s => new Tuple<DateTime, int, int>(s.Timestamp, s.EventCode, s.EventParam)).ToList()).GZipCompressToByte()
                            };

                        //_log.LogError($"ControllerLogArchive: {cla.SignalId} - {cla.ArchiveDate} - {cla.LogData.Length}");

                        await output.Writer.WriteAsync(new KeyValuePair<DirectoryInfo, ControllerLogArchive>(item.Key,cla));
                        //}
                    }

                }
            }

            Task.Run(async () =>
            {
                await Work();
                output.Writer.Complete();
                _log.LogInformation($"Step Complete: {nameof(StepThree)}");
            });

            return output;
        }

        public void StepFour(ChannelReader<KeyValuePair<DirectoryInfo, ControllerLogArchive>> reader, ChannelWriter<DirectoryInfo> deleteChannel)
        {
            _log.LogError($"---------------------------------------------------------Starting");

            async Task Work()
            {
                await foreach (var item in reader.ReadAllAsync())
                {
                    //_log.LogInformation($"ControllerLogArchive: {item.Value.SignalId} - {item.Value.ArchiveDate} - {item.Value.LogData.Length}");

                    await db.ControllerLogArchives.AddAsync(item.Value);

                    //add to delete channel
                    await deleteChannel.WriteAsync(item.Key);
                }

                //db.SaveChanges();

            }

            Task.Run(async () =>
            {
                await Work();
                _log.LogError($"-----------------------------------------------------SavingChanges-{sw.Elapsed}");
                db.SaveChanges();

                deleteChannel.TryComplete();

                _log.LogError($"-----------------------------------------------------Complete-{sw.Elapsed}");
                sw.Stop();
                _log.LogInformation($"Step Complete: {nameof(StepFour)}");
            });
        }
    }
}
