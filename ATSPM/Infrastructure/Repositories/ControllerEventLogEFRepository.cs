using ATSPM.Application.Repositories;
using ATSPM.Application.Specifications;
using ATSPM.Data;
using ATSPM.Data.Models;
using ATSPM.Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories
{
    public class ControllerEventLogEFRepository : ATSPMRepositoryEFBase<ControllerLogArchive>, IControllerEventLogRepository
    {
        public ControllerEventLogEFRepository(EventLogContext db, ILogger<ControllerEventLogEFRepository> log) : base(db, log) { }

        #region IControllerEventLogRepository

        public IReadOnlyList<ControllerEventLog> GetLocationEventsBetweenDates(string locationId, DateTime startTime, DateTime endTime)
        {
            var result = table
                .FromSpecification(new ControllerLogDateRangeSpecification(locationId, startTime, endTime))
                .AsNoTracking()
                .AsEnumerable()
                .SelectMany(m => m.LogData)
                .Select(s => new ControllerEventLog()
                {
                    SignalIdentifier = locationId,
                    Timestamp = s.Timestamp,
                    EventCode = s.EventCode,
                    EventParam = s.EventParam
                })
                .FromSpecification(new ControllerLogDateTimeRangeSpecification(locationId, startTime, endTime))
                .ToList();

            return result;
        }

        #endregion
    }
}
