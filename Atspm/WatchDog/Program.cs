#region license
// Copyright 2024 Utah Departement of Transportation
// for WatchDog - WatchDog/Program.cs
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

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Utah.Udot.Atspm.Common;
using Utah.Udot.Atspm.Configuration;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Infrastructure.Extensions;
using Utah.Udot.Atspm.Infrastructure.Repositories;
using Utah.Udot.Atspm.Infrastructure.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Infrastructure.Repositories.EventLogRepositories;
using Utah.Udot.Atspm.Infrastructure.Services.EmailServices;
using Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices;
using Utah.Udot.NetStandardToolkit.Configuration;

namespace Utah.Udot.Atspm.WatchDog
{
    class Program
    {
        static async Task Main(string[] args)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var rootCmd = new WatchdogCommand();

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((h, c) => {
                    c.AddUserSecrets<Program>(optional: true);
                    c.AddCommandLine(args);

                })
                .ConfigureServices((h, s) =>
                {
                    s.AddEmailServices(h);
                    s.AddScoped<IEmailService, SmtpEmailService>();
                    s.AddScoped<WatchdogEmailService>();

                    s.AddAtspmDbContext(h);
                    s.AddScoped<ILocationRepository, LocationEFRepository>();
                    s.AddScoped<IWatchDogIgnoreEventRepository, WatchDogIgnoreEventEFRepository>();
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
                    s.AddScoped<SegmentedErrorsService>();
                    s.AddScoped<WatchDogIgnoreEventService>();

                    // Register the hosted service with the date
                    //s.AddHostedService<ScanHostedService>();
                    s.AddIdentity<ApplicationUser, IdentityRole>() // Add this line to register Identity
                     .AddEntityFrameworkStores<IdentityContext>() // Specify the EF Core store
                     .AddDefaultTokenProviders();

                    s.AddSingleton<WatchdogCommand>();
                    s.AddSingleton<ICommandOption<WatchdogConfiguration>, WatchdogCommand>();

                    // Other service registrations
                    s.AddOptions<WatchdogConfiguration>().Bind(h.Configuration.GetSection("WatchdogConfiguration"));
                    s.AddHostedService<ScanHostedService>();

                    //s.Configure<WatchdogConfiguration>(h.Configuration.GetSection(nameof(WatchdogConfiguration)));
                    //s.Configure<WatchdogConfiguration>(h.Configuration);
                    //s.AddScoped<WatchdogConfiguration>();
                    s.Configure<EmailConfiguration>(h.Configuration.GetSection("WatchdogConfiguration:EmailConfiguration"));

                })
                .Build();


            await host.StartAsync();
            await host.StopAsync();

        }
    }
}