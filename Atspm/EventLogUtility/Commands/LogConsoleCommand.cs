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
using System.CommandLine.Hosting;
using System.CommandLine.NamingConventionBinder;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Infrastructure.Services.HostedServices;

namespace Utah.Udot.Atspm.EventLogUtility.Commands
{
    public class LogConsoleCommand : Command, ICommandOption<DeviceEventLoggingConfiguration>
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

            AddOption(IncludeOption);
            AddOption(ExcludeOption);
            AddOption(AreaOption);
            AddOption(JurisdictionOption);
            AddOption(RegionOption);
            AddOption(LocationTypeOption);
            AddOption(DeviceTypeOption);
            AddOption(TransportProtocolOption);
            AddOption(PathCommandOption);
        }

        public Argument<bool> PingDeviceArg { get; set; } = new Argument<bool>("ping", "Ping to verify device is online");

        public Argument<bool> DeleteLocalFileArg { get; set; } = new Argument<bool>("delete local", "Delete local file");

        public LocationIncludeCommandOption IncludeOption { get; set; } = new();

        public LocationExcludeCommandOption ExcludeOption { get; set; } = new();

        public LocationAreaCommandOption AreaOption { get; set; } = new();

        public LocationJurisdictionCommandOption JurisdictionOption { get; set; } = new();

        public LocationRegionCommandOption RegionOption { get; set; } = new();

        public LocationTypeCommandOption LocationTypeOption { get; set; } = new();

        public DeviceTypeCommandOption DeviceTypeOption { get; set; } = new();

        public TransportProtocolCommandOption TransportProtocolOption { get; set; } = new();

        public PathCommandOption PathCommandOption { get; set; } = new();

        public ModelBinder<DeviceEventLoggingConfiguration> GetOptionsBinder()
        {
            var binder = new ModelBinder<DeviceEventLoggingConfiguration>();

            binder.BindMemberFromValue(b => b.IncludedLocations, IncludeOption);
            binder.BindMemberFromValue(b => b.ExcludedLocations, ExcludeOption);
            binder.BindMemberFromValue(b => b.IncludedAreas, AreaOption);
            binder.BindMemberFromValue(b => b.IncludedJurisdictions, JurisdictionOption);
            binder.BindMemberFromValue(b => b.IncludedRegions, RegionOption);
            binder.BindMemberFromValue(b => b.IncludedLocationTypes, LocationTypeOption);
            binder.BindMemberFromValue(b => b.DeviceType, DeviceTypeOption);
            binder.BindMemberFromValue(b => b.TransportProtocol, TransportProtocolOption);
            binder.BindMemberFromValue(b => b.Path, PathCommandOption);

            return binder;
        }

        public void BindCommandOptions(HostBuilderContext host, IServiceCollection services)
        {
            services.AddSingleton(GetOptionsBinder());
            services.Configure<DeviceEventLoggingConfiguration>(host.Configuration.GetSection(nameof(DeviceEventLoggingConfiguration)));
            services.AddOptions<DeviceEventLoggingConfiguration>().BindCommandLine();
            services.AddHostedService<LocationLoggerUtilityHostedService>();
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
}
