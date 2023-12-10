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

            .AddScoped<ISignalRepository, SignalEFRepository>()
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
                // Call the StartScan method with appropriate LoggingOptions
                var options = new LoggingOptions
                {
                    ConsecutiveCount = 3, // Set your options here
                    LowHitThreshold = 50, // Set your options here
                    MaxDegreeOfParallelism = 5, // Set your options here
                    MaximumPedestrianEvents = 200, // Set your opti
                    MinimumRecords = 500,
                    MinPhaseTerminations = 50,
                    PercentThreshold = 0.9,
                    PreviousDayPMPeakEnd = 17,
                    PreviousDayPMPeakStart = 18,
                    ScanDate = new DateTime(2023, 12, 8),
                    ScanDayEndHour = 5,
                    ScanDayStartHour = 1,
                    WeekdayOnly = false
                };
                var emailOptions = new EmailOptions
                {
                    PreviousDayPMPeakEnd = 17,
                    PreviousDayPMPeakStart = 18,
                    ScanDate = new DateTime(2023, 12, 8),
                    ScanDayEndHour = 5,
                    ScanDayStartHour = 1,
                    WeekdayOnly = false,
                    EmailServer = "smtp.freesmtpservers.com",
                    //UserName = "241b4c03c87968",
                    //Password = "0f894391e4e8d3",
                    Port = 25,
                    //EnableSsl = true,
                    DefaultEmailAddress = "derekjlowe@gmail.com",
                    EmailAllErrors = true
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