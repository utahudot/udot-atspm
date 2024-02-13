using ATSPM.Application.Configuration;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Application.Services;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using ATSPM.Domain.Workflows;
using ATSPM.Domain.Extensions;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Services.ControllerDownloaders;
using ATSPM.Infrastructure.Services.DownloaderClients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using ATSPM.Infrastructure.Services.ControllerDecoders;
using ATSPM.Application.Common.EqualityComparers;
using System.Net.NetworkInformation;
using ATSPM.Data;
using Newtonsoft.Json;
using AutoFixture;
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Application.Analysis.WorkflowSteps;
using Microsoft.AspNetCore.Mvc;

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
                    //    l.AddGoogle(new LoggingServiceOptions
                    //    {
                    //        ProjectId = "1022556126938",
                    //        //ProjectId = "869261868126",
                    //        ServiceName = AppDomain.CurrentDomain.FriendlyName,
                    //        Version = Assembly.GetEntryAssembly().GetName().Version.ToString(),
                    //        Options = LoggingOptions.Create(LogLevel.Information, AppDomain.CurrentDomain.FriendlyName)
                    //    });
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
                        o.ConnectionTimeout = 2000;
                        o.ReadTimeout = 2000;
                        //o.DeleteFile = true;
                    });


                })

                .UseConsoleLifetime()
                .Build();

            //await host.RunAsync();

            using (var scope = host.Services.CreateScope())
            {

                //var repo = scope.ServiceProvider.GetService<IIndianaEventLogRepository>();
                //var repo = scope.ServiceProvider.GetService<IEventLogRepository>();


                //var eventLogs = new CompressedEventLogs<IndianaEvent>()
                //{
                //    LocationIdentifier = "1001",
                //    ArchiveDate = DateOnly.FromDateTime(DateTime.Now),
                //    DeviceId = 1,
                //    Data = new Fixture().CreateMany<IndianaEvent>(10).ToList()
                //};

                //foreach (var d in eventLogs.Data)
                //{
                //    Console.WriteLine($"d: {d}");
                //}

                //CompressedEventLogBase test = eventLogs;

                //await repo.AddAsync(test);













                //var ftpDevices = scope.ServiceProvider.GetService<IDeviceRepository>().GetActiveDevicesByAllLatestLocations()
                //    .Where(w => w.Ipaddress.ToString() != "10.10.10.10")
                //    .Where(w => w.Ipaddress.IsValidIPAddress())
                //    .Where(w => w.DeviceConfiguration.Protocol == TransportProtocols.Ftp)
                //    .Take(3);

                //var httpDevices = scope.ServiceProvider.GetService<IDeviceRepository>().GetActiveDevicesByAllLatestLocations()
                //    .Where(w => w.Ipaddress.ToString() != "10.10.10.10")
                //    .Where(w => w.Ipaddress.IsValidIPAddress())
                //    .Where(w => w.DeviceConfiguration.Protocol == TransportProtocols.Http)
                //    .Take(3);

                var sftpDevices = scope.ServiceProvider.GetService<IDeviceRepository>().GetActiveDevicesByAllLatestLocations()
                    .Where(w => w.Ipaddress.ToString() != "10.10.10.10")
                    .Where(w => w.Ipaddress.IsValidIPAddress())
                    .Where(w => w.DeviceConfiguration.Protocol == TransportProtocols.Sftp)
                    .Take(1);

                var devices = sftpDevices;

                Console.WriteLine($"devices: {devices.Count()}");

                //var input = new BufferBlock<Device>();
                var input = new BufferBlock<Tuple<Device, FileInfo>>();

                var downloadStep = new DownloadDeviceData(host.Services, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 10 });
                //var decodeStep = new DecodeDeviceData<IndianaEvent>(host.Services, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 10 });
                //var logArchiveBatch = new BatchBlock<Tuple<Device, IndianaEvent>>(50000);
                //var archiveDeviceData = new ArchiveDeviceData<IndianaEvent>(host.Services, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 10 });

                var processEventLogFileWorkflow = new ProcessEventLogFileWorkflow<IndianaEvent>(host.Services, 1);

                //var actionResult = new ActionBlock<CompressedEventLogBase>(async t => Console.WriteLine($"{t.LocationIdentifier} - {t.ArchiveDate} - {t.Data.Count()}"));
                var actionResult = new ActionBlock<CompressedEventLogBase>(async t =>
                {
                    //Console.WriteLine($"{t.LocationIdentifier} - {t.ArchiveDate} - {t.DeviceId} - {t.DataType} - {t.Data.Count()}");

                    var repo = scope.ServiceProvider.GetService<IEventLogRepository>();

                    var searchLog = await repo.LookupAsync(t);

                    if (searchLog != null)
                    {
                        Console.WriteLine($"&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&& {searchLog.Data.Count()} --- {t.Data.Count()}");

                        var eventLogs = new HashSet<EventLogModelBase>(Enumerable.Union(searchLog.Data, t.Data));

                        searchLog.Data = eventLogs.ToList();

                        await repo.UpdateAsync(searchLog);
                    }
                    else
                    {
                        await repo.AddAsync(t);
                    }

                    foreach (var i in repo.GetList())
                    {
                        Console.WriteLine($"{i.LocationIdentifier} - {i.ArchiveDate} - {i.DeviceId} - {i.Data.Count()}");
                    }
                });



                //input.LinkTo(downloadStep, new DataflowLinkOptions() { PropagateCompletion = true });
                //downloadStep.LinkTo(decodeStep, new DataflowLinkOptions() { PropagateCompletion = true });


                //await processEventLogFileWorkflow.Initialize();

                await Task.Delay(TimeSpan.FromSeconds(1));

                input.LinkTo(processEventLogFileWorkflow.Input, new DataflowLinkOptions() { PropagateCompletion = true });
                processEventLogFileWorkflow.Output.LinkTo(actionResult, new DataflowLinkOptions() { PropagateCompletion = true });


                foreach (var f in new DirectoryInfo("C:\\temp\\5222 - 205 S. (SR-193)\\SignalController\\10.233.7.51").GetFiles())
                {
                    input.Post(Tuple.Create(devices.First(), f));
                }

                //foreach (var d in devices)
                //{
                //    //Console.WriteLine($"{d}");
                //    input.Post(d);
                //}

                input.Complete();


                await actionResult.Completion;


                Console.WriteLine($"*********************************************complete");
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

                Console.WriteLine($"device: {input.DeviceConfiguration.Protocol} - downloader: {downloader.GetType().Name}");

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
            using (var scope = _serviceProvider.CreateAsyncScope())
            {
                var decoder = scope.ServiceProvider.GetServices<ILocationControllerDecoder<T>>().First(c => c.CanExecute(input));

                //Console.WriteLine($"device: {input.DeviceConfiguration.Protocol} - downloader: {downloader.GetType().Name}");

                return decoder.Execute(input, cancelToken);
            }
        }
    }

    public class ArchiveDeviceData<T> : TransformManyProcessStepBaseAsync<Tuple<Device, T>[], CompressedEventLogs<T>> where T : EventLogModelBase
    {
        private readonly IServiceProvider _serviceProvider;

        /// <inheritdoc/>
        public ArchiveDeviceData(IServiceProvider serviceProvider, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async IAsyncEnumerable<CompressedEventLogs<T>> Process(Tuple<Device, T>[] input, CancellationToken cancelToken = default)
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
                //Console.WriteLine($"$$${r.LocationIdentifier} - {r.ArchiveDate} - {r.DeviceId} - {r.DataType} - {r.Data.Count()}");

                yield return r;
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
            ArchiveDeviceDataStep = new(_serviceProvider, _stepOptions);
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