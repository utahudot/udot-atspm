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

using Utah.Udot.Atspm.Data.Interfaces;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.NetStandardToolkit.Specifications;

namespace Utah.Udot.Atspm.Specifications
{
    /// <summary>
    /// A specification that filters <see cref="EventLogModelBase"/> entities based on location and time criteria.
    /// </summary>
    /// <remarks>
    /// This specification can be constructed either with an <see cref="ILocationLayer"/> to filter logs
    /// for a specific location, or with a location identifier and a time range to filter logs within that period.
    /// Results are ordered by <see cref="EventLogModelBase.Timestamp"/> in ascending order.
    /// </remarks>
    public class EventLogSpecification : BaseSpecification<EventLogModelBase>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogSpecification"/> class
        /// using the provided <see cref="ILocationLayer"/>.
        /// </summary>
        /// <param name="locationLayer">
        /// The location layer whose <see cref="ILocationLayer.LocationIdentifier"/> is used
        /// to filter event logs.
        /// </param>
        /// <remarks>
        /// This constructor filters event logs to only those matching the location identifier
        /// of the given <paramref name="locationLayer"/> and orders them by timestamp.
        /// </remarks>
        public EventLogSpecification(ILocationLayer locationLayer) : base()
        {
            Criteria = c => c.LocationIdentifier == locationLayer.LocationIdentifier;
            ApplyOrderBy(o => o.Timestamp);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogSpecification"/> class
        /// using a location identifier and a time range.
        /// </summary>
        /// <param name="locationIdentifier">The unique identifier of the location to filter logs for.</param>
        /// <param name="start">The start of the time range (inclusive).</param>
        /// <param name="end">The end of the time range (inclusive).</param>
        /// <remarks>
        /// This constructor filters event logs to only those matching the given location identifier
        /// and whose <see cref="EventLogModelBase.Timestamp"/> falls between <paramref name="start"/> and <paramref name="end"/>.
        /// Results are ordered by timestamp.
        /// </remarks>
        public EventLogSpecification(string locationIdentifier, DateTime start, DateTime end) : base()
        {
            Criteria = c => c.LocationIdentifier == locationIdentifier
                         && c.Timestamp >= start
                         && c.Timestamp <= end;

            ApplyOrderBy(o => o.Timestamp);
        }
    }
}
