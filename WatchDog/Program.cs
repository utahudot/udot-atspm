// See https://aka.ms/new-console-template for more information

using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WatchDog.Models;
using WatchDog.Services;

class Program
{
    static void Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();


        var serviceProvider = new ServiceCollection()
            .AddDbContext<ConfigContext>(db => db.UseSqlServer(configuration.GetConnectionString("ConfigContext")))
            .AddDbContext<IdentityContext>(db => db.UseSqlServer(configuration.GetConnectionString("IdentityContext")))
            .AddDbContext<EventLogContext>(db => db.UseSqlServer(configuration.GetConnectionString("EventLogContext")))
            .AddScoped<ISignalRepository, SignalEFRepository>()
            .AddScoped<IControllerEventLogRepository, ControllerEventLogEFRepository>()
            .AddScoped<IWatchDogLogEventRepository, WatchDogLogEventEFRepository>()
            .AddScoped<WatchDogLogService>()
            .AddScoped<EmailService>()
            .AddScoped<ScanService>()
            .AddScoped<PlanService>()
            .AddScoped<AnalysisPhaseCollectionService>()
            .AddScoped<AnalysisPhaseService>()
            .AddScoped<UserManager<ApplicationUser>>()
            .AddLogging(builder =>
            {
                builder.AddConsole(); // Use Console logger provider
            })
            .BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var configContext = scope.ServiceProvider.GetRequiredService<ConfigContext>();
            var identityContext = scope.ServiceProvider.GetRequiredService<IdentityContext>();
            var eventLogContext = scope.ServiceProvider.GetRequiredService<EventLogContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
            var scanService = scope.ServiceProvider.GetRequiredService<ScanService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();


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
                    ScanDate = new DateTime(2023, 8, 24),
                    ScanDayEndHour = 5,
                    ScanDayStartHour = 1,
                    WeekdayOnly = false
                };
                var emailOptions = new EmailOptions
                {
                    PreviousDayPMPeakEnd = 17,
                    PreviousDayPMPeakStart = 18,
                    ScanDate = new DateTime(2023, 8, 24),
                    ScanDayEndHour = 5,
                    ScanDayStartHour = 1,
                    WeekdayOnly = false,
                    EmailServer = "sandbox.smtp.mailtrap.io",
                    FromEmailAddress = "241b4c03c87968",
                    Password = "0f894391e4e8d3",
                    Port = 587,
                    EnableSsl = true,
                    DefaultEmailAddress = "derekjlowe@gmail.com",
                    EmailAllErrors = true
                };

                scanService.StartScan(options, emailOptions);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during scanning.");
            }
        }
    }

    // The rest of your program remains the same
}
