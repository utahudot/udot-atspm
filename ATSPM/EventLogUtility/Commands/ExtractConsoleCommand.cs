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
using ATSPM.Infrastructure.Services.HostedServices;

namespace ATSPM.EventLogUtility.Commands
{
    public class ExtractConsoleCommand : Command, ICommandOption<EventLogExtractConfiguration>
    {
        public ExtractConsoleCommand() : base("extract", "Extract compressed controller event logs")
        {
            FileCommandOption.FromAmong(".csv", ".json", ".parquet");

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

            AddOption(FileCommandOption);
            AddOption(DateTimeFormatOption);
            AddOption(DateOption);
            AddOption(IncludeOption);
            AddOption(ExcludeOption);
            AddOption(PathCommandOption);

            //this.SetHandler((f, d, i, e, p) =>
            //{
            //    Console.WriteLine($"{this.Name} is executing");

            //    Console.WriteLine($"file type {f}");

            //    foreach (var s in d)
            //    {
            //        Console.WriteLine($"Extracting event logs for {s:dd/MM/yyyy}");
            //    }

            //    foreach (var s in i)
            //    {
            //        Console.WriteLine($"Extracting event logs for Location {s}");
            //    }

            //    foreach (var s in e)
            //    {
            //        Console.WriteLine($"Excluding event logs for Location {s}");
            //    }

            //    Console.WriteLine($"Extraction path {p}");

            //}, FileCommandOption, DateOption, IncludeOption, ExcludeOption, PathCommandOption);
        }

        public Option<string> FileCommandOption { get; set; } = new("--filetype", () => ".csv", "File type format to export to");

        public Option<string> DateTimeFormatOption { get; set; } = new("--datetimeformat", () => "yyyy-MM-dd'T'HH:mm:ss.f", "Date/Time format string to use");

        public DateCommandOption DateOption { get; set; } = new();

        public LocationIncludeCommandOption IncludeOption { get; set; } = new();

        public LocationExcludeCommandOption ExcludeOption { get; set; } = new();

        public PathCommandOption PathCommandOption { get; set; } = new();

        public ModelBinder<EventLogExtractConfiguration> GetOptionsBinder()
        {
            var binder = new ModelBinder<EventLogExtractConfiguration>();

            binder.BindMemberFromValue(b => b.FileFormat, FileCommandOption);
            binder.BindMemberFromValue(b => b.DateTimeFormat, DateTimeFormatOption);
            binder.BindMemberFromValue(b => b.Dates, DateOption);
            binder.BindMemberFromValue(b => b.Included, IncludeOption);
            binder.BindMemberFromValue(b => b.Excluded, ExcludeOption);
            binder.BindMemberFromValue(b => b.Path, PathCommandOption);

            return binder;
        }

        public void BindCommandOptions(IServiceCollection services)
        {
            services.AddSingleton(GetOptionsBinder());
            services.AddOptions<EventLogExtractConfiguration>().BindCommandLine();
            services.AddHostedService<ExportUtilityService>();
        }
    }
}
