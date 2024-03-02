// See https://aka.ms/new-console-template for more information

using ATSPM.Application.Business.Common;
using ATSPM.Application.Repositories;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WatchDog.Models;
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
                   string emailType = h.Configuration.GetValue<string>("EmailType");
                   if (emailType == "smtp")
                   {
                       s.AddScoped<IMailService>(serviceProvider =>
                       {
                           var config = h.Configuration;
                           var smtpHost = config.GetValue<string>("SmtpSettings:Host");
                           var smtpPort = config.GetValue<int>("SmtpSettings:Port");
                           var smtpUser = config.GetValue<string>("SmtpSettings:UserName");
                           var smtpPass = config.GetValue<string>("SmtpSettings:Password");
                           var enableSsl = config.GetValue<bool>("SmtpSettings:EnableSsl");

                           // Now create the SMTPMailService instance with the parameters
                           return new SMTPMailService(
                               smtpHost,
                               smtpPort,
                               enableSsl,
                               smtpUser,
                               smtpPass,
                               serviceProvider.GetRequiredService<ILogger<SMTPMailService>>());
                       });
                   }
                   else
                   {
                   }


                   s.AddScoped<EmailService>(serviceProvider =>
                   {
                       return new EmailService(
                           serviceProvider.GetRequiredService<ILogger<EmailService>>(),
                           serviceProvider.GetRequiredService<IMailService>());
                   }
                   );

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
                           scanDate,
                           serviceProvider.GetRequiredService<IHostApplicationLifetime>()
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
