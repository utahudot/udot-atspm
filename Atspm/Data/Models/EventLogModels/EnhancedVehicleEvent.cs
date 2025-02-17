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


    public class EnhancedVehicleEvent : EventLogModelBase
    {
        /// <summary>
        /// Zone Identifier
        /// </summary>
        public long ZoneId { get; set; } //needs converting to location identifier

        /// <summary>
        /// Name of the Zone
        /// </summary>
        public string ZoneName { get; set; }

        /// <summary>
        /// Object Type (e.g., Vehicle)
        /// </summary>
        public string ObjectType { get; set; }

        /// <summary>
        /// Speed of the object
        /// </summary>
        public double Speed { get; set; }

        /// <summary>
        /// Length of the object
        /// </summary>
        public double Length { get; set; }

        /// <summary>
        /// Direction of the object
        /// </summary>
        public string Direction { get; set; }

        ///<inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is EnhancedVehicleEvent zoneEvent &&
                   ZoneId == zoneEvent.ZoneId &&
                   ZoneName == zoneEvent.ZoneName &&
                   ObjectType == zoneEvent.ObjectType &&
                   Speed == zoneEvent.Speed &&
                   Length == zoneEvent.Length &&
                   Direction == zoneEvent.Direction;
        }

        ///<inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(ZoneId, ZoneName, Timestamp, ObjectType, Speed, Length, Direction);
        }

        ///<inheritdoc/>
        public override string ToString()
        {
            return $"{ZoneId}-{ZoneName}-{Timestamp}-{ObjectType}-{Speed}-{Length}-{Direction}";
        }
    }

}
