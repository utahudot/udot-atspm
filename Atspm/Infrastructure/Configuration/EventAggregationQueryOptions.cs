#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Configuration/EventAggregationQueryOptions.cs
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
    public class EventAggregationQueryOptions
    {
        /// <summary>
        /// List of <see cref="Location.LocationIdentifier"/> to include
        /// </summary>
        public IEnumerable<string> IncludedLocations { get; set; } = [];

        /// <summary>
        /// List of <see cref="Location.LocationIdentifier"/> to exclude
        /// </summary>
        public IEnumerable<string> ExcludedLocations { get; set; } = [];

        /// <summary>
        /// List of <see cref="LocationType.Name"/> to include
        /// </summary>
        public IEnumerable<string> IncludedLocationTypes { get; set; } = [];

        /// <summary>
        /// List of <see cref="Area.Name"/> to include
        /// </summary>
        public IEnumerable<string> IncludedAreas { get; set; } = [];

        /// <summary>
        /// List of <see cref="Jurisdiction.Name"/> to include
        /// </summary>
        public IEnumerable<string> IncludedJurisdictions { get; set; } = [];

        /// <summary>
        /// List of <see cref="Region.Description"/> to include
        /// </summary>
        public IEnumerable<string> IncludedRegions { get; set; } = [];

        /// <inheritdoc/>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{nameof(EventAggregationQueryOptions)}***************************************************");

            foreach (var i in IncludedLocations)
            {
                sb.AppendLine($"{nameof(IncludedLocations)}: {i}");
            }

            foreach (var i in IncludedLocationTypes)
            {
                sb.AppendLine($"{nameof(IncludedLocationTypes)}: {i}");
            }

            foreach (var i in IncludedAreas)
            {
                sb.AppendLine($"{nameof(IncludedAreas)}: {i}");
            }

            foreach (var i in IncludedJurisdictions)
            {
                sb.AppendLine($"{nameof(IncludedJurisdictions)}: {i}");
            }

            foreach (var i in IncludedRegions)
            {
                sb.AppendLine($"{nameof(IncludedRegions)}: {i}");
            }

            sb.AppendLine($"{nameof(EventAggregationQueryOptions)}***************************************************");

            return sb.ToString();
        }
    }
}
