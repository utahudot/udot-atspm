using ATSPM.Application.Models;
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
namespace ATSPM.Infrasturcture.Repositories
{
    public class ControllerEventLogEFRepository : ATSPMRepositoryEFBase<Signal>, IControllerEventLogRepository
    {
        public ControllerEventLogEFRepository(DbContext db, ILogger<SignalEFRepository> log) : base(db, log) { }

        [Obsolete("This method isn't currently being used, Also can't use this becuase it has times not dates")]
        public bool CheckForRecords(string signalId, DateTime startTime, DateTime endTime)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<ControllerEventLog> GetAllAggregationCodes(string signalId, DateTime startTime, DateTime endTime)
        {
            //var codes = new List<int> { 150, 114, 113, 112, 105, 102, 1 };

            throw new NotImplementedException();



            //var records = _db.Set<ControllerLogArchive>().Where(c => c.SignalId == signalId && c.ArchiveDate >= startTime && c.ArchiveDate <= endTime &&
            //                codes.Contains(c.EventCode))
            //    .ToList();
            //return records;
        }

        public int GetApproachEventsCountBetweenDates(int approachId, DateTime startTime, DateTime endTime, int phaseNumber)
        {
            throw new NotImplementedException();
        }

        public int GetDetectorActivationCount(string signalId, DateTime startTime, DateTime endTime, int detectorChannel)
        {
            throw new NotImplementedException();
        }

        public int GetEventCountByEventCodesParamDateTimeRange(string signalId, DateTime startTime, DateTime endTime, int startHour, int startMinute, int endHour, int endMinute, IReadOnlyList<int> eventCodes, int param)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<ControllerEventLog> GetEventsBetweenDates(DateTime startTime, DateTime endTime)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParam(string signalId, DateTime startTime, DateTime endTime, IReadOnlyList<int> eventCodes, int param)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamDateTimeRange(string signalId, DateTime startTime, DateTime endTime, int startHour, int startMinute, int endHour, int endMinute, IReadOnlyList<int> eventCodes, int param)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamWithLatencyCorrection(string signalId, DateTime startTime, DateTime endTime, IReadOnlyList<int> eventCodes, int param, double latencyCorrection)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamWithOffsetAndLatencyCorrection(string signalId, DateTime startTime, DateTime endTime, IReadOnlyList<int> eventCodes, int param, double offset, double latencyCorrection)
        {
            throw new NotImplementedException();
        }

        public ControllerEventLog GetFirstEventBeforeDate(string signalId, int eventCode, DateTime date)
        {
            throw new NotImplementedException();
        }

        public ControllerEventLog GetFirstEventBeforeDateByEventCodeAndParameter(string signalId, int eventCode, int eventParam, DateTime date)
        {
            throw new NotImplementedException();
        }

        public DateTime GetMostRecentRecordTimestamp(string signalID)
        {
            throw new NotImplementedException();
        }

        public int GetRecordCount(string signalId, DateTime startTime, DateTime endTime)
        {
            throw new NotImplementedException();
        }

        public int GetRecordCountByParameterAndEvent(string signalId, DateTime startTime, DateTime endTime, IReadOnlyList<int> eventParameters, IReadOnlyList<int> events)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<ControllerEventLog> GetRecordsByParameterAndEvent(string signalId, DateTime startTime, DateTime endTime, IReadOnlyList<int> eventParameters, IReadOnlyList<int> eventCodes)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<ControllerEventLog> GetSignalEventsBetweenDates(string signalId, DateTime startTime, DateTime endTime)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<ControllerEventLog> GetSignalEventsByEventCode(string signalId, DateTime startTime, DateTime endTime, int eventCode)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<ControllerEventLog> GetSignalEventsByEventCodes(string signalId, DateTime startTime, DateTime endTime, IReadOnlyList<int> eventCodes)
        {
            throw new NotImplementedException();
        }

        public int GetSignalEventsCountBetweenDates(string signalId, DateTime startTime, DateTime endTime)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<ControllerEventLog> GetSplitEvents(string signalId, DateTime startTime, DateTime endTime)
        {
            throw new NotImplementedException();
        }

        public double GetTmcVolume(DateTime startDate, DateTime endDate, string signalId, int phase)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<ControllerEventLog> GetTopEventsAfterDateByEventCodesParam(string signalId, DateTime timestamp, IReadOnlyList<int> eventCodes, int param, int top)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<ControllerEventLog> GetTopNumberOfSignalEventsBetweenDates(string signalId, int numberOfRecords, DateTime startTime, DateTime endTime)
        {
            throw new NotImplementedException();
        }
    }
}
