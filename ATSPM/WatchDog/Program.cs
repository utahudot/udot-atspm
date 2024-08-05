﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for WatchDog - %Namespace%/Program.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using ATSPM.Application.Business.Common;
using ATSPM.Application.Repositories;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Data;
using ATSPM.Infrastructure.Extensions;
using ATSPM.Infrastructure.Repositories;
using ATSPM.Infrastructure.Repositories.ConfigurationRepositories;
using ATSPM.Infrastructure.Repositories.EventLogRepositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WatchDog.Services;

namespace WatchDog
{
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
                       //string emailType = h.Configuration.GetValue<string>("EmailType");
                       //if (emailType == "smtp")
                       //{
                       //    s.AddScoped<IMailService>(serviceProvider =>
                       //    {
                       //        var config = h.Configuration;
                       //        var smtpHost = config.GetValue<string>("SmtpSettings:Host");
                       //        var smtpPort = config.GetValue<int>("SmtpSettings:Port");
                       //        var smtpUser = config.GetValue<string>("SmtpSettings:UserName");
                       //        var smtpPass = config.GetValue<string>("SmtpSettings:Password");
                       //        var enableSsl = config.GetValue<bool>("SmtpSettings:EnableSsl");

                       //        // Now create the SMTPMailService instance with the parameters
                       //        return new SMTPMailService(
                       //            config,
                       //            serviceProvider.GetRequiredService<ILogger<SMTPMailService>>());
                       //    });
                       //}
                       //else
                       //{
                       //}

                       //s.AddScoped<EmailService>(serviceProvider =>
                       //{
                       //    return new EmailService(
                       //        serviceProvider.GetRequiredService<ILogger<EmailService>>(),
                       //        serviceProvider.GetRequiredService<IMailService>());
                       //}
                       //);

                       s.AddEmailServices(h);
                       s.AddScoped<WatchdogEmailService>();

                       s.AddAtspmDbContext(h);
                       s.AddScoped<ILocationRepository, LocationEFRepository>();
                       s.AddScoped<IIndianaEventLogRepository, IndianaEventLogEFRepository>();
                       s.AddScoped<IWatchDogEventLogRepository, WatchDogLogEventEFRepository>();
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
                       s.AddHostedService(serviceProvider =>
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
}