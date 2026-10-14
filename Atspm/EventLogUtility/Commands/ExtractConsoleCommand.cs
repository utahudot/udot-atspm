#region license
// Copyright 2026 Utah Departement of Transportation
// for EventLogUtility - Utah.Udot.Atspm.EventLogUtility.Commands/ExtractConsoleCommand.cs
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
using Utah.Udot.Atspm.Infrastructure.Services.HostedServices;

namespace Utah.Udot.Atspm.EventLogUtility.Commands
{
    /// <summary>
    /// Console command for extracting controller event logs to disk.
    /// </summary>
    public class ExtractConsoleCommand : Command, ICommandOption<EventLogExtractConfiguration>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractConsoleCommand"/> class.
        /// </summary>
        public ExtractConsoleCommand() : base("extract", "Extract compressed controller event logs")
        {
            FileCommandOption.FromAmong("csv", "json");

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
            AddOption(ParallelProcessesOption);
        }

        /// <summary>
        /// Gets or sets the file format option.
        /// </summary>
        public Option<string> FileCommandOption { get; set; } = new("--filetype", () => "csv", "File type format to export to");

        /// <summary>
        /// Gets or sets the date and time format string option.
        /// </summary>
        public Option<string> DateTimeFormatOption { get; set; } = new("--datetimeformat", () => "yyyy-MM-dd'T'HH:mm:ss.f", "Date/Time format string to use");

        /// <summary>
        /// Gets or sets the extraction dates option.
        /// </summary>
        public DateCommandOption DateOption { get; set; } = new();

        /// <summary>
        /// Gets or sets the location filter inclusion option.
        /// </summary>
        public LocationIncludeCommandOption IncludeOption { get; set; } = new();

        /// <summary>
        /// Gets or sets the location filter exclusion option.
        /// </summary>
        public LocationExcludeCommandOption ExcludeOption { get; set; } = new();

        /// <summary>
        /// Gets or sets the output directory path option.
        /// </summary>
        public PathCommandOption PathCommandOption { get; set; } = new();

        /// <summary>
        /// Gets or sets the maximum degree of parallelism option.
        /// </summary>
        public PrallelProcessesOption ParallelProcessesOption { get; set; } = new();

        /// <inheritdoc/>
        public ModelBinder<EventLogExtractConfiguration> GetOptionsBinder()
        {
            var binder = new ModelBinder<EventLogExtractConfiguration>();

            binder.BindMemberFromValue(b => b.FileFormat, FileCommandOption);
            binder.BindMemberFromValue(b => b.DateTimeFormat, DateTimeFormatOption);
            binder.BindMemberFromValue(b => b.Dates, DateOption);
            binder.BindMemberFromValue(b => b.Included, IncludeOption);
            binder.BindMemberFromValue(b => b.Excluded, ExcludeOption);
            binder.BindMemberFromValue(b => b.Path, PathCommandOption);
            binder.BindMemberFromValue(b => b.ParallelProcesses, ParallelProcessesOption);

            return binder;
        }

        /// <inheritdoc/>
        public void BindCommandOptions(HostBuilderContext host, IServiceCollection services)
        {
            services.AddSingleton(GetOptionsBinder());
            services.AddOptions<EventLogExtractConfiguration>().BindCommandLine();
            services.AddHostedService<ExtractEventLogHostedService>();
        }
    }
}
