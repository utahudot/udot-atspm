using ATSPM.Application.Configuration;
using ATSPM.Application.Services;
using ATSPM.Data;
using ATSPM.Data.Models;

using ATSPM.Domain.Workflows;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Services.ControllerDownloaders;
using ATSPM.Infrastructure.Services.DownloaderClients;
using AutoMapper.Internal;
using EFCore.BulkExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using AutoFixture;
using ATSPM.Data.Models.AggregationModels;
using ATSPM.Domain.Services;
using ATSPM.Domain.Extensions;
using ATSPM.Infrastructure.Repositories.EventLogRepositories;
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Data.Models.EventLogModels;
using ATSPM.Data.Enums;
using ATSPM.Application.Common.EqualityComparers;

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
                    //s.AddScoped<ILocationControllerDecoder, ASCLocationControllerDecoder>();
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
                    });


                })

                .UseConsoleLifetime()
                .Build();

            //await host.RunAsync();

            //using (var scope = host.Services.CreateScope())
            //{
            //    var ftpDevices = scope.ServiceProvider.GetService<IDeviceRepository>().GetActiveDevicesByAllLatestLocations()
            //        .Where(w => w.Ipaddress.ToString() != "10.10.10.10")
            //        .Where(w => w.Ipaddress.IsValidIPAddress())
            //        .Where(w => w.DeviceConfiguration.Protocol == TransportProtocols.Ftp)
            //        .Take(3);

            //    var httpDevices = scope.ServiceProvider.GetService<IDeviceRepository>().GetActiveDevicesByAllLatestLocations()
            //        .Where(w => w.Ipaddress.ToString() != "10.10.10.10")
            //        .Where(w => w.Ipaddress.IsValidIPAddress())
            //        .Where(w => w.DeviceConfiguration.Protocol == TransportProtocols.Http)
            //        .Take(3);

            //    var devices = ftpDevices.Union(httpDevices);

            //    Console.WriteLine($"{devices.Count()}");

            //    var input = new BufferBlock<Device>();

            //    //var signalControllers = new ActionBlock<Device>(i =>
            //    //{
            //    //    Console.WriteLine($"signalControllers - {i}");
            //    //});

            //    //var rampController = new ActionBlock<Device>(i => Console.WriteLine($"rampController - {i}"));
            //    //var aiCamera = new ActionBlock<Device>(i => Console.WriteLine($"aiCamera - {i}"));
            //    //var firCamera = new ActionBlock<Device>(i => Console.WriteLine($"firCamera - {i}"));

            //    //input.LinkTo(signalControllers, new DataflowLinkOptions() { PropagateCompletion = true }, i => i.DeviceConfiguration.Product.DeviceType == Data.Enums.DeviceTypes.SignalController);
            //    //input.LinkTo(rampController, new DataflowLinkOptions() { PropagateCompletion = true }, i => i.DeviceConfiguration.Product.DeviceType == Data.Enums.DeviceTypes.RampController);
            //    //input.LinkTo(aiCamera, new DataflowLinkOptions() { PropagateCompletion = true }, i => i.DeviceConfiguration.Product.DeviceType == Data.Enums.DeviceTypes.AICamera);
            //    //input.LinkTo(firCamera, new DataflowLinkOptions() { PropagateCompletion = true }, i => i.DeviceConfiguration.Product.DeviceType == Data.Enums.DeviceTypes.FIRCamera);

            //    //var downloadStep = new TransformManyBlock<Device, FileInfo>(t =>
            //    //{
            //    //    using (var downloadscope = host.Services.CreateScope())
            //    //    {
            //    //        var downloader = downloadscope.ServiceProvider.GetServices<IDeviceDownloader>().First(c => c.CanExecute(t));

            //    //        return downloader.Execute(t);
            //    //    }

            //    //    //await foreach (var file in downloader.Execute(t))
            //    //    //{
            //    //    //    //Console.WriteLine($"{t.Ipaddress} -- {file.FullName}");
            //    //    //    return file;
            //    //    //}
            //    //}, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 5});


            //    var downloadStep = new DownloadDeviceData(host.Services, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1 });

            //    var downloadResult = new ActionBlock<FileInfo>(t => Console.WriteLine($"Downloaded file - {t}"));

            //    input.LinkTo(downloadStep, new DataflowLinkOptions() { PropagateCompletion = true });
            //    downloadStep.LinkTo(downloadResult, new DataflowLinkOptions() { PropagateCompletion = true });

            //    foreach (var d in devices)
            //    {
            //        //Console.WriteLine($"{d}");
            //        input.Post(d);
            //    }

            //    input.Complete();


            //    await downloadResult.Completion;


            //    Console.WriteLine($"*********************************************complete");
            //}



            //var type = "System.Collections.Generic.List`1[[ATSPM.Data.Models.ControllerEventLog, ATSPM.Data]], System.Private.CoreLib";


            //var e = new IndianaEvent()
            //{
            //    LocationIdentifier = "1234",
            //    Timestamp = DateTime.Now,
            //    EventCode = 1,
            //    EventParam = 1
            //};

            //var list = new List<IndianaEvent>() { e };




            //var test2 = JsonConvert.DeserializeObject<IEnumerable<EventLogModelBase>>(test.GZipDecompressToString(), new JsonSerializerSettings()
            //{
            //    TypeNameHandling = TypeNameHandling.Arrays
            //});

            //Console.WriteLine($"{test2?.Count()}");


            //Console.WriteLine($"{typeof(IndianaEvent).FullName}, {typeof(IndianaEvent).Assembly}");
            //Console.WriteLine($"{typeof(IndianaEvent).AssemblyQualifiedName}");


            using (var scope = host.Services.CreateScope())
            {
                var loc = "1234";
                var timestamp = DateTime.Now.ToString();

                //var repo = scope.ServiceProvider.GetService<IEventLogRepository>();

                //for (var i = 0; i < 5; i++)
                //{
                //    var indianaEvents = Enumerable.Range(1, 10).Select(s => new IndianaEvent()
                //    {
                //        LocationIdentifier = loc,
                //        Timestamp = DateTime.Now,
                //        EventCode = (DataLoggerEnum)Random.Shared.Next(1, 255),
                //        EventParam = (byte)Random.Shared.Next(1, 255)
                //    }).ToList();

                //    repo.Add(new CompressedEventLogs<IndianaEvent>()
                //    {
                //        LocationIdentifier = loc,
                //        ArchiveDate = DateOnly.FromDateTime(DateTime.Now),
                //        DataType = typeof(IndianaEvent),
                //        DeviceId = i,
                //        Data = indianaEvents
                //    });

                //    var speedEvents = Enumerable.Range(1, 10).Select(s => new SpeedEvent()
                //    {
                //        LocationIdentifier = loc,
                //        Timestamp = DateTime.Now,
                //        DetectorId = "1",
                //        Mph = Random.Shared.Next(1, 100),
                //        Kph = Random.Shared.Next(1, 100)
                //    }).ToList();

                //repo.Add(new CompressedEventLogs<SpeedEvent>()
                //{
                //    LocationIdentifier = loc,
                //    ArchiveDate = DateOnly.FromDateTime(DateTime.Now),
                //    DataType = typeof(SpeedEvent),
                //    DeviceId = i + 10,
                //    Data = speedEvents
                //});



                var repo = scope.ServiceProvider.GetService<IIndianaEventLogRepository>();

                //foreach (var i in repo.GetEventsBetweenDates(loc, DateTime.Now.AddHours(-4), DateTime.Now.AddHours(4)))
                //{
                //    Console.WriteLine($"{i}");
                //}



                var indianaEvents = Enumerable.Range(1, 100).Select(s => new IndianaEvent()
                {
                    LocationIdentifier = loc,
                    Timestamp = DateTime.Parse(timestamp),
                    EventCode = (DataLoggerEnum)Random.Shared.Next(1, 10),
                    EventParam = (byte)Random.Shared.Next(1, 10)
                }).ToList();

                Console.WriteLine($"total created: {indianaEvents.Count}");

                //var noequal = new HashSet<IndianaEvent>(indianaEvents);

                //Console.WriteLine($"{noequal.Count}");

                await repo.AddAsync(new CompressedEventLogs<IndianaEvent>()
                {
                    LocationIdentifier = loc,
                    ArchiveDate = DateOnly.FromDateTime(DateTime.Now),
                    DataType = typeof(IndianaEvent),
                    DeviceId = 1,
                    Data = indianaEvents
                });

                Console.WriteLine($"total saved: {repo.GetList().AsEnumerable().SelectMany(m => m.Data).Count()}");
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
}