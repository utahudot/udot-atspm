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

namespace ATSPM.Infrasturcture.Repositories
{
    public class ControllerEventLogEFRepository : ATSPMRepositoryEFBase<ControllerLogArchive>, IControllerEventLogRepository
    {
        public ControllerEventLogEFRepository(ConfigContext db, ILogger<ControllerEventLogEFRepository> log) : base(db, log) { }

        #region IControllerEventLogRepository

        public IQueryable<ControllerEventLog> GetSignalEventsBetweenDates(string SignalId, DateTime startTime, DateTime endTime)
        {
            var result = table
                .FromSpecification(new ControllerLogDateRangeSpecification(SignalId, startTime, endTime))
                .AsNoTracking()
                .AsEnumerable()
                .SelectMany(s => s.LogData)
                .AsQueryable();

            return result;
        }

        #endregion
    }
}
