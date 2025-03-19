#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Specifications/EventLogsSpecifications.cs
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

using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.NetStandardToolkit.Specifications;

namespace Utah.Udot.Atspm.Specifications
{
    /// <summary>
    /// Specification for <see cref="EventLogModelBase"/>
    /// </summary>
    public class EventLogSpecification : BaseSpecification<EventLogModelBase>
    {
        /// <summary>
        /// Matches <see cref="EventLogModelBase"/> by <see cref="EventLogModelBase.LocationIdentifier"/>
        /// and orders by <see cref="EventLogModelBase.Timestamp"/>
        /// </summary>
        /// <param name="Location"></param>
        public EventLogSpecification(Location Location) : base()
        {
            Criteria = c => c.LocationIdentifier == Location.LocationIdentifier;

            ApplyOrderBy(o => o.Timestamp);
        }

        /// <summary>
        /// Matches <see cref="EventLogModelBase"/> by <see cref="EventLogModelBase.LocationIdentifier"/>
        /// and <see cref="EventLogModelBase.Timestamp"/> is within <paramref name="start"/> and <paramref name="end"/>
        /// then orders by <see cref="EventLogModelBase.Timestamp"/>
        /// </summary>
        /// <param name="locationIdentifier"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public EventLogSpecification(string locationIdentifier, DateTime start, DateTime end) : base()
        {
            Criteria = c => c.LocationIdentifier == locationIdentifier && c.Timestamp >= start && c.Timestamp <= end;

            ApplyOrderBy(o => o.Timestamp);
        }
    }
}
