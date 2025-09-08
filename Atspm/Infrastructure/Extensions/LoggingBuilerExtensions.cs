using Google.Api;
using Google.Cloud.Diagnostics.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Utah.Udot.Atspm.Infrastructure.Extensions
{
    public static class LoggingBuilerExtensions
    {
        public static ILoggingBuilder AddGoogleLogging(this ILoggingBuilder builder, HostBuilderContext host)
        {
            var config = host.Configuration.GetSection("Logging:GoogleDiagnostics").Get<GoogleDiagnosticsConfiguration>();

            if (config != null && config.Enabled)
            {
                var resource = new MonitoredResource() { Type = "global"};
                var labels = new Dictionary<string, string>();

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

                if (host.HostingEnvironment.IsCloudRunService())
                {
                    var service = host.HostingEnvironment.GetCloudRunConfiguration();

                    builder.ClearProviders();

                    builder.AddGoogle(new LoggingServiceOptions
                    {
                        //ProjectId = config.ProjectId,
                        ServiceName = config.ServiceName,
                        Version = config.Version,
                        Options = LoggingOptions.Create(config.MinimumLogLevel, config.LogName, bufferOptions: BufferOptions.NoBuffer())
                    });
                }
                else
                {
                    builder.AddGoogle(new LoggingServiceOptions
                    {
                        ProjectId = config.ProjectId,
                        ServiceName = config.ServiceName,
                        Version = config.Version,
                        Options = LoggingOptions.Create(config.MinimumLogLevel, config.LogName, labels, resource)
                    });
                }

                
            }

            return builder;
        }
    }
}
