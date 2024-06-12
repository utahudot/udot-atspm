#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Services.HostedServices/SignalLoggerUtilityHostedService.cs
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
using ATSPM.Application.Configuration;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Services.HostedServices
{
    public class LocationLoggerUtilityHostedService : IHostedService
    {
        private readonly ILogger _log;
        private IServiceProvider _serviceProvider;
        private IOptions<EventLogLoggingConfiguration> _options;

        public LocationLoggerUtilityHostedService(ILogger<LocationLoggerUtilityHostedService> log, IServiceProvider serviceProvider, IOptions<EventLogLoggingConfiguration> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _log.LogInformation("Log Path: {path}", _options.Value.Path);

                using (var scope = _serviceProvider.CreateAsyncScope())
                {
                    var LocationRepository = scope.ServiceProvider.GetService<ILocationRepository>();
                    var controllerLoggingService = scope.ServiceProvider.GetService<ILocationControllerLoggerService>();

                    var LocationQuery = LocationRepository.GetLatestVersionOfAllLocations().Where(s => s.ChartEnabled);

                    //if (_options.Value.ControllerTypes != null)
                    //{
                    //    foreach (var s in _options.Value.ControllerTypes)
                    //    {
                    //        _log.LogInformation("Including Event Logs for Types(s): {type}", s);
                    //    }

                    //    LocationQuery = LocationQuery.Where(i => _options.Value.ControllerTypes.Any(d => i.ControllerTypeId == d));
                    //}

                    if (_options.Value.Included != null)
                    {
                        foreach (var s in _options.Value.Included)
                        {
                            _log.LogInformation("Including Event Logs for Location(s): {Location}", s);
                        }

                        LocationQuery = LocationQuery.Where(i => _options.Value.Included.Any(d => i.LocationIdentifier == d));
                    }

                    if (_options.Value.Excluded != null)
                    {
                        foreach (var s in _options.Value.Excluded)
                        {
                            _log.LogInformation("Excluding Event Logs for Location(s): {Location}", s);
                        }

                        LocationQuery = LocationQuery.Where(i => !_options.Value.Excluded.Contains(i.LocationIdentifier));
                    }

                    var Locations = LocationQuery.ToList();

                    _log.LogInformation("Number of Locations to Process: {count}", Locations.Count);

                    await controllerLoggingService.ExecuteAsync(Locations, cancellationToken);
                }
            }
            catch (Exception e)
            {
                _log.LogError("Exception: {e}", e);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine();
            Console.WriteLine($"Operation Completed or Cancelled...");

            return Task.CompletedTask;
        }
    }
}