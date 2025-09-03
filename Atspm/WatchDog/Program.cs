#region license
// Copyright 2025 Utah Departement of Transportation
// for WatchDog - %Namespace%/Program.cs
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

using Google.Api;
using Google.Cloud.Diagnostics.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.Diagnostics;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Infrastructure.Extensions;
using Utah.Udot.Atspm.Services;
using Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices;
using Utah.Udot.ATSPM.WatchDog.Commands;

if (OperatingSystem.IsWindows())
{
    if (!EventLog.SourceExists("Atspm"))
        EventLog.CreateEventSource(AppDomain.CurrentDomain.FriendlyName, "Atspm");
}

var rootCmd = new WatchdogCommand();
var cmdBuilder = new CommandLineBuilder(rootCmd);
cmdBuilder.UseDefaults();

cmdBuilder.UseHost(hostBuilder =>
{
    return Host.CreateDefaultBuilder(args)
    //.UseConsoleLifetime()
    .ApplyVolumeConfiguration()
    .ConfigureLogging((h, l) =>
    {
        if (OperatingSystem.IsWindows())
        {
            l.AddEventLog(c =>
            {
                c.SourceName = AppDomain.CurrentDomain.FriendlyName;
                c.LogName = "Atspm";
            });
        }

        
        l.AddGoogleLogging(h);
    })
    .ConfigureServices((h, s) =>
    {
        s.AddEmailServices(h);
        s.AddAtspmDbContext(h);
        s.AddAtspmEFConfigRepositories();
        s.AddAtspmEFEventLogRepositories();
        s.AddAtspmEFAggregationRepositories();
        s.AddScoped<IWatchdogEmailService, WatchdogEmailService>();
        s.AddScoped<WatchDogLogService>();
        s.AddScoped<ScanService>();
        s.AddScoped<PlanService>();
        s.AddScoped<AnalysisPhaseCollectionService>();
        s.AddScoped<AnalysisPhaseService>();
        s.AddScoped<PhaseService>();
        s.AddScoped<SegmentedErrorsService>();
        s.AddScoped<IWatchDogIgnoreEventService, WatchDogIgnoreEventService>();


        // Register the hosted service with the date
        s.AddIdentity<ApplicationUser, IdentityRole>() // Add this line to register Identity
            .AddEntityFrameworkStores<IdentityContext>() // Specify the EF Core store
            .AddDefaultTokenProviders();
    });
},
host =>
{
    var cmd = host.GetInvocationContext().ParseResult.CommandResult.Command;

    host.ConfigureServices((context, services) =>
    {
        if (cmd is ICommandOption commandOption)
        {
            commandOption.BindCommandOptions(context, services);
        }
    });
});


var cmdParser = cmdBuilder.Build();
await cmdParser.InvokeAsync(args);


/// <summary>
/// Provides extension methods for <see cref="IHostEnvironment"/> to detect and extract environment context
/// </summary>
public static class HostEnvironmentExtensions
{
    /// <summary>
    /// Determines if the application is running inside a container by checking the <c>DOTNET_RUNNING_IN_CONTAINER</c> environment variable.
    /// </summary>
    /// <param name="env">The host environment.</param>
    /// <returns>True if running in a container; otherwise, false.</returns>
    public static bool IsContainer(this IHostEnvironment env)
    {
        return string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines if the application is running as a Google Cloud Run service by checking the <c>K_SERVICE</c> environment variable.
    /// </summary>
    /// <param name="env">The host environment.</param>
    /// <returns>True if running as a Cloud Run service; otherwise, false.</returns>
    public static bool IsCloudRunService(this IHostEnvironment env)
    {
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("K_SERVICE"));
    }

    /// <summary>
    /// Determines if the application is running as a Google Cloud Run job by checking the <c>CLOUD_RUN_JOB</c> environment variable.
    /// </summary>
    /// <param name="env">The host environment.</param>
    /// <returns>True if running as a Cloud Run job; otherwise, false.</returns>
    public static bool IsCloudRunJob(this IHostEnvironment env)
    {
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CLOUD_RUN_JOB"));
    }

    /// <summary>
    /// Extracts Cloud Run service metadata from environment variables (<c>K_SERVICE</c>, <c>K_REVISION</c>, <c>K_CONFIGURATION</c>)
    /// and returns a <see cref="CloudRunConfiguration"/> instance.
    /// </summary>
    /// <param name="env">The host environment.</param>
    /// <returns>A <see cref="CloudRunConfiguration"/> populated from environment variables.</returns>
    public static CloudRunConfiguration GetCloudRunConfiguration(this IHostEnvironment env)
    {
        return new CloudRunConfiguration()
        {
            Service = Environment.GetEnvironmentVariable("K_SERVICE"),
            Revision = Environment.GetEnvironmentVariable("K_REVISION"),
            Configuration = Environment.GetEnvironmentVariable("K_CONFIGURATION"),
        };
    }

    /// <summary>
    /// Extracts Cloud Run job metadata from environment variables (<c>CLOUD_RUN_JOB</c>, <c>CLOUD_RUN_JOB_EXECUTION</c>,
    /// <c>CLOUD_RUN_TASK_INDEX</c>, <c>CLOUD_RUN_TASK_ATTEMPT</c>) and returns a <see cref="CloudRunJobConfiguration"/> instance.
    /// </summary>
    /// <param name="env">The host environment.</param>
    /// <returns>A <see cref="CloudRunJobConfiguration"/> populated from environment variables.</returns>
    public static CloudRunJobConfiguration GetCloudRunJobConfiguration(this IHostEnvironment env)
    {
        return new CloudRunJobConfiguration()
        {
            JobName = Environment.GetEnvironmentVariable("CLOUD_RUN_JOB"),
            ExecutionId = Environment.GetEnvironmentVariable("CLOUD_RUN_EXECUTION"),
            TaskIndex = Environment.GetEnvironmentVariable("CLOUD_RUN_TASK_INDEX"),
            TaskAttempt = Environment.GetEnvironmentVariable("CLOUD_RUN_TASK_ATTEMPT"),
        };
    }
}

public static class LoggingBuilerExtensions
{
    public static ILoggingBuilder AddGoogleLogging(this ILoggingBuilder builder, HostBuilderContext host)
    {
        var config = host.Configuration.GetSection("Logging:GoogleDiagnostics").Get<GoogleDiagnosticsConfiguration>();

        if (config != null && config.Enabled)
        {
            var resource = new MonitoredResource();
            var labels = new Dictionary<string, string>();

            if (host.HostingEnvironment.IsCloudRunService())
            {
                var service = host.HostingEnvironment.GetCloudRunConfiguration();
            }

            if (host.HostingEnvironment.IsCloudRunJob())
            {
                var job = host.HostingEnvironment.GetCloudRunJobConfiguration();

                builder.ClearProviders();

                resource.Type = "cloud_run_job";
                resource.Labels.Add("project_id", config.ProjectId);
                resource.Labels.Add("job_name", job?.JobName);
                resource.Labels.Add("location", config.Region);

                labels.Add("run.googleapis.com/execution_name", job?.ExecutionId ?? "manual");
                labels.Add("run.googleapis.com/task_attempt", job?.TaskAttempt ?? "0");
                labels.Add("run.googleapis.com/task_index", job?.TaskIndex ?? "0");
            }

            builder.AddGoogle(new LoggingServiceOptions
            {
                ProjectId = config.ProjectId,
                ServiceName = config.ServiceName,
                Version = config.Version,
                Options = LoggingOptions.Create(config.MinimumLogLevel, config.LogName, labels, resource)
            });
        }

        return builder;
    }
}