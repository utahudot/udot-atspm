#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Specifications/AggregationSpecification.cs
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

using Utah.Udot.NetStandardToolkit.Specifications;

namespace Utah.Udot.Atspm.Specifications
{
    /// <summary>
    /// Specification for filtering <see cref="CompressedAggregationBase"/> entities 
    /// within a given date range for a specific location.
    /// </summary>
    public class CompressedAggregationsSpecification : BaseSpecification<CompressedAggregationBase>
    {
        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="CompressedAggregationsSpecification"/> class.
        /// </summary>
        /// <param name="locationIdentifier">
        /// The identifier of the location to filter aggregations for.
        /// </param>
        /// <param name="start">
        /// The inclusive lower bound of the date range.
        /// </param>
        /// <param name="end">
        /// The exclusive upper bound of the date range.
        /// </param>
        /// <remarks>
        /// The constructor sets the filtering criteria and applies ordering 
        /// by the aggregation start date.
        /// </remarks>
        public CompressedAggregationsSpecification(string locationIdentifier, DateTime start, DateTime end) : base()
        {
            Criteria = c => c.LocationIdentifier == locationIdentifier && c.End > start && c.Start < end;

            ApplyOrderBy(o => o.Start);
        }
    }
}
