#region license
// Copyright 2025 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Models/WatchDogLogEvent.cs
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

using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Interfaces;
using Utah.Udot.NetStandardToolkit.Interfaces;

namespace Utah.Udot.Atspm.Data.Models
{
    /// <summary>
    /// Watchdog log event
    /// </summary>
    public class WatchDogLogEvent : ILocationLayer, ITimestamp
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Location id
        /// </summary>
        public int LocationId { get; set; }

        /// <summary>
        /// Location identifier
        /// </summary>
        public string LocationIdentifier { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Component type
        /// </summary>
        public WatchDogComponentTypes ComponentType { get; set; }

        /// <summary>
        /// Component id
        /// </summary>
        public int ComponentId { get; set; }

        /// <summary>
        /// Issue type
        /// </summary>
        public WatchDogIssueTypes IssueType { get; set; }

        /// <summary>
        /// Details
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// Phase
        /// </summary>
        public int? Phase { get; set; }

        /// <summary>
        /// Watchdog log event
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="locationIdentifier"></param>
        /// <param name="timestamp"></param>
        /// <param name="componentType"></param>
        /// <param name="componentId"></param>
        /// <param name="issueType"></param>
        /// <param name="details"></param>
        /// <param name="phase"></param>
        public WatchDogLogEvent(int locationId, string locationIdentifier, DateTime timestamp, WatchDogComponentTypes componentType, int componentId, WatchDogIssueTypes issueType, string details, int? phase)
        {
            this.LocationId = locationId;
            this.LocationIdentifier = locationIdentifier;
            Timestamp = timestamp;
            ComponentType = componentType;
            ComponentId = componentId;
            IssueType = issueType;
            Details = details;
            Phase = phase;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"[{LocationId}-{Timestamp}] {ComponentType} (ID: {ComponentId}) - {IssueType}: {Details}";
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            WatchDogLogEvent other = (WatchDogLogEvent)obj;

            return LocationIdentifier == other.LocationIdentifier &&
                    Timestamp == other.Timestamp &&
                   ComponentType == other.ComponentType &&
                   ComponentId == other.ComponentId &&
                   IssueType == other.IssueType &&
                       Phase == other.Phase;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(LocationIdentifier, Timestamp, ComponentType, ComponentId, IssueType, (Phase.HasValue ? Phase.Value.GetHashCode() : -1));
        }
    }
}