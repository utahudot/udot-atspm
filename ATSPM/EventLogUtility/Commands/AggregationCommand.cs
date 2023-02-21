using ATSPM.Application.Configuration;
using ATSPM.Infrastructure.Services.HostedServices;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.EventLogUtility.Commands
{
    public class AggregationCommand : Command, ICommandOption<EventLogAggregateConfiguration>
    {
        public AggregationCommand() : base("aggregate", "Run data aggregation")
        {
            AggregationType.FromAmong(
                "approach-cycle",
                "approach-pcd-cycle",
                "approach-speed",
                "approach-splitfail",
                "detector-event-count",
                "left-turn-gap",
                "signal-event-data",
                "signal-phase-delay",
                "signal-phase-termination",
                "signal-plan",
                "signal-preempt-priority",
                "split-monitor",
                "yellow-red-activation");

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

            AddArgument(AggregationType);
            AddArgument(BinSize);
            AddOption(DateOption);
            AddOption(IncludeOption);
            AddOption(ExcludeOption);
        }

        public Argument<string> AggregationType { get; set; } = new Argument<string>("type", "Aggregation type to run");

        //TODO: add a parse param to handle zero time to too large or a time
        public Argument<int> BinSize { get; set; } = new Argument<int>("Size", () => 15, "Size in minutes to aggregate");

        public DateCommandOption DateOption { get; set; } = new();

        public SignalIncludeCommandOption IncludeOption { get; set; } = new();

        public SignalExcludeCommandOption ExcludeOption { get; set; } = new();

        public ModelBinder<EventLogAggregateConfiguration> GetOptionsBinder()
        {
            var binder = new ModelBinder<EventLogAggregateConfiguration>();

            binder.BindMemberFromValue(b => b.AggregationType, AggregationType);
            binder.BindMemberFromValue(b => b.BinSize, BinSize);
            binder.BindMemberFromValue(b => b.Dates, DateOption);
            binder.BindMemberFromValue(b => b.Included, IncludeOption);
            binder.BindMemberFromValue(b => b.Excluded, ExcludeOption);

            return binder;
        }

        public void BindCommandOptions(IServiceCollection services)
        {
            services.AddSingleton(GetOptionsBinder());
            services.AddOptions<EventLogAggregateConfiguration>().BindCommandLine();
            services.AddHostedService<TestAggregationHostedService>();
        }
    }
}
