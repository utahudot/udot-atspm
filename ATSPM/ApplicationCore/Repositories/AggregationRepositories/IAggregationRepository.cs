#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Repositories.AggregationRepositories/IAggregationRepository.cs
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
using ATSPM.Data.Models;
using ATSPM.Data.Models.AggregationModels;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Repositories.AggregationRepositories
{
    /// <summary>
    /// Aggregation repository
    /// </summary>
    public interface IAggregationRepository : IAsyncRepository<CompressedAggregationBase>
    {
        /// <summary>
        /// Get all aggregations that match <paramref name="locationIdentifier"/>, <paramref name="start"/>/<paramref name="end"/> and <paramref name="dataType"/>
        /// </summary>
        /// <param name="locationIdentifier">Location identifier</param>
        /// <param name="start">Archive date of event to start with</param>
        /// <param name="end">Archive date of event to end with</param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        IReadOnlyList<CompressedAggregationBase> GetArchivedAggregations(string locationIdentifier, DateOnly start, DateOnly end, Type dataType);

        /// <summary>
        /// Get all aggregations that match <paramref name="locationIdentifier"/> and <paramref name="start"/>/<paramref name="end"/>
        /// Where date type of derrived from <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Data type of <see cref="AggregationModelBase"/></typeparam>
        /// <param name="locationIdentifier">Location identifier</param>
        /// <param name="start">Archive date of event to start with</param>
        /// <param name="end">Archive date of event to end with</param>
        /// <returns></returns>
        IReadOnlyList<CompressedAggregations<T>> GetArchivedAggregations<T>(string locationIdentifier, DateOnly start, DateOnly end) where T : AggregationModelBase;

        /// <summary>
        /// Get all aggregations that match <paramref name="locationIdentifier"/> and <paramref name="start"/>/<paramref name="end"/>
        /// </summary>
        /// <param name="locationIdentifier">Location identifier</param>
        /// <param name="start">Archive date of event to start with</param>
        /// <param name="end">Archive date of event to end with</param>
        /// <returns></returns>
        IReadOnlyList<CompressedAggregationBase> GetArchivedAggregations(string locationIdentifier, DateOnly start, DateOnly end);
    }

    /// <summary>
    /// Generic interface for accessing aggregations
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAggregationRepository<T> : IAsyncRepository<CompressedAggregations<T>> where T : AggregationModelBase
    {
        /// <summary>
        /// Get all <see cref="AggregationModelBase"/> by <c>LocationId</c> and date range
        /// </summary>
        /// <param name="locationId">Location identifier</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <returns></returns>
        IReadOnlyList<T> GetAggregationsBetweenDates(string locationId, DateTime startTime, DateTime endTime);
    }
}
