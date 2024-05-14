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
        IReadOnlyList<T> GetAllAggregationsBetweenDates(DateTime startTime, DateTime endTime);
    }
}
