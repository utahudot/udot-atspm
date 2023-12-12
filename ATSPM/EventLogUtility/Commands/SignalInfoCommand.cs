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
    public class SignalInfoCommand : Command, ICommandOption<EventLogSignalInfoConfiguration>
    {
        public SignalInfoCommand() : base("signal-info", "Gets information about signal controllers")
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

        public SignalIncludeCommandOption IncludeOption { get; set; } = new();

        public SignalExcludeCommandOption ExcludeOption { get; set; } = new();

        public SignalTypeCommandOption TypeOption { get; set; } = new();

        public ModelBinder<EventLogSignalInfoConfiguration> GetOptionsBinder()
        {
            var binder = new ModelBinder<EventLogSignalInfoConfiguration>();

            binder.BindMemberFromValue(b => b.Included, IncludeOption);
            binder.BindMemberFromValue(b => b.Excluded, ExcludeOption);
            binder.BindMemberFromValue(b => b.ControllerTypes, TypeOption);

            return binder;
        }

        public void BindCommandOptions(IServiceCollection services)
        {
            services.AddSingleton(GetOptionsBinder());
            services.AddOptions<EventLogSignalInfoConfiguration>().BindCommandLine();
            services.AddHostedService<SignaInfoHostedService>();
        }
    }

    public class SignaInfoHostedService : IHostedService
    {
        private readonly ILogger _log;
        private IServiceProvider _serviceProvider;
        private IOptions<EventLogSignalInfoConfiguration> _options;

        public SignaInfoHostedService(ILogger<SignaInfoHostedService> log, IServiceProvider serviceProvider, IOptions<EventLogSignalInfoConfiguration> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                using (var scope = _serviceProvider.CreateAsyncScope())
                {
                    var signalRepository = scope.ServiceProvider.GetService<ISignalRepository>();
                    var controllerLoggingService = scope.ServiceProvider.GetService<ISignalControllerLoggerService>();

                    var signalQuery = signalRepository.GetLatestVersionOfAllSignals().Where(s => s.ChartEnabled);

                    if (_options.Value.ControllerTypes != null)
                    {
                        foreach (var s in _options.Value.ControllerTypes)
                        {
                            _log.LogInformation("Including Event Logs for Types(s): {type}", s);
                        }

                        signalQuery = signalQuery.Where(i => _options.Value.ControllerTypes.Any(d => i.ControllerTypeId == d));
                    }

                    if (_options.Value.Included != null)
                    {
                        foreach (var s in _options.Value.Included)
                        {
                            _log.LogInformation("Including Event Logs for Signal(s): {signal}", s);
                        }

                        signalQuery = signalQuery.Where(i => _options.Value.Included.Any(d => i.LocationIdentifier == d));
                    }

                    if (_options.Value.Excluded != null)
                    {
                        foreach (var s in _options.Value.Excluded)
                        {
                            _log.LogInformation("Excluding Event Logs for Signal(s): {signal}", s);
                        }

                        signalQuery = signalQuery.Where(i => !_options.Value.Excluded.Contains(i.LocationIdentifier));
                    }

                    var signals = signalQuery.ToList();

                    foreach (var s in signals)
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

        private void PrintInfo(Location signal)
        {
            Console.WriteLine();
            Console.WriteLine($"-------------------------------------------------------------");

            Console.WriteLine($"key: {signal.Id}");
            Console.WriteLine($"id: {signal.LocationIdentifier}");
            Console.WriteLine($"primary name: {signal.PrimaryName}");
            Console.WriteLine($"secondary name: {signal.SecondaryName}");
            Console.WriteLine($"ip address: {signal.Ipaddress}");
            Console.WriteLine($"controller type: {signal.ControllerType}");
            Console.WriteLine($"enabled: {signal.ChartEnabled}");
            Console.WriteLine($"jurisdiction: {signal.Jurisdiction}");
            Console.WriteLine($"region: {signal.Region}");
            Console.WriteLine($"long/lat: {signal.Longitude}/{signal.Latitude}");

            Console.WriteLine($"-------------------------------------------------------------");
        }
    }
}
