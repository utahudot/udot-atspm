#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Specifications/SignalTimingPlanSpecification.cs
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

namespace Utah.Udot.Atspm.Specifications
{
    /// <summary>
    /// Defines query specifications for retrieving valid <see cref="SignalTimingPlan"/> records
    /// based on location and date range.
    /// </summary>
    public class SignalTimingPlanSpecification : LocationDateRangeSpecification<SignalTimingPlan>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignalTimingPlanSpecification"/> class
        /// for valid signal timing plans at a specific location and date range.
        /// </summary>
        /// <param name="locationIdentifier">
        /// Unique identifier of the location whose timing plans are requested.
        /// </param>
        /// <param name="start">
        /// Inclusive start date of the query range. Records must end after this date.
        /// </param>
        /// <param name="end">
        /// Inclusive end date of the query range. Records must start before this date.
        /// </param>
        public SignalTimingPlanSpecification(string locationIdentifier, DateTime start, DateTime end)
            : base(locationIdentifier, start, end)
        {
            Criteria = c => c.LocationIdentifier == locationIdentifier &&
                            c.End > start &&
                            c.Start < end &&
                            c.Valid;
        }
    }
}
