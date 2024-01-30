using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Repositories.EventLogRepositories
{
    /// <summary>
    /// Indianna event log repository
    /// </summary>
    public interface IIndiannaEventLogRepository : IAsyncRepository<CompressedEventLogs<IndianaEvent>>
    {
        /// <summary>
        /// Get all <see cref="IndianaEvent"/> logs by <c>LocationId</c> and date range
        /// </summary>
        /// <param name="locationId">Location identifier</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <returns></returns>
        IReadOnlyList<IndianaEvent> GetEventsBetweenDates(string locationId, DateTime startTime, DateTime endTime);
    }
}
