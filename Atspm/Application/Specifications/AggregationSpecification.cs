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
    /// A specification that filters <see cref="AggregationModelBase"/> entities
    /// based on location and overlapping time range.
    /// </summary>
    public class AggregationSpecification : BaseSpecification<AggregationModelBase>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregationSpecification"/> class
        /// with the provided location identifier and time range.
        /// </summary>
        /// <param name="locationIdentifier">The unique identifier of the location to filter aggregations for.</param>
        /// <param name="start">The start of the time window (inclusive boundary for overlap).</param>
        /// <param name="end">The end of the time window (inclusive boundary for overlap).</param>
        /// <remarks>
        /// Aggregations are included if their <see cref="AggregationModelBase.End"/> is after <paramref name="start"/>
        /// and their <see cref="AggregationModelBase.Start"/> is before <paramref name="end"/>.
        /// This effectively selects all aggregations that overlap with the given time range.
        /// </remarks>
        public AggregationSpecification(string locationIdentifier, DateTime start, DateTime end) : base()
        {
            Criteria = c => c.LocationIdentifier == locationIdentifier
                         && c.End > start
                         && c.Start < end;

            ApplyOrderBy(o => o.Start);
        }
    }
}
