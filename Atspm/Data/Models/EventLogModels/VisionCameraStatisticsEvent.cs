#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Models.EventLogModels/PedestrianCounter.cs
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

#nullable disable


namespace Utah.Udot.Atspm.Data.Models.EventLogModels
{
    public class VisionCameraStatisticsEvent : EventLogModelBase
    {
        /// <summary>
        /// Zone Identifier
        /// </summary>
        public long ZoneId { get; set; }

        /// <summary>
        /// Name of the Zone
        /// </summary>
        public string ZoneName { get; set; }

        /// <summary>
        /// Average Speed in the Zone
        /// </summary>
        public double AverageSpeed { get; set; }

        /// <summary>
        /// Total Volume of Vehicles
        /// </summary>
        public int Volume { get; set; }

        /// <summary>
        /// Occupancy Rate
        /// </summary>
        public double Occupancy { get; set; }

        /// <summary>
        /// Number of Vehicles Going Through
        /// </summary>
        public int ThroughCount { get; set; }

        /// <summary>
        /// Number of Right Turns
        /// </summary>
        public int RightTurnCount { get; set; }

        /// <summary>
        /// Number of Left Turns
        /// </summary>
        public int LeftTurnCount { get; set; }

        /// <summary>
        /// Count of Left to Right Turns
        /// </summary>
        public int LeftToRightCount { get; set; }

        /// <summary>
        /// Count of Right to Left Turns
        /// </summary>
        public int RightToLeftCount { get; set; }

        ///<inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is VisionCameraStatisticsEvent trafficEvent &&
                   ZoneId == trafficEvent.ZoneId &&
                   ZoneName == trafficEvent.ZoneName &&
                   Timestamp == trafficEvent.Timestamp &&
                   AverageSpeed == trafficEvent.AverageSpeed &&
                   Volume == trafficEvent.Volume &&
                   Occupancy == trafficEvent.Occupancy &&
                   ThroughCount == trafficEvent.ThroughCount &&
                   RightTurnCount == trafficEvent.RightTurnCount &&
                   LeftTurnCount == trafficEvent.LeftTurnCount &&
                   LeftToRightCount == trafficEvent.LeftToRightCount &&
                   RightToLeftCount == trafficEvent.RightToLeftCount;
        }

        ///<inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(ZoneId, ZoneName, Timestamp, AverageSpeed, Volume, Occupancy);
        }

        ///<inheritdoc/>
        public override string ToString()
        {
            return $"{ZoneId}-{ZoneName}-{Timestamp}-{AverageSpeed}-{Volume}-{Occupancy}";
        }
    }

}
