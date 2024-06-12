#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - ATSPM.Data.Models.EventLogModels/IndianaEvent.cs
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
    /// Indiana Traffic Signal Hi Resolution Data Logger Enumerations
    /// <seealso cref="https://docs.lib.purdue.edu/jtrpdata/4/"/>
    /// </summary>
    public class IndianaEvent : EventLogModelBase
    {
        /// <summary>
        /// Event code from <see cref="IndianaEnumerations"/>
        /// </summary>
        public short EventCode { get; set; }

        /// <summary>
        /// Event parameter that is specific to <see cref="EventCode"/>
        /// </summary>
        public short EventParam { get; set; }

        ///<inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is IndianaEvent @event &&
                   LocationIdentifier == @event.LocationIdentifier &&
                   Timestamp == @event.Timestamp &&
                   EventCode == @event.EventCode &&
                   EventParam == @event.EventParam;
        }

        ///<inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(LocationIdentifier, Timestamp, EventCode, EventParam);
        }

        ///<inheritdoc/>
        public override string ToString()
        {
            return $"{LocationIdentifier}-{Timestamp}-{EventCode}-{EventParam}";
        }
    }
}
