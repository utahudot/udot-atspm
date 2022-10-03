using ATSPM.Application.Configuration;
using ATSPM.Application.Repositories;
using ATSPM.Application.Services;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Data;
using ATSPM.Domain.Common;
using ATSPM.Infrastructure.Converters;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Repositories;
using ATSPM.Infrastructure.Services.ControllerDecoders;
using ATSPM.Infrastructure.Services.ControllerDownloaders;
using ATSPM.Infrastructure.Services.SignalControllerLoggers;
using ControllerEventLogExportUtility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

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

        //s.AddLogging();
        s.AddDbContext<EventLogContext>(db => db.UseSqlServer(h.Configuration.GetConnectionString(nameof(EventLogContext)), opt => opt.MigrationsAssembly(typeof(ServiceExtensions).Assembly.FullName)).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(h.HostingEnvironment.IsDevelopment()));

        //s.AddATSPMDbContext(h);

        //background services
        s.AddHostedService<ExportUtilityService>();

        //repositories
        s.AddScoped<IControllerEventLogRepository, ControllerEventLogEFRepository>();
    })

    .UseConsoleLifetime()
    .Build();

await host.RunAsync();

//Console.Read();

//Console.ReadKey();