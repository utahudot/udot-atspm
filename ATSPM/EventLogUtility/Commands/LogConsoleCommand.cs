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
using ATSPM.EventLogUtility.CommandBinders;
using System.CommandLine.NamingConventionBinder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ATSPM.EventLogUtility.Commands
{
    public class LogConsoleCommand : Command
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

            AddOption(DateOption);
            AddOption(IncludeOption);
            AddOption(ExcludeOption);
            AddOption(PathCommandOption);

            this.SetHandler((d, i, e, p) =>
            {
                Console.WriteLine($"{this.Name} is executing");

                foreach (var s in d)
                {
                    Console.WriteLine($"Extracting event logs for {s:dd/MM/yyyy}");
                }

                foreach (var s in i)
                {
                    Console.WriteLine($"Extracting event logs for signal {s}");
                }

                foreach (var s in e)
                {
                    Console.WriteLine($"Excluding event logs for signal {s}");
                }

                Console.WriteLine($"Extraction path {p}");

            }, DateOption, IncludeOption, ExcludeOption, PathCommandOption);
        }

        public DateCommandOption DateOption { get; set; } = new();

        public SignalIncludeCommandOption IncludeOption { get; set; } = new();

        public SignalExcludeCommandOption ExcludeOption { get; set; } = new();

        public PathCommandOption PathCommandOption { get; set; } = new();

        //TODO: Make an interface and have a method that passes in IServiceCollection so this is reusable by other commands
        public EventLogLoggingConfiguration ParseOptions(EventLogLoggingConfiguration config, InvocationContext invocation)
        {
            if (invocation.ParseResult.CommandResult.Command == this)
            {
                if (invocation.ParseResult.HasOption(DateOption))
                    config.Dates = invocation.ParseResult.GetValueForOption(DateOption) ?? new List<DateTime>();

                if (invocation.ParseResult.HasOption(IncludeOption))
                    config.Included = invocation.ParseResult.GetValueForOption(IncludeOption) ?? new List<string>();

                if (invocation.ParseResult.HasOption(ExcludeOption))
                    config.Excluded = invocation.ParseResult.GetValueForOption(ExcludeOption) ?? new List<string>();

                if (invocation.ParseResult.HasOption(PathCommandOption))
                    config.Path = invocation.ParseResult.GetValueForOption(PathCommandOption) ?? new DirectoryInfo(string.Empty);
            }

            return config;
        }

    }
}
