#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Extensions/ICompressedDataRepositoryExtensions.cs
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

using Utah.Udot.Atspm.Repositories.AggregationRepositories;
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace Utah.Udot.Atspm.Extensions
{
    /// <summary>
    /// Extensions for <see cref="ICompressedDataRepository{T}"/>
    /// </summary>
    public static class ICompressedDataRepositoryExtensions
    {
        /// <summary>
        /// Retrieves the distinct days that contain data for the specified location and type.
        /// </summary>
        /// <typeparam name="T">The compressed data entity type.</typeparam>
        /// <param name="repo">The repository to query.</param>
        /// <param name="locationIdentifier">The location identifier to filter by.</param>
        /// <param name="dataType">The data type to filter by.</param>
        /// <param name="start">Inclusive start date/time of the range.</param>
        /// <param name="end">Inclusive end date/time of the range.</param>
        /// <returns>A sorted, distinct list of <see cref="DateOnly"/> values representing days with data.</returns>
        public static IReadOnlyList<DateOnly> GetDaysWithData<T>(this ICompressedDataRepository<T> repo, string locationIdentifier, Type dataType, DateTime start, DateTime end) where T : CompressedDataBase
        {
            return repo.GetList()
                .FromSpecification(new CompressedDataSpecification<T>(locationIdentifier, start, end, dataType))
                .Select(x => DateOnly.FromDateTime(x.Start))
                .Distinct()
                .OrderBy(d => d)
                .ToList();
        }
    }
}
