using ATSPM.Data.Models;
using ATSPM.Data.Models.EventModels;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Repositories
{
    /// <summary>
    /// Device event log repository
    /// </summary>
    public interface IEventLogRepository : IAsyncRepository<CompressedEventsBase>
    {
        /// <summary>
        /// Get all events that match <paramref name="locationIdentifier"/> and <paramref name="date"/>
        /// </summary>
        /// <param name="locationIdentifier">Location identifier</param>
        /// <param name="date">Archive date of events</param>
        /// <returns></returns>
        IReadOnlyList<AtspmEventModelBase> GetEvents(string locationIdentifier, DateOnly date);

        /// <summary>
        /// Get all events that match <paramref name="locationIdentifier"/>, <paramref name="date"/> and <paramref name="deviceId"/>
        /// </summary>
        /// <param name="locationIdentifier">Location identifier</param>
        /// <param name="date">Archive date of events</param>
        /// <param name="deviceId">Deivce id events came from</param>
        /// <returns></returns>
        IReadOnlyList<AtspmEventModelBase> GetEvents(string locationIdentifier, DateOnly date, int deviceId);

        /// <summary>
        /// Get all events that match <paramref name="locationIdentifier"/>, <paramref name="date"/> and <paramref name="dataType"/>
        /// </summary>
        /// <param name="locationIdentifier">Location identifier</param>
        /// <param name="date">Archive date of events</param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        IReadOnlyList<AtspmEventModelBase> GetEvents(string locationIdentifier, DateOnly date, Type dataType);

        /// <summary>
        /// Get all events that match <paramref name="locationIdentifier"/> and <paramref name="date"/>
        /// Where date type of derrived from <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Data type of <see cref="AtspmEventModelBase"/></typeparam>
        /// <param name="locationIdentifier">Location identifier</param>
        /// <param name="date">Archive date of events</param>
        /// <returns></returns>
        IReadOnlyList<T> GetEvents<T>(string locationIdentifier, DateOnly date) where T : AtspmEventModelBase;

        /// <summary>
        /// Get all events that match <paramref name="locationIdentifier"/>, <paramref name="date"/> and <paramref name="deviceId"/>
        /// Where date type of derrived from <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Data type of <see cref="AtspmEventModelBase"/></typeparam>
        /// <param name="locationIdentifier">Location identifier</param>
        /// <param name="date">Archive date of events</param>
        /// <param name="deviceId">Deivce id events came from</param>
        /// <returns></returns>
        IReadOnlyList<T> GetEvents<T>(string locationIdentifier, DateOnly date, int deviceId) where T : AtspmEventModelBase;
    }
}
