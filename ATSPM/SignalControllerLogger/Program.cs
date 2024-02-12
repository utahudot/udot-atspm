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
                    s.AddScoped<ILocationControllerDecoder, ASCLocationControllerDecoder>();
                    //s.AddScoped<ILocationControllerDecoder, MaxTimeLocationControllerDecoder>();

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
                var input = new BufferBlock<FileInfo>();

                var downloadStep = new DownloadDeviceData(host.Services, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 10 });
                var decodeStep = new DecodeDeviceData(host.Services, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 10 });
                var logArchiveBatch = new BatchBlock<EventLogModelBase>(50000);

                var testStep = new TransformBlock<EventLogModelBase[], IEnumerable<EventLogModelBase>>(t => t.Cast<IndianaEvent>().ToList());

                var archiveDeviceData = new ArchiveDeviceData<IndianaEvent>(host.Services, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 10 });

                //var actionResult = new ActionBlock<CompressedEventLogBase>(async t => Console.WriteLine($"{t.LocationIdentifier} - {t.ArchiveDate} - {t.Data.Count()}"));
                var actionResult = new ActionBlock<CompressedEventLogBase>(async t =>
                {
                    var repo = scope.ServiceProvider.GetService<IEventLogRepository>();

                    //var searchLog = await repo.LookupAsync(t);

                    Console.WriteLine($"three: {t.Data.GetType()}");

                    await repo.AddAsync(t);


                    //if (t.GetType().IsSubclassOf)

                    //var test1 = t.GetType().GetGenericArguments()[0];
                    //var test2 = typeof(CompressedEventLogs<>).MakeGenericType(test1);
                    ////var test3 = (CompressedEventLogBase)Activator.CreateInstance(test2);

                    //dynamic huh = Convert.ChangeType(t, test2);

                    //Console.WriteLine($"{huh.Data.GetType()}");




                    //var test = await repo.GetListAsync(w => w.LocationIdentifier == t.LocationIdentifier);
                    //var test = scope.ServiceProvider.GetService<IIndianaEventLogRepository>();


                    //if (t is CompressedEventLogs<IndianaEvent> huh)
                    //{
                    //    var test = Newtonsoft.Json.JsonConvert.SerializeObject(t.Data.Take(2), new JsonSerializerSettings()
                    //    {
                    //        Formatting = Newtonsoft.Json.Formatting.Indented,
                    //        TypeNameHandling = TypeNameHandling.Arrays
                    //    });


                    //    Console.WriteLine($"{test}");
                    //}

                    //if (t is CompressedEventLogs<IndianaEvent> huh)
                    //    await repo.AddAsync(huh);



                    foreach (var i in repo.GetList())
                    {
                        Console.WriteLine($"{i.LocationIdentifier} - {i.ArchiveDate} - {i.DeviceId} - {i.Data.Count()}");
                    }




                    //var searchLog = test.CompressedEvents.FirstOrDefault();

                    //if (searchLog != null)
                    //{
                    //    Console.WriteLine($"&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&& {searchLog.Data.Count()} --- {t.Data.Count()}");

                    //    var eventLogs = new HashSet<EventLogModelBase>(Enumerable.Union(searchLog.Data, t.Data));

                    //    searchLog.Data = eventLogs.ToList();

                    //    await repo.UpdateAsync(searchLog);
                    //}
                    //else
                    //{
                    //    if (t is CompressedEventLogs<IndianaEvent> huh)
                    //        await repo.AddAsync(huh);
                    //    //await repo.AddAsync(t);
                    //}
                });



                //input.LinkTo(downloadStep, new DataflowLinkOptions() { PropagateCompletion = true });
                //downloadStep.LinkTo(decodeStep, new DataflowLinkOptions() { PropagateCompletion = true });


                input.LinkTo(decodeStep, new DataflowLinkOptions() { PropagateCompletion = true });

                decodeStep.LinkTo(logArchiveBatch, new DataflowLinkOptions() { PropagateCompletion = true });
                logArchiveBatch.LinkTo(testStep, new DataflowLinkOptions() { PropagateCompletion = true });
                testStep.LinkTo(archiveDeviceData, new DataflowLinkOptions() { PropagateCompletion = true });
                archiveDeviceData.LinkTo(actionResult, new DataflowLinkOptions() { PropagateCompletion = true });




                foreach (var f in new DirectoryInfo("C:\\temp\\5222 - 205 S. (SR-193)\\SignalController\\10.233.7.51").GetFiles())
                {
                    input.Post(f);
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



    public class DownloadDeviceData : TransformManyProcessStepBaseAsync<Device, FileInfo>
    {
        private readonly IServiceProvider _serviceProvider;
        
        /// <inheritdoc/>
        public DownloadDeviceData(IServiceProvider serviceProvider, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) 
        {
            _serviceProvider = serviceProvider;
        }

        protected override IAsyncEnumerable<FileInfo> Process(Device input, CancellationToken cancelToken = default)
        {
            using (var scope = _serviceProvider.CreateAsyncScope())
            {
                var downloader = scope.ServiceProvider.GetServices<IDeviceDownloader>().First(c => c.CanExecute(input));

                Console.WriteLine($"device: {input.DeviceConfiguration.Protocol} - downloader: {downloader.GetType().Name}");

                return downloader.Execute(input, cancelToken);
            }
        }
    }

    public class DecodeDeviceData : TransformManyProcessStepBaseAsync<FileInfo, EventLogModelBase>
    {
        private readonly IServiceProvider _serviceProvider;

        /// <inheritdoc/>
        public DecodeDeviceData(IServiceProvider serviceProvider, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            _serviceProvider = serviceProvider;
        }

        protected override IAsyncEnumerable<EventLogModelBase> Process(FileInfo input, CancellationToken cancelToken = default)
        {
            using (var scope = _serviceProvider.CreateAsyncScope())
            {
                var decoder = scope.ServiceProvider.GetServices<ILocationControllerDecoder>().First(c => c.CanExecute(input));

                //Console.WriteLine($"device: {input.DeviceConfiguration.Protocol} - downloader: {downloader.GetType().Name}");

                return decoder.Execute(input, cancelToken);
            }
        }
    }

    public class ArchiveDeviceData<T> : TransformManyProcessStepBaseAsync<IEnumerable<EventLogModelBase>, CompressedEventLogBase> where T : EventLogModelBase
    {
        private readonly IServiceProvider _serviceProvider;

        /// <inheritdoc/>
        public ArchiveDeviceData(IServiceProvider serviceProvider, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async IAsyncEnumerable<CompressedEventLogBase> Process(IEnumerable<EventLogModelBase> input, CancellationToken cancelToken = default)
        {
            var result = input.GroupBy(g => (g.Timestamp.Date, g.LocationIdentifier))
                .Select(s => new CompressedEventLogs<T>()
                {
                    //LocationIdentifier = s.Key.LocationIdentifier,
                    LocationIdentifier = "test",
                    ArchiveDate = DateOnly.FromDateTime(s.Key.Date),
                    Data = s.OfType<T>().ToList()
                });

            foreach (var r in result)
            {
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
}