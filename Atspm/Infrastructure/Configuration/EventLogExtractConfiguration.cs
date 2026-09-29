#region license
// Copyright 2026 Utah Departement of Transportation
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
    /// Configuration options for extracting controller event logs.
    /// </summary>
    public class EventLogExtractConfiguration
    {
        /// <summary>
        /// Gets or sets the output file format (e.g., csv, json).
        /// </summary>
        public string FileFormat { get; set; }

        /// <summary>
        /// Gets or sets the format string used when rendering timestamps.
        /// </summary>
        public string DateTimeFormat { get; set; }

        /// <summary>
        /// Gets or sets the collection of dates for which event logs are extracted.
        /// </summary>
        public IEnumerable<DateTime> Dates { get; set; }

        /// <summary>
        /// Gets or sets the collection of location identifiers to include in the extraction.
        /// </summary>
        public IEnumerable<string> Included { get; set; }

        /// <summary>
        /// Gets or sets the collection of location identifiers to exclude from the extraction.
        /// </summary>
        public IEnumerable<string> Excluded { get; set; }

        /// <summary>
        /// Gets or sets the root destination directory path where exported files will be written.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the maximum degree of parallelism for concurrent extraction tasks.
        /// </summary>
        public int ParallelProcesses { get; set; } = 1;
    }
}
