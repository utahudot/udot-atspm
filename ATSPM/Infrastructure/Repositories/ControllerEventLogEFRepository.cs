using ATSPM.Data.Models;
using ATSPM.Application.Repositories;
using ATSPM.Application.ValueObjects;
using ATSPM.Domain.Common;
using ATSPM.Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ATSPM.Application.Specifications;
using ATSPM.Domain.Services;
using ATSPM.Domain.Specifications;
using ATSPM.Data;

namespace ATSPM.Infrastructure.Repositories
{
    public class ControllerEventLogEFRepository : ATSPMRepositoryEFBase<ControllerLogArchive>, IControllerEventLogRepository
    {
        public ControllerEventLogEFRepository(EventLogContext db, ILogger<ControllerEventLogEFRepository> log) : base(db, log) { }

        #region IControllerEventLogRepository

        public IReadOnlyList<ControllerEventLog> GetSignalEventsBetweenDates(string signalId, DateTime startTime, DateTime endTime)
        {
            var result = table
                .FromSpecification(new ControllerLogDateRangeSpecification(signalId, startTime, endTime))
                .AsNoTracking()
                .AsEnumerable()
                .SelectMany(s => s.LogData)
                .FromSpecification(new ControllerLogDateTimeRangeSpecification(signalId, startTime, endTime))
                .ToList();

            return result;
        }

        #endregion
    }
}
