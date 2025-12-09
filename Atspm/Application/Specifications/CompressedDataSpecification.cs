#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Specifications/CompressedDataSpecification.cs
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
    /// Defines query specifications for retrieving <see cref="CompressedDataBase"/> records
    /// based on location, date range, and optional data type.
    /// </summary>
    /// <typeparam name="T">
    /// The entity type being queried. Must inherit from <see cref="CompressedDataBase"/>.
    /// </typeparam>
    /// <remarks>
    /// This specification applies filtering criteria and ordering rules that can be used
    /// with repositories or query providers supporting the Specification pattern.
    /// </remarks>
    public class CompressedDataSpecification<T> : BaseSpecification<T> where T : CompressedDataBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompressedDataSpecification{T}"/> class
        /// for a specific location and date range.
        /// </summary>
        /// <param name="locationIdentifier">
        /// Unique identifier of the location whose compressed data is requested.
        /// </param>
        /// <param name="start">
        /// Inclusive start date of the query range. Records must end after this date.
        /// </param>
        /// <param name="end">
        /// Inclusive end date of the query range. Records must start before this date.
        /// </param>
        /// <remarks>
        /// Results are ordered by the <c>Start</c> property of <see cref="CompressedDataBase"/>.
        /// </remarks>
        public CompressedDataSpecification(string locationIdentifier, DateTime start, DateTime end) : base()
        {
            Criteria = c => c.LocationIdentifier == locationIdentifier &&
                            c.End > start &&
                            c.Start < end;

            ApplyOrderBy(o => o.Start);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompressedDataSpecification{T}"/> class
        /// for a specific location, date range, and data type.
        /// </summary>
        /// <param name="locationIdentifier">
        /// Unique identifier of the location whose compressed data is requested.
        /// </param>
        /// <param name="start">
        /// Inclusive start date of the query range. Records must end after this date.
        /// </param>
        /// <param name="end">
        /// Inclusive end date of the query range. Records must start before this date.
        /// </param>
        /// <param name="type">
        /// CLR type representing the aggregation data type to filter by.
        /// </param>
        /// <remarks>
        /// Results are ordered by the <c>Start</c> property of <see cref="CompressedDataBase"/>.
        /// </remarks>
        public CompressedDataSpecification(string locationIdentifier, DateTime start, DateTime end, Type type) : base()
        {
            Criteria = c => c.LocationIdentifier == locationIdentifier &&
                            c.End > start &&
                            c.Start < end &&
                            c.DataType == type;

            ApplyOrderBy(o => o.Start);
        }
    }

}