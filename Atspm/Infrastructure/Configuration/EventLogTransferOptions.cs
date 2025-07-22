#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Configuration/EventLogTransferOptions.cs
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

using System.Text;

namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    /// <summary>
    /// Options for transferring event logs between repositories.
    /// </summary>
    public class EventLogTransferOptions
    {
        /// <summary>
        /// Configuration for the source repository from which logs will be transferred
        /// </summary>
        public RepositoryConfiguration SourceRepository { get; set; } = new RepositoryConfiguration();

        /// <summary>
        /// Configuration for the destination repository to which logs will be transferred
        /// </summary>
        public RepositoryConfiguration DestinationRepository { get; set; } = new RepositoryConfiguration();

        /// <summary>
        /// List of <see cref="Location.LocationIdentifier"/> to include
        /// </summary>
        public IEnumerable<string> IncludedLocations { get; set; } = [];

        /// <summary>
        /// List of <see cref="Location.LocationIdentifier"/> to exclude
        /// </summary>
        public IEnumerable<string> ExcludedLocations { get; set; } = [];

        /// <summary>
        /// Start date for the transfer
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date for the transfer
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// List of <see cref="Device"/> Id's to include
        /// </summary>
        public IEnumerable<int> IncludedDeviceIds { get; set; } = [];

        /// <summary>
        /// Data type of the event logs to transfer. Defaults to "all" for all types.
        /// </summary>
        public string DataType { get; set; } = "all"; // Default to all data types

        /// <inheritdoc/>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{nameof(EventLogTransferOptions)}***************************************************");

            sb.AppendLine($"{nameof(SourceRepository)}: {SourceRepository}");
            sb.AppendLine($"{nameof(DestinationRepository)}: {DestinationRepository}");
            sb.AppendLine($"{nameof(StartDate)}: {StartDate}");
            sb.AppendLine($"{nameof(EndDate)}: {EndDate}");

            foreach (var i in IncludedLocations)
            {
                sb.AppendLine($"{nameof(IncludedLocations)}: {i}");
            }

            foreach (var i in ExcludedLocations)
            {
                sb.AppendLine($"{nameof(ExcludedLocations)}: {i}");
            }

            foreach (var i in IncludedDeviceIds)
            {
                sb.AppendLine($"{nameof(IncludedDeviceIds)}: {i}");
            }

            sb.AppendLine($"{nameof(DataType)}: {DataType}");

            sb.AppendLine($"{nameof(EventLogTransferOptions)}***************************************************");

            return sb.ToString();
        }
    }
}
