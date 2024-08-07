﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for EventLogUtility - ATSPM.EventLogUtility.Commands/SignalInfoCommand.cs
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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.NamingConventionBinder;
using Utah.Udot.Atspm.Data.Models;

namespace ATSPM.EventLogUtility.Commands
{
    public class LocationInfoCommand : Command, ICommandOption<EventLogLocationInfoConfiguration>
    {
        public LocationInfoCommand() : base("Location-info", "Gets information about Location controllers")
        {
            IncludeOption.AddValidator(r =>
            {
                if (r.GetValueForOption(ExcludeOption)?.Count() > 0)
                    r.ErrorMessage = "Can't use include option when also using exclude option";
            });
            ExcludeOption.AddValidator(r =>
            {
                if (r.GetValueForOption(IncludeOption)?.Count() > 0)
                    r.ErrorMessage = "Can't use exclude option when also using include option";
            });

            AddOption(IncludeOption);
            AddOption(ExcludeOption);
            AddOption(TypeOption);
        }

        public LocationIncludeCommandOption IncludeOption { get; set; } = new();

        public LocationExcludeCommandOption ExcludeOption { get; set; } = new();

        public LocationTypeCommandOption TypeOption { get; set; } = new();

        public ModelBinder<EventLogLocationInfoConfiguration> GetOptionsBinder()
        {
            var binder = new ModelBinder<EventLogLocationInfoConfiguration>();

            binder.BindMemberFromValue(b => b.Included, IncludeOption);
            binder.BindMemberFromValue(b => b.Excluded, ExcludeOption);
            binder.BindMemberFromValue(b => b.ControllerTypes, TypeOption);

            return binder;
        }

        public void BindCommandOptions(IServiceCollection services)
        {
            services.AddSingleton(GetOptionsBinder());
            services.AddOptions<EventLogLocationInfoConfiguration>().BindCommandLine();
            services.AddHostedService<SignaInfoHostedService>();
        }
    }

    public class SignaInfoHostedService : IHostedService
    {
        private readonly ILogger _log;
        private IServiceProvider _serviceProvider;
        private IOptions<EventLogLocationInfoConfiguration> _options;

        public SignaInfoHostedService(ILogger<SignaInfoHostedService> log, IServiceProvider serviceProvider, IOptions<EventLogLocationInfoConfiguration> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
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

                    foreach (var s in Locations)
                    {
                        PrintInfo(s);
                    }

                }
            }
            catch (Exception e)
            {
                _log.LogError("Exception: {e}", e);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine();
            Console.WriteLine($"Operation Completed or Cancelled...");

            return Task.CompletedTask;
        }

        private void PrintInfo(Location Location)
        {
            Console.WriteLine();
            Console.WriteLine($"-------------------------------------------------------------");

            Console.WriteLine($"key: {Location.Id}");
            Console.WriteLine($"id: {Location.LocationIdentifier}");
            Console.WriteLine($"primary name: {Location.PrimaryName}");
            Console.WriteLine($"secondary name: {Location.SecondaryName}");
            //Console.WriteLine($"ip address: {Location.Ipaddress}");
            //Console.WriteLine($"controller type: {Location.ControllerType}");
            Console.WriteLine($"enabled: {Location.ChartEnabled}");
            Console.WriteLine($"jurisdiction: {Location.Jurisdiction}");
            Console.WriteLine($"region: {Location.Region}");
            Console.WriteLine($"long/lat: {Location.Longitude}/{Location.Latitude}");

            Console.WriteLine($"-------------------------------------------------------------");
        }
    }
}
