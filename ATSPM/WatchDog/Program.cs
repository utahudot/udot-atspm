// See https://aka.ms/new-console-template for more information
using ATSPM.Application.Repositories;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Repositories;
using ATSPM.Infrastructure.Repositories.ConfigurationRepositories;
using ATSPM.ReportApi.Business.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WatchDog.Services;

class Program
{
    static async Task Main(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var host = Host.CreateDefaultBuilder()
               //.ConfigureWebHostDefaults(webBuilder =>
               //{
               //    webBuilder.UseEnvironment("Development"); // Setting the environment
               //})
               .ConfigureLogging((h, l) =>
               {
               })
               .ConfigureServices((h, s) =>
               {
                   s.AddAtspmDbContext(h);
                   s.AddScoped<ILocationRepository, LocationEFRepository>();
                   s.AddScoped<IControllerEventLogRepository, ControllerEventLogEFRepository>();
                   s.AddScoped<IWatchDogLogEventRepository, WatchDogLogEventEFRepository>();
                   s.AddScoped<IRegionsRepository, RegionEFRepository>();
                   s.AddScoped<IJurisdictionRepository, JurisdictionEFRepository>();
                   s.AddScoped<IAreaRepository, AreaEFRepository>();
                   s.AddScoped<IUserAreaRepository, UserAreaEFRepository>();
                   s.AddScoped<IUserRegionRepository, UserRegionEFRepository>();
                   s.AddScoped<IUserJurisdictionRepository, UserJurisdictionEFRepository>();
                   s.AddScoped<WatchDogLogService>();
                   s.AddScoped<EmailService>();
                   s.AddTransient<ScanService>();
                   s.AddScoped<PlanService>();
                   s.AddScoped<AnalysisPhaseCollectionService>();
                   s.AddScoped<AnalysisPhaseService>();
                   s.AddScoped<PhaseService>();
                   DateTime scanDate = args.Length > 0 ? DateTime.Parse(args[0]) : DateTime.Today.AddDays(-1);

                   // Register the hosted service with the date
                   s.AddHostedService<ScanHostedService>(serviceProvider =>
                       new ScanHostedService(
                           serviceProvider.GetRequiredService<ScanService>(),
                           serviceProvider.GetRequiredService<ILogger<ScanHostedService>>(),
                           scanDate // Pass the parsed date here
                       ));
                   s.AddLogging(builder =>
                    {
                        builder.AddConsole(); // Use Console logger provider
                    });
                   s.AddIdentity<ApplicationUser, IdentityRole>() // Add this line to register Identity
                    .AddEntityFrameworkStores<IdentityContext>() // Specify the EF Core store
                    .AddDefaultTokenProviders();

               })
               .ConfigureAppConfiguration((context, config) =>
               {
                   // Add other configuration files, environment variables, etc.
                   if (context.HostingEnvironment.IsDevelopment())
                   {
                       config.AddUserSecrets<Program>();
                   }
               })
                .UseConsoleLifetime()
                .Build();
        await host.RunAsync();

    }
}
