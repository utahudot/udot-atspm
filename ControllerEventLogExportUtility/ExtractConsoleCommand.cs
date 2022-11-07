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

namespace ControllerEventLogExportUtility
{
    public class ExtractConsoleCommand : Command
    {
        public ExtractConsoleCommand() : base("extract", "Extract compressed controller event logs")
        {
            ExtractionDateOption.AddAlias("-d");
            ExtractionDateOption.AddValidator(r =>
            {
                if (r.GetValueForOption(ExtractionDateOption).Any(a => a > DateTime.Now))
                    r.ErrorMessage = "Date must not be greater than current date";
            });

            ExtractionIncludeOption.AddAlias("-i");
            ExtractionExcludeOption.AddAlias("-e");

            ExtractionIncludeOption.AddValidator(r =>
            {
                if (r.GetValueForOption(ExtractionExcludeOption)?.Count() > 0)
                    r.ErrorMessage = "Can't use include option when also using exclude option";
            });
            ExtractionExcludeOption.AddValidator(r =>
            {
                if (r.GetValueForOption(ExtractionIncludeOption)?.Count() > 0)
                    r.ErrorMessage = "Can't use exclude option when also using include option";
            });

            ExtractionTypeOption.AddAlias("-t");

            ExtractionPathOption.AddAlias("-p");

            this.AddOption(ExtractionDateOption);
            this.AddOption(ExtractionIncludeOption);
            this.AddOption(ExtractionExcludeOption);
            this.AddOption(ExtractionTypeOption);
            this.AddOption(ExtractionPathOption);

            //this.SetHandler((d, i, e, t, p) =>
            //{
            //    foreach (var s in d)
            //    {
            //        Console.WriteLine($"Extracting event logs for {s:dd/MM/yyyy}");
            //    }

            //    foreach (var s in i)
            //    {
            //        Console.WriteLine($"Extracting event logs for signal {s}");
            //    }

            //    foreach (var s in e)
            //    {
            //        Console.WriteLine($"Excluding event logs for signal {s}");
            //    }

            //    foreach (var s in t)
            //    {
            //        Console.WriteLine($"Extracting event logs for signal type {s}");
            //    }

            //    Console.WriteLine($"Extraction path {p}");

            //}, ExtractionDateOption, ExtractionIncludeOption, ExtractionExcludeOption, ExtractionTypeOption, ExtractionPathOption);
        }

        public Option<IEnumerable<DateTime>> ExtractionDateOption = new("--date", "Date to extract event logs for in dd/mm/yyyy format")
        {
            IsRequired = true,
            AllowMultipleArgumentsPerToken = true
        };

        public Option<IEnumerable<string>> ExtractionIncludeOption = new("--include", "List of signal controller numbers to include")
        {
            AllowMultipleArgumentsPerToken = true
        };


        public Option<IEnumerable<string>> ExtractionExcludeOption = new("--exclude", "List of signal controller numbers to exclude")
        {
            AllowMultipleArgumentsPerToken = true
        };


        public Option<IEnumerable<int>> ExtractionTypeOption = new("--type", "List of controller types to extract")
        {
            AllowMultipleArgumentsPerToken = true
        };

        public Option<DirectoryInfo> ExtractionPathOption = new("--path", () => new DirectoryInfo(Path.Combine("C:", "temp", "exports")), "Path to extraction directory");

        public ExtractConsoleConfiguration ParseOptions(ExtractConsoleConfiguration config, InvocationContext invocation)
        {
            //var config = new ExtractConsoleConfiguration();

            Console.WriteLine($"config2: {config.GetHashCode()}");

            if (invocation.ParseResult.CommandResult.Command == this)
            {
                if (invocation.ParseResult.HasOption(ExtractionDateOption))
                    config.Dates = invocation.ParseResult.GetValueForOption(ExtractionDateOption) ?? new List<DateTime>();

                if (invocation.ParseResult.HasOption(ExtractionIncludeOption))
                    config.Included = invocation.ParseResult.GetValueForOption(ExtractionIncludeOption) ?? new List<string>();

                if (invocation.ParseResult.HasOption(ExtractionExcludeOption))
                    config.Excluded = invocation.ParseResult.GetValueForOption(ExtractionExcludeOption) ?? new List<string>();

                if (invocation.ParseResult.HasOption(ExtractionTypeOption))
                    config.ControllerTypes = invocation.ParseResult.GetValueForOption(ExtractionTypeOption) ?? new List<int>();

                if (invocation.ParseResult.HasOption(ExtractionPathOption))
                    config.Path = invocation.ParseResult.GetValueForOption(ExtractionPathOption) ?? new DirectoryInfo(string.Empty);
            }

            return config;
        }

    }
}
