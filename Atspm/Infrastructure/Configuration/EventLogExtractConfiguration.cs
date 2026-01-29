#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Configuration/EventLogExtractConfiguration.cs
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

namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    /// <summary>
    /// Provides configuration settings for extracting event log data, including
    /// formatting rules, date filters, inclusion and exclusion lists, and the
    /// destination directory for generated output.
    /// </summary>
    [ConfigurationSection(nameof(EventLogExtractConfiguration), "Configuration for extracting raw event log files")]
    public class EventLogExtractConfiguration
    {
        /// <summary>
        /// Gets or sets the file format used when exporting event log data.
        /// Common values might include CSV, JSON, or XML depending on the
        /// requirements of the consuming system.
        /// </summary>
        public string FileFormat { get; set; }

        /// <summary>
        /// Gets or sets the date and time format applied to timestamps within
        /// the exported event log data. This should follow standard .NET
        /// date/time format patterns.
        /// </summary>
        public string DateTimeFormat { get; set; }

        /// <summary>
        /// Gets or sets the collection of specific dates to extract event logs for.
        /// Only events occurring on these dates will be included in the output.
        /// </summary>
        public IEnumerable<DateTime> Dates { get; set; }

        /// <summary>
        /// Gets or sets a list of event identifiers or categories that should be
        /// explicitly included in the extraction. If populated, only matching
        /// events will be processed.
        /// </summary>
        public IEnumerable<string> Included { get; set; }

        /// <summary>
        /// Gets or sets a list of event identifiers or categories that should be
        /// excluded from the extraction. This is applied after any inclusion
        /// filters.
        /// </summary>
        public IEnumerable<string> Excluded { get; set; }

        /// <summary>
        /// Gets or sets the directory where extracted event log files will be
        /// written. This must point to a valid, writable directory on the system.
        /// </summary>
        public DirectoryInfo Path { get; set; }
    }
}
