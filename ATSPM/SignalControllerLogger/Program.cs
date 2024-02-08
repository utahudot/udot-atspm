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

                Console.WriteLine($"{devices.Count()}");

                var input = new BufferBlock<Device>();

                //var signalControllers = new ActionBlock<Device>(i =>
                //{
                //    Console.WriteLine($"signalControllers - {i}");
                //});

                //var rampController = new ActionBlock<Device>(i => Console.WriteLine($"rampController - {i}"));
                //var aiCamera = new ActionBlock<Device>(i => Console.WriteLine($"aiCamera - {i}"));
                //var firCamera = new ActionBlock<Device>(i => Console.WriteLine($"firCamera - {i}"));

                //input.LinkTo(signalControllers, new DataflowLinkOptions() { PropagateCompletion = true }, i => i.DeviceConfiguration.Product.DeviceType == Data.Enums.DeviceTypes.SignalController);
                //input.LinkTo(rampController, new DataflowLinkOptions() { PropagateCompletion = true }, i => i.DeviceConfiguration.Product.DeviceType == Data.Enums.DeviceTypes.RampController);
                //input.LinkTo(aiCamera, new DataflowLinkOptions() { PropagateCompletion = true }, i => i.DeviceConfiguration.Product.DeviceType == Data.Enums.DeviceTypes.AICamera);
                //input.LinkTo(firCamera, new DataflowLinkOptions() { PropagateCompletion = true }, i => i.DeviceConfiguration.Product.DeviceType == Data.Enums.DeviceTypes.FIRCamera);

                //var downloadStep = new TransformManyBlock<Device, FileInfo>(t =>
                //{
                //    using (var downloadscope = host.Services.CreateScope())
                //    {
                //        var downloader = downloadscope.ServiceProvider.GetServices<IDeviceDownloader>().First(c => c.CanExecute(t));

                //        return downloader.Execute(t);
                //    }

                //    //await foreach (var file in downloader.Execute(t))
                //    //{
                //    //    //Console.WriteLine($"{t.Ipaddress} -- {file.FullName}");
                //    //    return file;
                //    //}
                //}, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 5});


                var downloadStep = new DownloadDeviceData(host.Services, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 10 });
                var decodeStep = new DecodeDeviceData(host.Services, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 10 });
                var logArchiveBatch = new BatchBlock<EventLogModelBase>(50000);

                var archiveLogs = new TransformManyBlock<EventLogModelBase[], CompressedEventLogBase>(t =>
                {

                    var test = t.GroupBy(g => (g.Timestamp.Date, g.LocationIdentifier, g.GetType()))
                    .Select(s => Stuff.MakeItRain<IndianaEvent>(s.Key.Item3));




                    //t.GroupBy(g => (g.Timestamp.Date, g.LocationIdentifier))
                    //.Select(s => new CompressedEventLogs<>() { SignalIdentifier = s.Key.LocationIdentifier, ArchiveDate = s.Key.Date, LogData = t.ToList() });
                });

                var actionResult = new ActionBlock<EventLogModelBase[]>(async t => Console.WriteLine($"{t}"));
                //{
                //    var repo = scope.ServiceProvider.GetService<IEventLogRepository>();

                //    var searchLog = await repo.LookupAsync(t);

                //    if (searchLog != null)
                //    {
                //        var eventLogs = new HashSet<EventLogModelBase>(Enumerable.Union(searchLog.LogData, archive.LogData));

                //        searchLog.Data = eventLogs.ToList();

                //        await EventLogArchive.UpdateAsync(searchLog);

                //        result.Add(searchLog);
                //    }
                //    else
                //    {
                //        await EventLogArchive.AddAsync(archive);
                //        result.Add(archive);
                //    }
                //});

                input.LinkTo(downloadStep, new DataflowLinkOptions() { PropagateCompletion = true });
                downloadStep.LinkTo(decodeStep, new DataflowLinkOptions() { PropagateCompletion = true });
                decodeStep.LinkTo(logArchiveBatch, new DataflowLinkOptions() { PropagateCompletion = true });
                logArchiveBatch.LinkTo(actionResult, new DataflowLinkOptions() { PropagateCompletion = true });

                foreach (var d in devices)
                {
                    //Console.WriteLine($"{d}");
                    input.Post(d);
                }

                input.Complete();


                await actionResult.Completion;


                Console.WriteLine($"*********************************************complete");
            }








            Console.Read();

        }

    }

    public static class Stuff
    {
        public static CompressedEventLogs<T> MakeItRain<T>(Type type) where T : EventLogModelBase
        {
            return (CompressedEventLogs<T>)Activator.CreateInstance(typeof(CompressedEventLogs<>).MakeGenericType(typeof(T)));
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
}