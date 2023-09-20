using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories
{
    public class ControllerEventLogEFRepository : ATSPMRepositoryEFBase<ControllerLogArchive>, IControllerEventLogRepository
    {
        public ControllerEventLogEFRepository(EventLogContext db, ILogger<ControllerEventLogEFRepository> log) : base(db, log) { }

        #region IControllerEventLogRepository

        public IReadOnlyList<ControllerEventLog> GetSignalEventsBetweenDates(string signalId, DateTime startTime, DateTime endTime)
        {
            var result = GetList()
                .FromSpecification(new ControllerLogDateRangeSpecification(signalId, startTime, endTime))
                .AsNoTracking()
                .AsEnumerable()
                .SelectMany(a => a.LogData.Select(s => new ControllerEventLog()
                {
                    SignalIdentifier = a.SignalIdentifier,
                    EventCode = s.EventCode,
                    EventParam = s.EventParam,
                    Timestamp = s.Timestamp
                }))
                .FromSpecification(new ControllerLogDateTimeRangeSpecification(signalId, startTime, endTime)).ToList();

            return result;
        }

        #endregion
    }
}
