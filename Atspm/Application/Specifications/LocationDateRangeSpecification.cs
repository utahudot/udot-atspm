#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Specifications/LocationDateRangeSpecification.cs
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
using Utah.Udot.NetStandardToolkit.Specifications;

namespace Utah.Udot.Atspm.Specifications
{
    /// <summary>
    /// Defines query specifications for retrieving location-scoped date-range records
    /// ordered by <see cref="StartEndRange.Start"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The entity type being queried. Must inherit from <see cref="StartEndRange"/> and implement <see cref="ILocationLayer"/>.
    /// </typeparam>
    public class LocationDateRangeSpecification<T> : BaseSpecification<T> where T : StartEndRange, ILocationLayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocationDateRangeSpecification{T}"/> class
        /// for a specific location and date range.
        /// </summary>
        /// <param name="locationIdentifier">
        /// Unique identifier of the location whose data is requested.
        /// </param>
        /// <param name="start">
        /// Inclusive start date of the query range. Records must end after this date.
        /// </param>
        /// <param name="end">
        /// Inclusive end date of the query range. Records must start before this date.
        /// </param>
        public LocationDateRangeSpecification(string locationIdentifier, DateTime start, DateTime end) : base()
        {
            Criteria = c => c.LocationIdentifier == locationIdentifier &&
                            c.End > start &&
                            c.Start < end;

            ApplyOrderBy(o => o.Start);
        }
    }
}
