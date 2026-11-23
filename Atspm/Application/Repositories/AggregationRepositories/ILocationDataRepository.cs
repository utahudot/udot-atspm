#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Repositories.AggregationRepositories/ILocationDataRepository.cs
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
using Utah.Udot.NetStandardToolkit.Services;

namespace Utah.Udot.Atspm.Repositories.AggregationRepositories
{
    /// <summary>
    /// Base repository interface for location-scoped date-range data models.
    /// </summary>
    /// <typeparam name="T">The entity type inheriting <see cref="StartEndRange"/> and implementing <see cref="ILocationLayer"/>.</typeparam>
    public interface ILocationDataRepository<T> : IAsyncRepository<T> where T : StartEndRange, ILocationLayer
    {
        /// <summary>
        /// Gets all data matching <paramref name="locationIdentifier"/> within the <paramref name="start"/> and <paramref name="end"/> range.
        /// </summary>
        /// <param name="locationIdentifier">Location identifier.</param>
        /// <param name="start">Inclusive start date/time of the range.</param>
        /// <param name="end">Inclusive end date/time of the range.</param>
        /// <returns>An asynchronous stream of matching <typeparamref name="T"/> records.</returns>
        IAsyncEnumerable<T> GetData(string locationIdentifier, DateTime start, DateTime end);

        /// <summary>
        /// Retrieves the distinct days that contain data for the specified location within the given date range.
        /// </summary>
        /// <param name="locationIdentifier">Location identifier.</param>
        /// <param name="start">Inclusive start date/time of the range.</param>
        /// <param name="end">Inclusive end date/time of the range.</param>
        /// <returns>A sorted, distinct list of <see cref="DateOnly"/> values representing days with data.</returns>
        IReadOnlyList<DateOnly> GetDaysWithData(string locationIdentifier, DateTime start, DateTime end);
    }
}
