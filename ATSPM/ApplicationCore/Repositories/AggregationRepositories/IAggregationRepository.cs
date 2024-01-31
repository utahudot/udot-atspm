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
        /// Get all events that match <paramref name="locationIdentifier"/> and <paramref name="date"/>
        /// </summary>
        /// <param name="locationIdentifier">Location identifier</param>
        /// <param name="date">Archive date of aggregations</param>
        /// <returns></returns>
        IReadOnlyList<AggregationModelBase> GetAggregations(string locationIdentifier, DateOnly date);

        /// <summary>
        /// Get all events that match <paramref name="locationIdentifier"/>, <paramref name="date"/> and <paramref name="dataType"/>
        /// </summary>
        /// <param name="locationIdentifier">Location identifier</param>
        /// <param name="date">Archive date of aggregations</param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        IReadOnlyList<AggregationModelBase> GetAggregations(string locationIdentifier, DateOnly date, Type dataType);

        /// <summary>
        /// Get all events that match <paramref name="locationIdentifier"/> and <paramref name="date"/>
        /// Where date type of derrived from <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Data type of <see cref="AggregationModelBase"/></typeparam>
        /// <param name="locationIdentifier">Location identifier</param>
        /// <param name="date">Archive date of aggregations</param>
        /// <returns></returns>
        IReadOnlyList<T> GetAggregations<T>(string locationIdentifier, DateOnly date) where T : AggregationModelBase;
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
