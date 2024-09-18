#region license
// Copyright 2024 Utah Departement of Transportation
// for EventLogUtility - ATSPM.EventLogUtility.Commands/LogConsoleCommand.cs
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
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.NamingConventionBinder;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Infrastructure.Services.HostedServices;

namespace Utah.Udot.Atspm.EventLogUtility.Commands
{
    public class LogConsoleCommand : Command, ICommandOption
    {
        public LogConsoleCommand() : base("log", "Logs data from Location controllers")
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

            AddArgument(PingDeviceArg);
            AddArgument(DeleteLocalFileArg);

            AddOption(PathCommandOption);
            AddOption(BatchSizeOption);
            AddOption(PrallelProcessesOption);

            AddOption(IncludeOption);
            AddOption(ExcludeOption);
            AddOption(AreaOption);
            AddOption(JurisdictionOption);
            AddOption(RegionOption);
            AddOption(LocationTypeOption);
            AddOption(DeviceTypeOption);
            AddOption(TransportProtocolOption);
            AddOption(DeviceStatusCommandOption);
        }

        public Argument<bool> PingDeviceArg { get; set; } = new Argument<bool>("ping", () => true, "Ping to verify device is online");

        public Argument<bool> DeleteLocalFileArg { get; set; } = new Argument<bool>("delete local", () => false, "Delete local file");

        public PathCommandOption PathCommandOption { get; set; } = new();

        public BatchSizeOption BatchSizeOption { get; set; } = new();

        public PrallelProcessesOption PrallelProcessesOption { get; set; } = new();

        public LocationIncludeCommandOption IncludeOption { get; set; } = new();

        public LocationExcludeCommandOption ExcludeOption { get; set; } = new();

        public LocationAreaCommandOption AreaOption { get; set; } = new();

        public LocationJurisdictionCommandOption JurisdictionOption { get; set; } = new();

        public LocationRegionCommandOption RegionOption { get; set; } = new();

        public LocationTypeCommandOption LocationTypeOption { get; set; } = new();

        public DeviceTypeCommandOption DeviceTypeOption { get; set; } = new();

        public TransportProtocolCommandOption TransportProtocolOption { get; set; } = new();

        public DeviceStatusCommandOption DeviceStatusCommandOption { get; set; } = new();   

        public void BindCommandOptions(HostBuilderContext host, IServiceCollection services)
        {
            services.Configure<DeviceEventLoggingConfiguration>(host.Configuration.GetSection(nameof(DeviceEventLoggingConfiguration)));

            var parentBinder = new ModelBinder<DeviceEventLoggingConfiguration>();

            parentBinder.BindMemberFromValue(b => b.Path, PathCommandOption);
            parentBinder.BindMemberFromValue(b => b.BatchSize, PathCommandOption);
            parentBinder.BindMemberFromValue(b => b.ParallelProcesses, PrallelProcessesOption);

            var childBinder = new ModelBinder<DeviceEventLoggingQueryOptions>();

            childBinder.BindMemberFromValue(b => b.IncludedLocations, IncludeOption);
            childBinder.BindMemberFromValue(b => b.ExcludedLocations, ExcludeOption);
            childBinder.BindMemberFromValue(b => b.IncludedAreas, AreaOption);
            childBinder.BindMemberFromValue(b => b.IncludedJurisdictions, JurisdictionOption);
            childBinder.BindMemberFromValue(b => b.IncludedRegions, RegionOption);
            childBinder.BindMemberFromValue(b => b.IncludedLocationTypes, LocationTypeOption);
            childBinder.BindMemberFromValue(b => b.DeviceType, DeviceTypeOption);
            childBinder.BindMemberFromValue(b => b.TransportProtocol, TransportProtocolOption);
            childBinder.BindMemberFromValue(b => b.TransportProtocol, TransportProtocolOption);

            services.AddOptions<DeviceEventLoggingConfiguration>()
                .Configure<BindingContext>((a, b) =>
                {
                    parentBinder.UpdateInstance(a, b);
                    childBinder.UpdateInstance(a.DeviceEventLoggingQueryOptions, b);
                });

            services.AddHostedService<DeviceEventLogHostedService>();
        }
    }

    public class BatchSizeOption : Option<int>
    {
        public BatchSizeOption() : base("--batch-size", "Batch size of event logs to save to repository")
        {
            AddAlias("-bs");
            SetDefaultValue(50000);
        }
    }

    public class PrallelProcessesOption : Option<int>
    {
        public PrallelProcessesOption() : base("--parallel-processes", "Amount of processes that can be run in parallel")
        {
            AddAlias("-pp");
            SetDefaultValue(50);
        }
    }

    public class DeviceTypeCommandOption : Option<DeviceTypes>
    {
        public DeviceTypeCommandOption() : base("--device-type", "Device type to include") 
        {
            AddAlias("-dt");
        }
    }

    public class TransportProtocolCommandOption : Option<TransportProtocols>
    {
        public TransportProtocolCommandOption() : base("--transport-protocol", "Device transport protocol to include")
        {
            AddAlias("-tp");
        }
    }

    public class DeviceStatusCommandOption : Option<DeviceStatus>
    {
        public DeviceStatusCommandOption() : base("--device-status", "Device status to include")
        {
            AddAlias("-ds");
        }
    }
}
