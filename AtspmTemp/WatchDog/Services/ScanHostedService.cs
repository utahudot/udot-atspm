#region license
// Copyright 2024 Utah Departement of Transportation
// for WatchDog - WatchDog.Services/ScanHostedService.cs
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
using Utah.Udot.Atspm.WatchDog.Models;

namespace Utah.Udot.Atspm.WatchDog.Services
{
    public class ScanHostedService : IHostedService
    {
        private readonly ScanService _scanService;
        private readonly ILogger<ScanHostedService> _log;
        private readonly IConfiguration _config;

        public ScanHostedService(ScanService scanService, ILogger<ScanHostedService> logger, IConfiguration config)
        {
            _scanService = scanService;
            _log = logger;
            _config = config;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var prviousDayPMPeakStart = Convert.ToInt32(_config["PreviousDayPMPeakStart"]);
                var prviousDayPMPeakEnd = Convert.ToInt32(_config["PreviousDayPMPeakEnd"]);
                var weekdayOnly = Convert.ToBoolean(_config["WeekdayOnly"]);
                var scanDayEndHour = Convert.ToInt32(_config["ScanDayEndHour"]);
                var scanDayStartHour = Convert.ToInt32(_config["ScanDayStartHour"]);
                var options = new LoggingOptions
                {
                    ConsecutiveCount = Convert.ToInt32(_config["ConsecutiveCount"]),
                    LowHitThreshold = Convert.ToInt32(_config["LowHitThreshold"]),
                    MaximumPedestrianEvents = Convert.ToInt32(_config["MaximumPedestrianEvents"]),
                    MinimumRecords = Convert.ToInt32(_config["MinimumRecords"]),
                    MinPhaseTerminations = Convert.ToInt32(_config["MinPhaseTerminations"]),
                    PercentThreshold = Convert.ToDouble(_config["PercentThreshold"]),
                    PreviousDayPMPeakEnd = prviousDayPMPeakEnd,
                    PreviousDayPMPeakStart = prviousDayPMPeakStart,
                    ScanDate = DateTime.TryParse(_config["ScanDate"], out DateTime date1) ? DateTime.Parse(_config["ScanDate"]) : DateTime.Today.AddDays(-1),
                    ScanDayEndHour = scanDayEndHour,
                    ScanDayStartHour = scanDayStartHour,
                    WeekdayOnly = weekdayOnly
                };
                var emailOptions = new EmailOptions
                {
                    PreviousDayPMPeakEnd = prviousDayPMPeakEnd,
                    PreviousDayPMPeakStart = prviousDayPMPeakStart,
                    ScanDate = DateTime.TryParse(_config["ScanDate"], out DateTime date2) ? DateTime.Parse(_config["ScanDate"]) : DateTime.Today.AddDays(-1),
                    ScanDayEndHour = scanDayStartHour,
                    ScanDayStartHour = scanDayEndHour,
                    WeekdayOnly = weekdayOnly,
                    DefaultEmailAddress = _config["DefaultEmailAddress"],
                    EmailAllErrors = Convert.ToBoolean(_config["EmailAllErrors"]),
                };
                await _scanService.StartScan(options, emailOptions, cancellationToken);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "An error occurred during scanning.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}