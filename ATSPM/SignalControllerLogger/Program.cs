using ATSPM.Application.Configuration;
using ATSPM.Application.Repositories;
using ATSPM.Application.Services;
using ATSPM.Domain.Common;
using ATSPM.Infrastructure.Converters;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Repositories;
using ATSPM.Infrastructure.Services.ControllerDecoders;
using ATSPM.Infrastructure.Services.ControllerDownloaders;
using ATSPM.Infrastructure.Services.LocationControllerLoggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LocationControllerLogger;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System;
using ATSPM.Application.Common.EqualityComparers;
using ATSPM.Data.Models;
using ATSPM.Data;
using EFCore.BulkExtensions;
using System.Collections.Generic;
using System.Threading;
using static Google.Cloud.Logging.V2.TailLogEntriesResponse.Types.SuppressionInfo.Types;
using Microsoft.OData.Edm.Csdl;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks.Dataflow;
using ATSPM.Domain.Workflows;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ATSPM.Data.Enums;
using ATSPM.Domain.Extensions;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Text.Json.Serialization.Metadata;

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
                    s.AddAtspmEFRepositories();

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






            var list = new List<ControllerEventLog>() { new ControllerEventLog(), new ControllerEventLog()};


            var test = Newtonsoft.Json.JsonConvert.SerializeObject(list, new JsonSerializerSettings()
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Arrays
            });

            Console.WriteLine($"{test}");

            List<ControllerEventLog> test2 = (List<ControllerEventLog>)JsonConvert.DeserializeObject(test, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            Console.WriteLine($"{test2.Count}");



            Console.Read();

        }
    }

    public class DeviceFtpDownloader : DeviceDownloaderBase
    {
        public DeviceFtpDownloader(IFTPDownloaderClient client, ILogger<DeviceFtpDownloader> log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> options) : base(client, log, options) { }

        public override TransportProtocols Protocol => TransportProtocols.Ftp;
    }

    public class DeviceSftpDownloader : DeviceDownloaderBase
    {
        public DeviceSftpDownloader(ISFTPDownloaderClient client, ILogger<DeviceSftpDownloader> log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> options) : base(client, log, options) { }

        public override TransportProtocols Protocol => TransportProtocols.Sftp;
    }

    public class DeviceHttpDownloader : DeviceDownloaderBase
    {
        public DeviceHttpDownloader(IHTTPDownloaderClient client, ILogger<DeviceHttpDownloader> log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> options) : base(client, log, options) { }

        public override TransportProtocols Protocol => TransportProtocols.Http;
    }

    public class DeviceSnmpDownloader : DeviceDownloaderBase
    {
        public DeviceSnmpDownloader(ISNMPDownloaderClient client, ILogger<DeviceSnmpDownloader> log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> options) : base(client, log, options) { }

        public override TransportProtocols Protocol => TransportProtocols.Snmp;
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