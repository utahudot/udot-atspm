#region license
// Copyright 2024 Utah Departement of Transportation
// for LocationControllerLogger - LocationControllerLogger/LoggerBackgroundService.cs
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

using ATSPM.Application;
using ATSPM.Application.Configuration;
using ATSPM.Data.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ATSPM.Application.Services;
using ATSPM.Application.Repositories.ConfigurationRepositories;

namespace LocationControllerLogger
{
    public class LoggerBackgroundService : BackgroundService
    {
        private readonly ILogger _log;
        private IServiceProvider _serviceProvider;

        public LoggerBackgroundService(ILogger<LoggerBackgroundService> log, IServiceProvider serviceProvider) =>
            (_log, _serviceProvider) = (log, serviceProvider);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //while (!stoppingToken.IsCancellationRequested)
            //{

            List<Location> Locations;

            try
            {
                using (var scope = _serviceProvider.CreateAsyncScope())
                {
                    var LocationRepository = scope.ServiceProvider.GetService<ILocationRepository>();
                    var controllerLoggingService = scope.ServiceProvider.GetService<ILocationControllerLoggerService>();

                    //Locations = LocationRepository.GetLatestVersionOfAllLocations().Where(w => w.ChartEnabled && w.ControllerTypeId == 4).ToList();
                    //await controllerLoggingService.ExecuteAsync(Locations, stoppingToken);
                }
            }
            catch (Exception e)
            {

                _log.LogError("Exception: {e}", e);
            }

            _serviceProvider.GetService<IHostApplicationLifetime>().StopApplication();

            //    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            //}
        }
    }
}
