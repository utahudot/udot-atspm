#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Repositories.AggregationRepositories/LocationDataEFRepositoryBase.cs
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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utah.Udot.Atspm.Data.Interfaces;

namespace Utah.Udot.Atspm.Infrastructure.Repositories.AggregationRepositories
{
    /// <inheritdoc cref="ILocationDataRepository{T}"/>
    public abstract class LocationDataEFRepositoryBase<T>(DbContext db, ILogger<LocationDataEFRepositoryBase<T>> log)
        : ATSPMRepositoryEFBase<T>(db, log), ILocationDataRepository<T> where T : StartEndRange, ILocationLayer
    {
        /// <summary>
        /// Creates the specification used to filter records by location and date range.
        /// </summary>
        /// <param name="locationIdentifier">Location identifier.</param>
        /// <param name="start">Inclusive start date/time of the range.</param>
        /// <param name="end">Inclusive end date/time of the range.</param>
        /// <returns>The <see cref="ISpecification{T}"/> applied to date-range queries.</returns>
        protected virtual ISpecification<T> CreateDateRangeSpecification(string locationIdentifier, DateTime start, DateTime end)
        {
            return new LocationDateRangeSpecification<T>(locationIdentifier, start, end);
        }

        #region ILocationDataRepository

        /// <inheritdoc cref="ILocationDataRepository{T}.GetData(string, DateTime, DateTime)"/>
        public IAsyncEnumerable<T> GetData(string locationIdentifier, DateTime start, DateTime end)
        {
            return GetList()
                .FromSpecification(CreateDateRangeSpecification(locationIdentifier, start, end))
                .AsAsyncEnumerable();
        }

        /// <inheritdoc cref="ILocationDataRepository{T}.GetDaysWithData(string, DateTime, DateTime)"/>
        public IReadOnlyList<DateOnly> GetDaysWithData(string locationIdentifier, DateTime start, DateTime end)
        {
            return GetList()
                .FromSpecification(CreateDateRangeSpecification(locationIdentifier, start, end))
                .Select(x => DateOnly.FromDateTime(x.Start))
                .Distinct()
                .OrderBy(d => d)
                .ToList();
        }

        #endregion
    }
}
