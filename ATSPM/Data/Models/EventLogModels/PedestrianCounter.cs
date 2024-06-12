#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - ATSPM.Data.Models.EventLogModels/PedestrianCounter.cs
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
    /// Generic pedestrian counter events
    /// </summary>
    public class PedestrianCounter : EventLogModelBase
    {
        /// <summary>
        /// Input count
        /// </summary>
        public ushort In { get; set; }
        
        /// <summary>
        /// Output count
        /// </summary>
        public ushort Out { get; set; }

        ///<inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is PedestrianCounter counter &&
                   LocationIdentifier == counter.LocationIdentifier &&
                   Timestamp == counter.Timestamp &&
                   In == counter.In &&
                   Out == counter.Out;
        }

        ///<inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(LocationIdentifier, Timestamp, In, Out);
        }

        ///<inheritdoc/>
        public override string ToString()
        {
            return $"{LocationIdentifier}-{Timestamp}-{In}-{Out}";
        }
    }
}
