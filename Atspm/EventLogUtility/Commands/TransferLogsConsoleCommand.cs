using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.NamingConventionBinder;
using System.Text.RegularExpressions;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.EventLogUtility.Commands;
using Utah.Udot.Atspm.Infrastructure.Services.HostedServices;

namespace Utah.Udot.ATSPM.EventLogUtility.Commands
{
    public  class TransferLogsConsoleCommand : Command, ICommandOption
    {
        public TransferLogsConsoleCommand() : base("transfer", "Transfers event logs between repositories")
        {
            var values = typeof(EventLogModelBase).Assembly.GetTypes()
                .Where(w => w.IsSubclassOf(typeof(EventLogModelBase)))
                .Select(s => Regex.Replace(s.Name, @"(?<=[a-z])([A-Z])", @"_$1").ToLower())
                .Prepend("all")
                .ToArray();

            DataTypeArgument.FromAmong(values);

            Start.AddValidator(r =>
            {
                if (r.GetValueForOption(End) < r.GetValueForOption(Start))
                    r.ErrorMessage = "Start date must be before end date";
            });
            End.AddValidator(r =>
            {
                if (r.GetValueForOption(Start) > r.GetValueForOption(End))
                    r.ErrorMessage = "End date must be after start date";
            });

            IncludeLocationOption.AddValidator(r =>
            {
                if (r.GetValueForOption(ExcludeLocationOption)?.Count() > 0)
                    r.ErrorMessage = "Can't use include option when also using exclude option";
            });
            ExcludeLocationOption.AddValidator(r =>
            {
                if (r.GetValueForOption(IncludeLocationOption)?.Count() > 0)
                    r.ErrorMessage = "Can't use exclude option when also using include option";
            });

            AddArgument(DataTypeArgument);
            AddGlobalOption(IncludeLocationOption);
            AddGlobalOption(ExcludeLocationOption);
            AddGlobalOption(Start);
            AddGlobalOption(End);
            AddGlobalOption(DeviceIdIncludeCommandOption);
        }

        public Argument<string> DataTypeArgument { get; set; } = new Argument<string>("type", () => "all", "Event log data type to transfer");

        public StartOption Start { get; set; } = new StartOption();

        public EndOption End { get; set; } = new EndOption();   

        public LocationIncludeCommandOption IncludeLocationOption { get; set; } = new();

        public LocationExcludeCommandOption ExcludeLocationOption { get; set; } = new();

        public DeviceIncludeCommandOption DeviceIdIncludeCommandOption { get; set; } = new();

        public void BindCommandOptions(HostBuilderContext host, IServiceCollection services)
        {
            services.Configure<EventLogTransferOptions>(host.Configuration.GetSection(nameof(EventLogTransferOptions)));

            var binder = new ModelBinder<EventLogTransferOptions>();

            //binder.BindMemberFromValue(b => b.SourceRepository, Start);
            //binder.BindMemberFromValue(b => b.DestinationRepository, Start);

            binder.BindMemberFromValue(b => b.StartDate, Start);
            binder.BindMemberFromValue(b => b.EndDate, End);
            binder.BindMemberFromValue(b => b.IncludedLocations, IncludeLocationOption);
            binder.BindMemberFromValue(b => b.ExcludedLocations, ExcludeLocationOption);
            binder.BindMemberFromValue(b => b.IncludedDeviceIds, DeviceIdIncludeCommandOption);
            binder.BindMemberFromValue(b => b.DataType, DataTypeArgument);

            services.AddOptions<EventLogTransferOptions>()
            .Configure<BindingContext>((a, b) =>
            {
                    binder.UpdateInstance(a, b);
                });

            services.AddHostedService<EventLogTransferHostedService>();
        }

        public class DeviceIncludeCommandOption : Option<IEnumerable<int>>
        {
            public DeviceIncludeCommandOption() : base("--include-device", "List of device id's to include")
            {
                AllowMultipleArgumentsPerToken = true;
                AddAlias("-id");
            }
        }
    }
}
