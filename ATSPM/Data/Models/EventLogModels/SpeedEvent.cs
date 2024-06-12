#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - ATSPM.Data.Models.EventLogModels/SpeedEvent.cs
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


namespace ATSPM.Data.Models.EventLogModels
{
    /// <summary>
    /// Generic speed events
    /// </summary>
    public class SpeedEvent : EventLogModelBase
    {
        //TODO: is this the database id or the detector channel?
        /// <summary>
        /// Detector id
        /// </summary>
        public string DetectorId { get; set; }

        /// <summary>
        /// Miles per hour
        /// </summary>
        public int Mph { get; set; }
        
        /// <summary>
        /// Kilometers per hour
        /// </summary>
        public int Kph { get; set; }

        ///<inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is SpeedEvent @event &&
                   LocationIdentifier == @event.LocationIdentifier &&
                   Timestamp == @event.Timestamp &&
                   DetectorId == @event.DetectorId &&
                   Mph == @event.Mph &&
                   Kph == @event.Kph;
        }

        ///<inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(LocationIdentifier, Timestamp, DetectorId, Mph, Kph);
        }

        ///<inheritdoc/>
        public override string ToString()
        {
            return $"{LocationIdentifier}-{Timestamp}-{DetectorId}-{Mph}-{Kph}";
        }
    }
}
