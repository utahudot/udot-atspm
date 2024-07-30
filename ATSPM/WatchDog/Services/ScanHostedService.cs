﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for WatchDog - %Namespace%/ScanHostedService.cs
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WatchDog.Models;

namespace WatchDog.Services
{
    public class ScanHostedService : IHostedService
    {
        private readonly ScanService scanService;
        private readonly ILogger<ScanHostedService> logger;
        private readonly DateTime scanDate;
        private readonly IHostApplicationLifetime appLifetime;

        // Add other dependencies if needed, like IConfiguration

        public ScanHostedService(ScanService scanService, ILogger<ScanHostedService> logger, DateTime scanDate, IHostApplicationLifetime appLifetime)
        {
            this.scanService = scanService;
            this.logger = logger;
            this.scanDate = scanDate;
            this.appLifetime = appLifetime;
            // Assign other dependencies
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            try
            {
                var prviousDayPMPeakStart = Convert.ToInt32(configuration["PreviousDayPMPeakStart"]);
                var prviousDayPMPeakEnd = Convert.ToInt32(configuration["PreviousDayPMPeakEnd"]);
                var weekdayOnly = Convert.ToBoolean(configuration["WeekdayOnly"]);
                var scanDayEndHour = Convert.ToInt32(configuration["ScanDayEndHour"]);
                var scanDayStartHour = Convert.ToInt32(configuration["ScanDayStartHour"]);
                var options = new LoggingOptions
                {
                    ConsecutiveCount = Convert.ToInt32(configuration["ConsecutiveCount"]),
                    LowHitThreshold = Convert.ToInt32(configuration["LowHitThreshold"]),
                    MaximumPedestrianEvents = Convert.ToInt32(configuration["MaximumPedestrianEvents"]),
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
                    //EmailServer = configuration["EmailServer"],
                    //UserName = configuration["UserName"],
                    //Password = configuration["Password"],
                    //Port = Convert.ToInt32(configuration["Port"]),
                    //EnableSsl = Convert.ToBoolean(configuration["EnableSsl"]),
                    DefaultEmailAddress = configuration["DefaultEmailAddress"],
                    EmailAllErrors = Convert.ToBoolean(configuration["EmailAllErrors"]),
                    //EmailType = configuration["EmailType"]
                };
                await scanService.StartScan(options, emailOptions, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during scanning.");
            }
            finally
            {
                appLifetime.StopApplication();
            }

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Perform any cleanup if necessary
            return Task.CompletedTask;
        }
    }
}