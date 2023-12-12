using ATSPM.Application.Configuration;
using ATSPM.Application.Repositories;
using ATSPM.Application.Services;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Domain.Common;
using ATSPM.Infrastructure.Converters;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Repositories;
using ATSPM.Infrastructure.Services.ControllerDecoders;
using ATSPM.Infrastructure.Services.ControllerDownloaders;
using ATSPM.Infrastructure.Services.SignalControllerLoggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SignalControllerLogger;
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

namespace ATSPM.SignalControllerLogger
{
    class Program
    {
        static async Task Main(string[] args)
        {

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
                    s.AddHostedService<LoggerBackgroundService>();

                    //repositories
                    s.AddScoped<ILocationRepository, LocationEFRepository>();
                    //s.AddScoped<ILocationRepository, SignalFileRepository>();
                    s.AddScoped<IControllerEventLogRepository, ControllerEventLogEFRepository>();
                    //s.AddScoped<IControllerEventLogRepository, ControllerEventLogFileRepository>();

                    //s.AddTransient<IFileTranscoder, JsonFileTranscoder>();
                    //s.AddTransient<IFileTranscoder, ParquetFileTranscoder>();
                    s.AddTransient<IFileTranscoder, CompressedJsonFileTranscoder>();

                    ////downloader clients
                    s.AddTransient<IHTTPDownloaderClient, HttpDownloaderClient>();
                    s.AddTransient<IFTPDownloaderClient, FluentFTPDownloaderClient>();
                    s.AddTransient<ISFTPDownloaderClient, SSHNetSFTPDownloaderClient>();

                    //downloaders
                    s.AddScoped<ISignalControllerDownloader, ASC3SignalControllerDownloader>();
                    s.AddScoped<ISignalControllerDownloader, CobaltSignalControllerDownloader>();
                    s.AddScoped<ISignalControllerDownloader, MaxTimeSignalControllerDownloader>();
                    s.AddScoped<ISignalControllerDownloader, EOSSignalControllerDownloader>();
                    s.AddScoped<ISignalControllerDownloader, NewCobaltSignalControllerDownloader>();

                    //decoders
                    s.AddScoped<ISignalControllerDecoder, ASCSignalControllerDecoder>();
                    s.AddScoped<ISignalControllerDecoder, MaxTimeSignalControllerDecoder>();

                    //SignalControllerDataFlow
                    //s.AddScoped<ISignalControllerLoggerService, CompressedSignalControllerLogger>();
                    s.AddScoped<ISignalControllerLoggerService, LegacySignalControllerLogger>();

                    //controller logger configuration
                    s.Configure<SignalControllerLoggerConfiguration>(h.Configuration.GetSection(nameof(SignalControllerLoggerConfiguration)));

                    //downloader configurations
                    s.Configure<SignalControllerDownloaderConfiguration>(nameof(ASC3SignalControllerDownloader), h.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(ASC3SignalControllerDownloader)}"));
                    s.Configure<SignalControllerDownloaderConfiguration>(nameof(CobaltSignalControllerDownloader), h.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(CobaltSignalControllerDownloader)}"));
                    s.Configure<SignalControllerDownloaderConfiguration>(nameof(MaxTimeSignalControllerDownloader), h.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(MaxTimeSignalControllerDownloader)}"));
                    s.Configure<SignalControllerDownloaderConfiguration>(nameof(EOSSignalControllerDownloader), h.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(EOSSignalControllerDownloader)}"));
                    s.Configure<SignalControllerDownloaderConfiguration>(nameof(NewCobaltSignalControllerDownloader), h.Configuration.GetSection($"{nameof(SignalControllerDownloaderConfiguration)}:{nameof(NewCobaltSignalControllerDownloader)}"));

                    //decoder configurations
                    s.Configure<SignalControllerDecoderConfiguration>(nameof(ASCSignalControllerDecoder), h.Configuration.GetSection($"{nameof(SignalControllerDecoderConfiguration)}:{nameof(ASCSignalControllerDecoder)}"));
                    s.Configure<SignalControllerDecoderConfiguration>(nameof(MaxTimeSignalControllerDecoder), h.Configuration.GetSection($"{nameof(SignalControllerDecoderConfiguration)}:{nameof(MaxTimeSignalControllerDecoder)}"));

                    s.Configure<FileRepositoryConfiguration>(h.Configuration.GetSection("FileRepositoryConfiguration"));
                })

                .UseConsoleLifetime()
                .Build();

            //await host.RunAsync();

            using (var scope = host.Services.CreateScope())
            {
                var repo = scope.ServiceProvider.GetService<IRegionsRepository>();

                var test = repo.GetList().ToList();
            }

            Console.Read();

        }
    }


}