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
    /// Represents configuration settings for enabling and controlling Google Diagnostics integration,
    /// including toggling diagnostics, setting the minimum log level, and specifying the Google Cloud project ID.
    /// </summary>
    public class GoogleDiagnosticsConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether Google Diagnostics is enabled.
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the minimum <see cref="LogLevel"/> for logging diagnostics to Google.
        /// </summary>
        public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;

        /// <summary>
        /// Gets or sets the Google Cloud Project ID used for diagnostics.
        /// </summary>
        public string ProjectId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the service reporting diagnostics to Google. 
        /// Defaults to the current application domain's friendly name.
        /// </summary>
        public string ServiceName { get; set; } = AppDomain.CurrentDomain.FriendlyName;

        /// <summary>
        /// Gets or sets the version of the service reporting diagnostics to Google.
        /// Defaults to the entry assembly's version or "1.0.0" if unavailable.
        /// </summary>
        public string Version { get; set; } = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "1.0.0";
    }
}