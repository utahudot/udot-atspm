using Microsoft.Extensions.Hosting;

namespace Utah.Udot.Atspm.Infrastructure.Extensions
{
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
}
