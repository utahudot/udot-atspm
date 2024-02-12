using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Application.Specifications;
using ATSPM.Data;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using ATSPM.Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories.EventLogRepositories
{
    /// <summary>
    /// Generic base for accessing device event logs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class EventLogEFRepositoryBase<T> : ATSPMRepositoryEFBase<CompressedEventLogs<T>>, IEventLogRepository<T> where T : EventLogModelBase
    {
        ///<inheritdoc/>
        public EventLogEFRepositoryBase(EventLogContext db, ILogger<EventLogEFRepositoryBase<T>> log) : base(db, log) { }

        #region IEventLogRepository

        ///<inheritdoc/>
        public virtual IReadOnlyList<T> GetEventsBetweenDates(string locationId, DateTime startTime, DateTime endTime)
        {
            //var result = table
            //    .FromSpecification(new EventLogDateRangeSpecification(locationId, DateOnly.FromDateTime(startTime), DateOnly.FromDateTime(endTime)))
            //    .AsNoTracking()
            //    .AsEnumerable()
            //    .SelectMany(m => m.Data)
            //    .FromSpecification(new EventLogDateTimeRangeSpecification(locationId, startTime, endTime))
            //    .Cast<T>()
            //    .ToList();

            //return result;

            throw new NotImplementedException();
        }

        #endregion
    }
}
