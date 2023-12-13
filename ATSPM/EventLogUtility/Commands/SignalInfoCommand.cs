using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CommandLine.Invocation;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using ATSPM.Application.Configuration;
using System.CommandLine.Binding;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine.NamingConventionBinder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Reflection.Metadata;
using ATSPM.Domain.Extensions;
using Microsoft.Extensions.Logging;
using ATSPM.Application.Repositories;
using ATSPM.Application.Services;
using ATSPM.Data.Models;

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

                    if (_options.Value.ControllerTypes != null)
                    {
                        foreach (var s in _options.Value.ControllerTypes)
                        {
                            _log.LogInformation("Including Event Logs for Types(s): {type}", s);
                        }

                        LocationQuery = LocationQuery.Where(i => _options.Value.ControllerTypes.Any(d => i.ControllerTypeId == d));
                    }

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
            Console.WriteLine($"ip address: {Location.Ipaddress}");
            Console.WriteLine($"controller type: {Location.ControllerType}");
            Console.WriteLine($"enabled: {Location.ChartEnabled}");
            Console.WriteLine($"jurisdiction: {Location.Jurisdiction}");
            Console.WriteLine($"region: {Location.Region}");
            Console.WriteLine($"long/lat: {Location.Longitude}/{Location.Latitude}");

            Console.WriteLine($"-------------------------------------------------------------");
        }
    }
}
