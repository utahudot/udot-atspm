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
using ATSPM.Infrastructure.Services.HostedServices;

namespace ATSPM.EventLogUtility.Commands
{
    public class LogConsoleCommand : Command, ICommandOption<EventLogLoggingConfiguration>
    {
        public LogConsoleCommand() : base("log", "Logs data from signal controllers")
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

            AddArgument(PingControllerArg);
            AddArgument(DeleteLocalFileArg);

            AddOption(IncludeOption);
            AddOption(ExcludeOption);
            AddOption(TypeOption);
            AddOption(PathCommandOption);

            //this.SetHandler((d, i, e, p) =>
            //{
            //    Console.WriteLine($"{this.Name} is executing");

            //    foreach (var s in i)
            //    {
            //        Console.WriteLine($"Extracting event logs for signal {s}");
            //    }

            //    foreach (var s in e)
            //    {
            //        Console.WriteLine($"Excluding event logs for signal {s}");
            //    }

            //    Console.WriteLine($"Extraction path {p}");

            //}, DateOption, IncludeOption, ExcludeOption, PathCommandOption);
        }

        public Argument<bool> PingControllerArg { get; set; } = new Argument<bool>("ping", "Ping to verify signal controller is online");

        public Argument<bool> DeleteLocalFileArg { get; set; } = new Argument<bool>("delete local", "Delete local file");

        public SignalIncludeCommandOption IncludeOption { get; set; } = new();

        public SignalExcludeCommandOption ExcludeOption { get; set; } = new();

        public SignalTypeCommandOption TypeOption { get; set; } = new();

        public PathCommandOption PathCommandOption { get; set; } = new();

        public ModelBinder<EventLogLoggingConfiguration> GetOptionsBinder()
        {
            var binder = new ModelBinder<EventLogLoggingConfiguration>();

            binder.BindMemberFromValue(b => b.Included, IncludeOption);
            binder.BindMemberFromValue(b => b.Excluded, ExcludeOption);
            binder.BindMemberFromValue(b => b.ControllerTypes, TypeOption);
            binder.BindMemberFromValue(b => b.Path, PathCommandOption);

            return binder;
        }

        public void BindCommandOptions(IServiceCollection services)
        {
            services.AddSingleton(GetOptionsBinder());
            services.AddOptions<EventLogLoggingConfiguration>().BindCommandLine();
            services.AddHostedService<SignalLoggerUtilityHostedService>();
        }
    }
}
