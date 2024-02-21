using ATSPM.Application.Configuration;
using ATSPM.Application.Exceptions;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Application.Services;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using ATSPM.Domain.Extensions;
using ATSPM.Domain.Workflows;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Services.ControllerDecoders;
using ATSPM.Infrastructure.Services.ControllerDownloaders;
using ATSPM.Infrastructure.Services.DownloaderClients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Google.Cloud.Diagnostics.Common;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace ATSPM.LocationControllerLogger
{
    class Program
    {
        static async Task Main(string[] args)
        {

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging((h, l) =>
                {
                    //l.SetMinimumLevel(LogLevel.None);

                    //TODO: add a GoogleLogger section
                    //LoggingServiceOptions GoogleOptions = h.Configuration.GetSection("GoogleLogging").Get<LoggingServiceOptions>();
                    //TODO: remove this to an extension method
                    //DOTNET_ENVIRONMENT = Development,GOOGLE_APPLICATION_CREDENTIALS = M:\My Drive\ut-udot-atspm-dev-023438451801.json
                    //if (h.Configuration.GetValue<bool>("UseGoogleLogger"))
                    //{
                    l.AddGoogle(new LoggingServiceOptions
                    {
                        ProjectId = "1022556126938",
                        //ProjectId = "869261868126",
                        ServiceName = AppDomain.CurrentDomain.FriendlyName,
                        Version = Assembly.GetEntryAssembly().GetName().Version.ToString(),
                        Options = LoggingOptions.Create(LogLevel.Information, AppDomain.CurrentDomain.FriendlyName)
                    });
                    //}
                })
                .ConfigureServices((h, s) =>
                {
                    //s.AddGoogleErrorReporting(new ErrorReportingServiceOptions() {
                    //    ProjectId = "1022556126938",
                    //    ServiceName = "ErrorReporting",
                    //    Version = "1.1",
                    //});

                    s.AddLogging();

                    s.AddAtspmDbContext(h);

                    //background services
                    //s.AddHostedService<LoggerBackgroundService>();

                    //repositories
                    s.AddAtspmEFConfigRepositories();
                    s.AddAtspmEFEventLogRepositories();
                    s.AddAtspmEFAggregationRepositories();

                    //s.AddTransient<IFileTranscoder, JsonFileTranscoder>();
                    //s.AddTransient<IFileTranscoder, ParquetFileTranscoder>();
                    //s.AddTransient<IFileTranscoder, CompressedJsonFileTranscoder>();

                    ////downloader clients
                    s.AddTransient<IHTTPDownloaderClient, HttpDownloaderClient>();
                    s.AddTransient<IFTPDownloaderClient, FluentFTPDownloaderClient>();
                    s.AddTransient<ISFTPDownloaderClient, SSHNetSFTPDownloaderClient>();

                    //downloaders
                    //s.AddScoped<IDeviceDownloader, DeviceFtpDownloader>();
                    //s.AddScoped<IDeviceDownloader, CobaltLocationControllerDownloader>();
                    //s.AddScoped<IDeviceDownloader, MaxTimeLocationControllerDownloader>();
                    //s.AddScoped<IDeviceDownloader, EOSSignalControllerDownloader>();
                    //s.AddScoped<IDeviceDownloader, NewCobaltLocationControllerDownloader>();
                    s.AddScoped<IDeviceDownloader, DeviceFtpDownloader>();
                    s.AddScoped<IDeviceDownloader, DeviceSftpDownloader>();
                    s.AddScoped<IDeviceDownloader, DeviceHttpDownloader>();
                    //s.AddScoped<IDeviceDownloader, DeviceSnmpDownloader>();











                    //decoders
                    s.AddScoped<ILocationControllerDecoder<IndianaEvent>, ASCLocationControllerDecoder>();
                    //s.AddScoped<ILocationControllerDecoder<IndianaEvent>, MaxTimeLocationControllerDecoder>();

                    //LocationControllerDataFlow
                    //s.AddScoped<ILocationControllerLoggerService, CompressedLocationControllerLogger>();
                    //s.AddScoped<ILocationControllerLoggerService, LegacyLocationControllerLogger>();

                    //controller logger configuration
                    //s.Configure<LocationControllerLoggerConfiguration>(h.Configuration.GetSection(nameof(LocationControllerLoggerConfiguration)));

                    //downloader configurations
                    //s.ConfigureSignalControllerDownloaders(h);
                    //s.Configure<SignalControllerDownloaderConfiguration>(nameof(DeviceFtpDownloader), h.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(DeviceFtpDownloader)}"));
                    //s.Configure<SignalControllerDownloaderConfiguration>(nameof(CobaltLocationControllerDownloader), h.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(CobaltLocationControllerDownloader)}"));
                    //s.Configure<SignalControllerDownloaderConfiguration>(nameof(MaxTimeLocationControllerDownloader), h.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(MaxTimeLocationControllerDownloader)}"));
                    //s.Configure<SignalControllerDownloaderConfiguration>(nameof(EOSSignalControllerDownloader), h.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(EOSSignalControllerDownloader)}"));
                    //s.Configure<SignalControllerDownloaderConfiguration>(nameof(NewCobaltLocationControllerDownloader), h.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(NewCobaltLocationControllerDownloader)}"));

                    //decoder configurations
                    //s.Configure<SignalControllerDecoderConfiguration>(nameof(ASCLocationControllerDecoder), h.Configuration.GetSection($"{nameof(SignalControllerDecoderConfiguration)}:{nameof(ASCLocationControllerDecoder)}"));
                    //s.Configure<SignalControllerDecoderConfiguration>(nameof(MaxTimeLocationControllerDecoder), h.Configuration.GetSection($"{nameof(SignalControllerDecoderConfiguration)}:{nameof(MaxTimeLocationControllerDecoder)}"));

                    //s.Configure<FileRepositoryConfiguration>(h.Configuration.GetSection("FileRepositoryConfiguration"));




                    s.PostConfigureAll<SignalControllerDownloaderConfiguration>(o =>
                    {
                        o.LocalPath = "C:\\temp";
                        o.PingControllerToVerify = true;
                        o.ConnectionTimeout = 3000;
                        o.ReadTimeout = 3000;
                        o.DeleteFile = false;
                    });

                    
                })

                .UseConsoleLifetime()
                .Build();

            host.Services.PrintHostInformation();

            //await host.RunAsync();

            using (var scope = host.Services.CreateScope())
            {
                ////var ftpDevices = scope.ServiceProvider.GetService<IDeviceRepository>().GetActiveDevicesByAllLatestLocations()
                ////    .Where(w => w.Ipaddress.ToString() != "10.10.10.10")
                ////    .Where(w => w.Ipaddress.IsValidIPAddress())
                ////    .Where(w => w.DeviceConfiguration.Protocol == TransportProtocols.Ftp)
                ////    .Take(3);

                ////var httpDevices = scope.ServiceProvider.GetService<IDeviceRepository>().GetActiveDevicesByAllLatestLocations()
                ////    .Where(w => w.Ipaddress.ToString() != "10.10.10.10")
                ////    .Where(w => w.Ipaddress.IsValidIPAddress())
                ////    .Where(w => w.DeviceConfiguration.Protocol == TransportProtocols.Http)
                ////    .Take(3);

                var sw = new Stopwatch();
                sw.Start();

                var sftpDevices = scope.ServiceProvider.GetService<IDeviceRepository>().GetActiveDevicesByAllLatestLocations()
                    .Where(w => w.Ipaddress.ToString() != "10.10.10.10")
                    .Where(w => w.Ipaddress.IsValidIPAddress())
                    .Where(w => w.DeviceConfiguration.Protocol == TransportProtocols.Sftp)
                    //.Where(w => w.DeviceConfiguration.Protocol != TransportProtocols.Http)
                    .OrderBy(o => o.Ipaddress.ToString());
                    //.Skip(10)
                    //.Take(100);

                //var devices = sftpDevices.Where(w => w.Ipaddress.IsValidIPAddress(true));
                var devices = sftpDevices;


                Console.WriteLine($"devices: {devices.Count()}");

                foreach (var d in devices)
                {
                    Console.WriteLine($"device: {d}");
                }


                int instances = 1;


                var input = new BufferBlock<Device>();

                var downloadStep = new DownloadDeviceData(host.Services, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = instances });
                var processEventLogFileWorkflow = new ProcessEventLogFileWorkflow<IndianaEvent>(host.Services, instances);
                var SaveEventsToRepo = new SaveEventsToRepo(host.Services, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1 });

                var actionResult = new ActionBlock<CompressedEventLogBase>(t =>
                {
                    Console.WriteLine($"{t.LocationIdentifier} - {t.ArchiveDate} - {t.DeviceId} - {t.Data.Count()}");

                    //var repo = scope.ServiceProvider.GetService<IEventLogRepository>();

                    //var i = await repo.LookupAsync(t);
                    //Console.WriteLine($"======================={i.LocationIdentifier} - {i.ArchiveDate} - {i.DeviceId} - {i.Data.Count()}=======================");

                    //foreach (var i in repo.GetList())
                    //{
                    //    Console.WriteLine($"{i.LocationIdentifier} - {i.ArchiveDate} - {i.DeviceId} - {i.Data.Count()}");
                    //}
                }, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = instances });

                input.LinkTo(downloadStep, new DataflowLinkOptions() { PropagateCompletion = true });

                await Task.Delay(TimeSpan.FromSeconds(1));

                downloadStep.LinkTo(processEventLogFileWorkflow.Input, new DataflowLinkOptions() { PropagateCompletion = true });
                processEventLogFileWorkflow.Output.LinkTo(SaveEventsToRepo, new DataflowLinkOptions() { PropagateCompletion = true });
                SaveEventsToRepo.LinkTo(actionResult, new DataflowLinkOptions() { PropagateCompletion = true });

                foreach (var d in devices)
                {
                    input.Post(d);
                }

                input.Complete();

                try
                {
                    await actionResult.Completion.ContinueWith(t => Console.WriteLine($"!!!Task actionResult is complete!!! {t.Status}"));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{actionResult.Completion.Status}---------------{e}");
                }











                sw.Stop();

                Console.WriteLine($"*********************************************complete - {sw.Elapsed}");
            }


            Console.Read();

        }

    }

    

    public static class Testing
    {
        public static string ToCsv(this object obj)
        {
            return string.Join(",", obj.GetType().GetProperties().Select(pi => pi.GetValue(obj, null)));
        }
    }

    public class DownloadDeviceData : TransformManyProcessStepBaseAsync<Device, Tuple<Device, FileInfo>>
    {
        private readonly IServiceProvider _serviceProvider;
        
        /// <inheritdoc/>
        public DownloadDeviceData(IServiceProvider serviceProvider, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) 
        {
            _serviceProvider = serviceProvider;
        }

        protected override IAsyncEnumerable<Tuple<Device, FileInfo>> Process(Device input, CancellationToken cancelToken = default)
        {
            using (var scope = _serviceProvider.CreateAsyncScope())
            {
                var downloader = scope.ServiceProvider.GetServices<IDeviceDownloader>().First(c => c.CanExecute(input));

                return downloader.Execute(input, cancelToken);
            }
        }
    }

    public class DecodeDeviceData<T> : TransformManyProcessStepBaseAsync<Tuple<Device, FileInfo>, Tuple<Device, T>> where T : EventLogModelBase
    {
        private readonly IServiceProvider _serviceProvider;

        /// <inheritdoc/>
        public DecodeDeviceData(IServiceProvider serviceProvider, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            _serviceProvider = serviceProvider;
        }

        protected override IAsyncEnumerable<Tuple<Device, T>> Process(Tuple<Device, FileInfo> input, CancellationToken cancelToken = default)
        {
            try
            {
                using (var scope = _serviceProvider.CreateAsyncScope())
                {
                    var decoder = scope.ServiceProvider.GetServices<ILocationControllerDecoder<T>>().First(c => c.CanExecute(input));

                    //Console.WriteLine($"device: {input.DeviceConfiguration.Protocol} - downloader: {downloader.GetType().Name}");

                    return decoder.Execute(input, cancelToken);
                }
            }
            catch (Exception)
            {
            }

            return default;
        }
    }

    public class ArchiveDeviceData<T> : TransformManyProcessStepBaseAsync<Tuple<Device, T>[], CompressedEventLogs<T>> where T : EventLogModelBase
    {
        /// <inheritdoc/>
        public ArchiveDeviceData(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override async IAsyncEnumerable<CompressedEventLogs<T>> Process(Tuple<Device, T>[] input, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            var result = input.GroupBy(g => (g.Item2.LocationIdentifier, g.Item2.Timestamp.Date, g.Item1.Id))
                .Select(s => new CompressedEventLogs<T>()
                {
                    LocationIdentifier = s.Key.LocationIdentifier,
                    ArchiveDate = DateOnly.FromDateTime(s.Key.Date),
                    DeviceId = s.Key.Id,
                    Data = s.Select(s => s.Item2).ToList()
                });

            foreach (var r in result)
            {
                yield return r;
            }
        }
    }

    public class SaveEventsToRepo : TransformManyProcessStepBaseAsync<CompressedEventLogBase, CompressedEventLogBase>
    {
        private readonly IServiceProvider _serviceProvider;

        /// <inheritdoc/>
        public SaveEventsToRepo(IServiceProvider serviceProvider, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async IAsyncEnumerable<CompressedEventLogBase> Process(CompressedEventLogBase input, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            using (var scope = _serviceProvider.CreateAsyncScope())
            {
                var repo = scope.ServiceProvider.GetService<IEventLogRepository>();

                var searchLog = await repo.LookupAsync(input);

                if (searchLog != null)
                {
                    var eventLogs = new HashSet<EventLogModelBase>(Enumerable.Union(searchLog.Data, input.Data));

                    searchLog.Data = eventLogs.ToList();

                    await repo.UpdateAsync(searchLog);
                }
                else
                {
                    await repo.AddAsync(input);
                }

                yield return input;
            }
        }
    }

    //public class ArchiveDeviceData<T> : TransformManyProcessStepBaseAsync<EventLogModelBase[], CompressedEventLogs<T>> where T : EventLogModelBase
    //{
    //    private readonly IServiceProvider _serviceProvider;

    //    /// <inheritdoc/>
    //    public ArchiveDeviceData(IServiceProvider serviceProvider, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
    //    {
    //        _serviceProvider = serviceProvider;
    //    }

    //    protected override IAsyncEnumerable<CompressedEventLogs<T>> Process(EventLogModelBase[] input, CancellationToken cancelToken = default)
    //    {
    //        using (var scope = _serviceProvider.CreateAsyncScope())
    //        {
    //            / t.GroupBy(g => (g.Timestamp.Date, g.LocationIdentifier))
    //                //.Select(s => new CompressedEventLogs<>() { SignalIdentifier = s.Key.LocationIdentifier, ArchiveDate = s.Key.Date, LogData = t.ToList() });

    //            var repo = scope.ServiceProvider.GetService<IEventLogRepository>();

    //            var searchLog = await repo.LookupAsync(t);

    //            if (searchLog != null)
    //            {
    //                var eventLogs = new HashSet<EventLogModelBase>(Enumerable.Union(searchLog.LogData, archive.LogData));

    //                searchLog.Data = eventLogs.ToList();

    //                await EventLogArchive.UpdateAsync(searchLog);

    //                result.Add(searchLog);
    //            }
    //            else
    //            {
    //                await EventLogArchive.AddAsync(archive);
    //                result.Add(archive);
    //            }
    //        }
    //    }
    //}

    public class ProcessEventLogFileWorkflow<T> : WorkflowBase<Tuple<Device, FileInfo>, CompressedEventLogs<T>> where T : EventLogModelBase
    {
        private readonly DataflowBlockOptions _filterOptions = new DataflowBlockOptions();
        private readonly ExecutionDataflowBlockOptions _stepOptions = new ExecutionDataflowBlockOptions();

        private readonly IServiceProvider _serviceProvider;

        public ProcessEventLogFileWorkflow(IServiceProvider serviceProvider, int maxDegreeOfParallelism = 1, CancellationToken cancellationToken = default)
        {
            _serviceProvider = serviceProvider;

            _filterOptions.CancellationToken = cancellationToken;
            _stepOptions.CancellationToken = cancellationToken;
            _stepOptions.MaxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        public DecodeDeviceData<T> DecodeDeviceDataStep { get; private set; }
        public BatchBlock<Tuple<Device, T>> BatchLogsStep { get; private set; }
        public ArchiveDeviceData<T> ArchiveDeviceDataStep { get; private set; }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            DecodeDeviceDataStep = new(_serviceProvider, _stepOptions);
            BatchLogsStep = new(50000);
            ArchiveDeviceDataStep = new(_stepOptions);
        }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(DecodeDeviceDataStep);
            Steps.Add(BatchLogsStep);
            Steps.Add(ArchiveDeviceDataStep);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(DecodeDeviceDataStep, new DataflowLinkOptions() { PropagateCompletion = true });
            DecodeDeviceDataStep.LinkTo(BatchLogsStep, new DataflowLinkOptions() { PropagateCompletion = true });
            BatchLogsStep.LinkTo(ArchiveDeviceDataStep, new DataflowLinkOptions() { PropagateCompletion = true });
            ArchiveDeviceDataStep.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }
}