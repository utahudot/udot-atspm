// See https://aka.ms/new-console-template for more information

using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Infrastructure.Repositories;
using ATSPM.ReportApi.Business.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WatchDog.Models;
using WatchDog.Services;

class Program
{
    static async Task Main(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();


        var serviceCollection = new ServiceCollection()
            .AddDbContext<ConfigContext>(db => db.UseNpgsql(configuration.GetConnectionString("ConfigContext")))
            .AddDbContext<IdentityContext>(db => db.UseNpgsql(configuration.GetConnectionString("IdentityContext")))
            .AddDbContext<EventLogContext>(db => db.UseNpgsql(configuration.GetConnectionString("EventLogContext")))

            .AddScoped<ILocationRepository, LocationEFRepository>()
            .AddScoped<IControllerEventLogRepository, ControllerEventLogEFRepository>()
            .AddScoped<IWatchDogLogEventRepository, WatchDogLogEventEFRepository>()
            .AddScoped<IRegionsRepository, RegionEFRepository>()
            .AddScoped<IJurisdictionRepository, JurisdictionEFRepository>()
            .AddScoped<IAreaRepository, AreaEFRepository>()
            .AddScoped<IUserAreaRepository, UserAreaEFRepository>()
            .AddScoped<IUserRegionRepository, UserRegionEFRepository>()
            .AddScoped<IUserJurisdictionRepository, UserJurisdictionEFRepository>()
            .AddScoped<WatchDogLogService>()
            .AddScoped<EmailService>()
            .AddScoped<ScanService>()
            .AddScoped<PlanService>()
            .AddScoped<AnalysisPhaseCollectionService>()
            .AddScoped<AnalysisPhaseService>()
            .AddScoped<PhaseService>()
            //.AddScoped<UserManager<ApplicationUser>>()
            //.AddScoped<RoleManager<IdentityRole>>()
            .AddLogging(builder =>
            {
                builder.AddConsole(); // Use Console logger provider
            });

        serviceCollection
             .AddIdentity<ApplicationUser, IdentityRole>() // Add this line to register Identity
                .AddEntityFrameworkStores<IdentityContext>() // Specify the EF Core store
                .AddDefaultTokenProviders();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var configContext = scope.ServiceProvider.GetRequiredService<ConfigContext>();
            var identityContext = scope.ServiceProvider.GetRequiredService<IdentityContext>();
            var eventLogContext = scope.ServiceProvider.GetRequiredService<EventLogContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
            var scanService = scope.ServiceProvider.GetRequiredService<ScanService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            var phaseService = scope.ServiceProvider.GetRequiredService<PhaseService>();


            try
            {
                var scanDate = args.Length == 0 ? DateTime.Today.AddDays(-1) : Convert.ToDateTime(args[0]);
                var prviousDayPMPeakStart = Convert.ToInt32(configuration["PreviousDayPMPeakStart"]);
                var prviousDayPMPeakEnd = Convert.ToInt32(configuration["PreviousDayPMPeakEnd"]);
                var weekdayOnly = Convert.ToBoolean(configuration["WeekdayOnly"]);
                var scanDayEndHour = Convert.ToInt32(configuration["ScanDayEndHour"]);
                var scanDayStartHour = Convert.ToInt32(configuration["ScanDayStartHour"]);
                if (weekdayOnly && (scanDate.DayOfWeek == DayOfWeek.Saturday || scanDate.DayOfWeek == DayOfWeek.Sunday))
                {
                    return;
                }

                // Call the StartScan method with appropriate LoggingOptions
                var options = new LoggingOptions
                {
                    ConsecutiveCount = Convert.ToInt32(configuration["ConsecutiveCount"]), // Set your options here
                    LowHitThreshold = Convert.ToInt32(configuration["LowHitThreshold"]), // Set your options here
                    MaxDegreeOfParallelism = Convert.ToInt32(configuration["MaxDegreeOfParallelism"]), // Set your options here
                    MaximumPedestrianEvents = Convert.ToInt32(configuration["MaximumPedestrianEvents"]), // Set your opti
                    MinimumRecords = Convert.ToInt32(configuration["MinimumRecords"]),
                    MinPhaseTerminations = Convert.ToInt32(configuration["MinPhaseTerminations"]),
                    PercentThreshold = Convert.ToDouble(configuration["PercentThreshold"]),
                    PreviousDayPMPeakEnd = prviousDayPMPeakEnd,
                    PreviousDayPMPeakStart = prviousDayPMPeakStart,
                    ScanDate = scanDate,
                    ScanDayEndHour = scanDayEndHour,
                    ScanDayStartHour = scanDayStartHour,
                    WeekdayOnly = weekdayOnly
                };
                var emailOptions = new EmailOptions
                {
                    PreviousDayPMPeakEnd = prviousDayPMPeakEnd,
                    PreviousDayPMPeakStart = prviousDayPMPeakStart,
                    ScanDate = scanDate,
                    ScanDayEndHour = scanDayStartHour,
                    ScanDayStartHour = scanDayEndHour,
                    WeekdayOnly = weekdayOnly,
                    EmailServer = configuration["EmailServer"],
                    UserName = configuration["UserName"],
                    Password = configuration["Password"],
                    Port = Convert.ToInt32(configuration["Port"]),
                    EnableSsl = Convert.ToBoolean(configuration["EnableSsl"]),
                    DefaultEmailAddress = configuration["DefaultEmailAddress"],
                    EmailAllErrors = Convert.ToBoolean(configuration["EmailAllErrors"])
                };

                await scanService.StartScan(options, emailOptions);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during scanning.");
            }
        }
    }

    // The rest of your program remains the same
}