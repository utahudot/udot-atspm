using ATSPM.Data.Models;
using ATSPM.Data.Models.EventModels;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Repositories
{
    /// <summary>
    /// Indianna event log repository
    /// </summary>
    public interface IIndiannaEventRepository : IAsyncRepository<CompressedEvents<IndiannaEvent>>
    {
        /// <summary>
        /// Get all <see cref="IndiannaEvent"/> logs by <c>LocationId</c> and date range
        /// </summary>
        /// <param name="locationId">Location identifier</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <returns></returns>
        IReadOnlyList<IndiannaEvent> GetEventsBetweenDates(string locationId, DateTime startTime, DateTime endTime);
    }
}
