using ATSPM.Application.Configuration;
using ATSPM.Application.Models;
using ATSPM.Application.Repositories;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Domain.Common;
using ATSPM.Infrasturcture.Converters;
using ATSPM.Infrasturcture.Data;
using ATSPM.Infrasturcture.Repositories;
using ATSPM.Infrasturcture.Services.ControllerDecoders;
using ATSPM.Infrasturcture.Services.ControllerDownloaders;
using FluentFTP;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Diagnostics.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

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
                    //if (h.Configuration.GetValue<bool>("UseGoogleLogger"))
                    //{
                    //    l.AddGoogle(new LoggingServiceOptions
                    //    {
                    //        ProjectId = "1022556126938",
                    //        //ProjectId = "869261868126",
                    //        ServiceName = AppDomain.CurrentDomain.FriendlyName,
                    //        Version = Assembly.GetEntryAssembly().GetName().Version.ToString()
                    //        //Options = LoggingOptions.Create(LogLevel.Warning, AppDomain.CurrentDomain.FriendlyName)
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
                    s.AddDbContext<DbContext, MOEContext>(db => db.UseSqlServer(h.Configuration.GetConnectionString(h.HostingEnvironment.EnvironmentName))); //b => b.UseLazyLoadingProxies().UseChangeTrackingProxies()

                    //background services
                    s.AddHostedService<TPLDataflowService>();

                    //repositories
                    s.AddScoped<IActionLogRepository, ActionLogEFRepository>();
                    s.AddScoped<IApplicationEventRepository, ApplicationEventEFRepository>();
                    s.AddScoped<IApplicationSettingsRepository, ApplicationSettingsEFRepository>();
                    s.AddScoped<IApproachCycleAggregationRepository, ApproachCycleAggregationEFRepository>();
                    s.AddScoped<IApproachPcdAggregationRepository, ApproachPcdAggregationEFRepository>();
                    s.AddScoped<IApproachRepository, ApproachEFRepository>();
                    s.AddScoped<IApproachSpeedAggregationRepository, ApproachSpeedAggregationEFRepository>();
                    s.AddScoped<IApproachSplitFailAggregationRepository, ApproachSplitFailAggregationEFRepository>();
                    s.AddScoped<IApproachYellowRedActivationsAggregationRepository, ApproachYellowRedActivationsAggregationEFRepository>();
                    s.AddScoped<IAreaRepository, AreaEFRepository>();
                    s.AddScoped<IControllerTypeRepository, ControllerTypeEFRepository>();
                    s.AddScoped<IDatabaseArchiveExcludedSignalsRepository, DatabaseArchiveExcludedSignalsEFRepository>();
                    s.AddScoped<IDetectionHardwareRepository, DetectionHardwareEFRepository>();
                    s.AddScoped<IDetectionTypeRepository, DetectionTypeEFRepository>();
                    s.AddScoped<IDetectorCommentRepository, DetectorCommentEFRepository>();
                    s.AddScoped<IDetectorEventCountAggregationRepository, DetectorEventCountAggregationEFRepository>();
                    s.AddScoped<IDetectorRepository, DetectorEFRepository>();
                    s.AddScoped<IDetectionTypeRepository, DetectionTypeEFRepository>();
                    s.AddScoped<IFAQRepository, FAQEFRepository>();
                    s.AddScoped<IJurisdictionRepository, JurisdictionEFRepository>();
                    s.AddScoped<ILaneTypeRepository, LaneTypeEFRepository>();
                    s.AddScoped<IMeasuresDefaultsRepository, MeasuresDefaultsEFRepository>();
                    s.AddScoped<IMenuRepository, MenuEFRepository>();
                    s.AddScoped<IMetricCommentRepository, MetricCommentEFRepository>();
                    s.AddScoped<IMetricFilterTypesRepository, MetricFilterTypesEFRepository>();
                    s.AddScoped<IMetricTypeRepository, MetricTypeEFRepository>();
                    s.AddScoped<IMovementTypeRepository, MovementTypeEFRepository>();
                    s.AddScoped<IPhaseCycleAggregationRepository, PhaseCycleAggregationEFRepository>();
                    s.AddScoped<IPhaseLeftTurnGapAggregationRepository, PhaseLeftTurnGapAggregationEFRepository>();
                    s.AddScoped<IPhasePedAggregationRepository, PhasePedAggregationEFRepository>();
                    s.AddScoped<IPhaseSplitMonitorAggregationRepository, PhaseSplitMonitorAggregationEFRepository>();
                    s.AddScoped<IPhaseTerminationAggregationRepository, PhaseTerminationAggregationEFRepository>();
                    s.AddScoped<IPreemptAggregationDatasRepository, PreemptAggregationDatasEFRepository>();
                    s.AddScoped<IPriorityAggregationDatasRepository, PriorityAggregationDatasEFRepository>();
                    s.AddScoped<IRegionsRepository, RegionsEFRepository>();
                    s.AddScoped<IRouteRepository, RouteEFRepository>();
                    s.AddScoped<IRoutePhaseDirectionRepository, RoutePhaseDirectionEFRepository>();
                    s.AddScoped<IRouteSignalsRepository, RouteSignalsEFRepository>();
                    s.AddScoped<ISignalEventCountAggregationRepository, SignalEventCountAggregationEFRepository>();
                    s.AddScoped<ISpeedEventRepository, SpeedEventEFRepository>();
                    s.AddScoped<ISPMMenuRepository, SPMMenuEFRepository>();
                    s.AddScoped<ISPMWatchDogErrorEventRepository, SPMWatchDogErrorEventEFRepository>();

                    s.AddScoped<ISignalRepository, SignalEFRepository>();
                    //s.AddScoped<ISignalRepository, SignalFileRepository>();
                    s.AddScoped<IControllerEventLogRepository, ControllerEventLogEFRepository>();
                    //s.AddScoped<IControllerEventLogRepository, ControllerEventLogFileRepository>();


                    //s.AddTransient<IFileTranscoder, JsonFileTranscoder>();
                    //s.AddTransient<IFileTranscoder, ParquetFileTranscoder>();
                    s.AddTransient<IFileTranscoder, CompressedJsonFileTranscoder>();

                    //downloader clients
                    s.AddTransient<IHTTPDownloaderClient, HttpDownloaderClient>();
                    s.AddTransient<IFTPDownloaderClient, FluentFTPDownloaderClient>();
                    //s.AddTransient<IFTPDownloaderClient, FTPDownloaderStubClient>();
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

                    //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0
                    //downloader configurations
                    s.Configure<SignalControllerDownloaderConfiguration>(nameof(ASC3SignalControllerDownloader), h.Configuration.GetSection(nameof(ASC3SignalControllerDownloader)));
                    s.Configure<SignalControllerDownloaderConfiguration>(nameof(CobaltSignalControllerDownloader), h.Configuration.GetSection(nameof(CobaltSignalControllerDownloader)));
                    s.Configure<SignalControllerDownloaderConfiguration>(nameof(MaxTimeSignalControllerDownloader), h.Configuration.GetSection(nameof(MaxTimeSignalControllerDownloader)));
                    s.Configure<SignalControllerDownloaderConfiguration>(nameof(EOSSignalControllerDownloader), h.Configuration.GetSection(nameof(EOSSignalControllerDownloader)));
                    s.Configure<SignalControllerDownloaderConfiguration>(nameof(NewCobaltSignalControllerDownloader), h.Configuration.GetSection(nameof(NewCobaltSignalControllerDownloader)));

                    s.Configure<FileRepositoryConfiguration>(h.Configuration.GetSection("FileRepositoryConfiguration"));
                })

                .UseConsoleLifetime()
                .Build();

            await host.RunAsync();

            Console.WriteLine($"done?");

            Console.ReadKey();
        }
    }
}