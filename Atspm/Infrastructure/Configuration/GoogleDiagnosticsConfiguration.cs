#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Configuration/GoogleDiagnosticsConfiguration.cs
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

using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    /// <summary>
    /// Represents configuration settings for enabling and controlling Google Cloud Diagnostics and Logging integration.
    /// This class is typically bound from the <c>Logging:GoogleDiagnostics</c> section in configuration files (e.g., <c>appsettings.json</c>),
    /// and its properties are used to configure the Google Cloud Logging provider via the <c>AddGoogleLogging</c> extension method.
    /// <para>
    /// The following properties map directly to options in the Google logging setup:
    /// <list type="bullet">
    ///   <item><description><see cref="Enabled"/>: Determines if Google Diagnostics logging is enabled.</description></item>
    ///   <item><description><see cref="ProjectId"/>: Sets the Google Cloud project ID for log entries (<c>ProjectId</c> in <c>LoggingServiceOptions</c>).</description></item>
    ///   <item><description><see cref="Region"/>: Optionally specifies the Google Cloud region for the logs.</description></item>
    ///   <item><description><see cref="ServiceName"/>: Sets the service name for log entries (<c>ServiceName</c> in <c>LoggingServiceOptions</c>).</description></item>
    ///   <item><description><see cref="LogName"/>: Sets the log name for log entries (<c>logName</c> in <c>LoggingOptions</c>).</description></item>
    ///   <item><description><see cref="Version"/>: Sets the service version for log entries (<c>Version</c> in <c>LoggingServiceOptions</c>).</description></item>
    ///   <item><description><see cref="MinimumLogLevel"/>: Sets the minimum log level for Google logging (<c>logLevel</c> in <c>LoggingOptions</c>).</description></item>
    /// </list>
    /// </para>
    /// When <c>AddGoogleLogging</c> is called, these values are used to initialize and control the Google Cloud Logging provider,
    /// ensuring logs are attributed to the correct project, service, and version context in Google Cloud.
    /// </summary>
    public class GoogleDiagnosticsConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether Google Cloud Diagnostics and Logging is enabled.
        /// When true, logging will be sent to Google Cloud using the provided configuration.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the Google Cloud project ID to which logs will be sent.
        /// This value is used as the <c>ProjectId</c> in Google Cloud Logging configuration.
        /// </summary>
        public string ProjectId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Google Cloud region for the logs.
        /// This is optional and may be used to further specify the logging resource location.
        /// </summary>
        public string Region { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the service for log entries.
        /// Used as the <c>ServiceName</c> in Google Cloud Logging and defaults to the current application domain's friendly name.
        /// </summary>
        public string ServiceName { get; set; } = AppDomain.CurrentDomain.FriendlyName;

        /// <summary>
        /// Gets or sets the log name for log entries.
        /// Used as the <c>logName</c> in Google Cloud Logging and defaults to the current application domain's friendly name.
        /// </summary>
        public string LogName { get; set; } = AppDomain.CurrentDomain.FriendlyName;

        /// <summary>
        /// Gets or sets the version of the service for log entries.
        /// Used as the <c>Version</c> in Google Cloud Logging and defaults to the entry assembly's version or "1.0.0" if unavailable.
        /// </summary>
        public string Version { get; set; } = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "1.0.0";

        /// <summary>
        /// Gets or sets the minimum <see cref="LogLevel"/> for logging to Google Cloud.
        /// Controls the lowest level of log messages that will be sent.
        /// </summary>
        public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;
    }

    /// <summary>
    /// Represents configuration values for a Google Cloud Run service instance.
    /// These properties correspond to environment variables automatically set by the Google Cloud Run service:
    /// <list type="bullet">
    ///   <item>
    ///     <description><c>K_SERVICE</c>: The name of the Cloud Run service.</description>
    ///   </item>
    ///   <item>
    ///     <description><c>K_REVISION</c>: Name of the specific revision being executed.</description>
    ///   </item>
    ///   <item>
    ///     <description><c>K_CONFIGURATION</c>: Name of the configuration that triggered this revision.</description>
    ///   </item>
    /// </list>
    /// </summary>
    public class CloudRunConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the Cloud Run service.
        /// Maps to the <c>K_SERVICE</c> environment variable.
        /// </summary>
        public string Service { get; set; }

        /// <summary>
        /// Gets or sets the name of the Cloud Run service revision.
        /// Maps to the <c>K_REVISION</c> environment variable.
        /// </summary>
        public string Revision { get; set; }

        /// <summary>
        /// Gets or sets the name of the Cloud Run configuration.
        /// Maps to the <c>K_CONFIGURATION</c> environment variable.
        /// </summary>
        public string Configuration { get; set; }
    }

    /// <summary>
    /// Represents configuration values for a Google Cloud Run Job execution.
    /// Each property corresponds to an environment variable automatically set by the Google Cloud Run Jobs service:
    /// <list type="bullet">
    ///   <item>
    ///     <description><c>CLOUD_RUN_JOB</c>: The name of the Cloud Run Job, mapped to <see cref="JobName"/>.</description>
    ///   </item>
    ///   <item>
    ///     <description><c>CLOUD_RUN_JOB_EXECUTION</c>: The unique identifier for the current job execution, mapped to <see cref="ExecutionId"/>.</description>
    ///   </item>
    ///   <item>
    ///     <description><c>CLOUD_RUN_TASK_INDEX</c>: The zero-based index of the current task within the job execution, mapped to <see cref="TaskIndex"/>.</description>
    ///   </item>
    ///   <item>
    ///     <description><c>CLOUD_RUN_TASK_ATTEMPT</c>: The number of times this task has been retried, mapped to <see cref="TaskAttempt"/>.</description>
    ///   </item>
    /// </list>
    /// These values are useful for diagnostics, logging, and monitoring in distributed job environments.
    /// </summary>
    public class CloudRunJobConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the Cloud Run Job.
        /// Maps to the <c>CLOUD_RUN_JOB</c> environment variable.
        /// </summary>
        public string JobName { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the current Cloud Run Job execution.
        /// Maps to the <c>CLOUD_RUN_JOB_EXECUTION</c> environment variable.
        /// </summary>
        public string ExecutionId { get; set; }

        /// <summary>
        /// Gets or sets the zero-based index of the current task within the job execution.
        /// Maps to the <c>CLOUD_RUN_TASK_INDEX</c> environment variable.
        /// </summary>
        public string TaskIndex { get; set; }

        /// <summary>
        /// Gets or sets the number of times this task has been retried.
        /// Maps to the <c>CLOUD_RUN_TASK_ATTEMPT</c> environment variable.
        /// </summary>
        public string TaskAttempt { get; set; }
    }
}