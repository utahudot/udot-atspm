using ATSPM.Application.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATSPM.Application.Repositories
{
    public interface IControllerEventLogRepository : IAsyncRepository<ControllerLogArchive>
    {
        IQueryable<ControllerEventLog> GetSignalEventsBetweenDates(string signalId, DateTime startTime, DateTime endTime);

        #region ExtensionMethods

        //int GetSignalEventsCountBetweenDates(string signalId, DateTime startTime, DateTime endTime);

        //IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParam(string signalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param);

        //IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParam(string signalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param, double latencyCorrection);

        //IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParam(string signalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param, double offset, double latencyCorrection);

        //IReadOnlyList<ControllerEventLog> GetRecordsByParameterAndEvent(string signalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventParameters, IEnumerable<int> eventCodes);

        //int GetRecordCountByParameterAndEvent(string signalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventParameters, IEnumerable<int> eventCodes);

        //IReadOnlyList<ControllerEventLog> GetAllAggregationCodes(string signalId, DateTime startTime, DateTime endTime);

        //int GetApproachEventsCountBetweenDates(int approachId, DateTime startTime, DateTime endTime, int phaseNumber);

        //int GetDetectorActivationCount(string signalId, DateTime startTime, DateTime endTime, int detectorChannel);

        //ControllerEventLog GetFirstEventBeforeDate(string signalId, int eventCode, DateTime date);

        //ControllerEventLog GetFirstEventBeforeDateByEventCodeAndParameter(string signalId, int eventCode, int eventParam, DateTime date);

        //IReadOnlyList<ControllerEventLog> GetSignalEventsByEventCode(string signalId, DateTime startTime, DateTime endTime, int eventCode);

        //IReadOnlyList<ControllerEventLog> GetSignalEventsByEventCodes(string signalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes);

        //IReadOnlyList<ControllerEventLog> GetSplitEvents(string signalId, DateTime startTime, DateTime endTime);

        //double GetTmcVolume(DateTime startDate, DateTime endDate, string signalId, int phase);

        //IReadOnlyList<ControllerEventLog> GetTopEventsAfterDateByEventCodesParam(string signalId, DateTime timestamp, IEnumerable<int> eventCodes, int param, int top);

        //IReadOnlyList<ControllerEventLog> GetTopNumberOfSignalEventsBetweenDates(string signalId, int numberOfRecords, DateTime startTime, DateTime endTime);

        #endregion

        #region Obsolete

        //[Obsolete("This method isn't currently being used")]
        //bool CheckForRecords(string signalId, DateTime startTime, DateTime endTime);

        //[Obsolete("This method isn't currently being used")]
        //int GetEventCountByEventCodesParamDateTimeRange(string signalId, DateTime startTime, DateTime endTime, int startHour, int startMinute, int endHour, int endMinute, IEnumerable<int> eventCodes, int param);

        //[Obsolete("This method isn't currently being used")]
        //IReadOnlyList<ControllerEventLog> GetEventsBetweenDates(DateTime startTime, DateTime endTime);

        //[Obsolete("This method isn't currently being used")]
        //IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamDateTimeRange(string signalId, DateTime startTime, DateTime endTime, int startHour, int startMinute, int endHour, int endMinute, IEnumerable<int> eventCodes, int param);

        //[Obsolete("Use GetEventsByEventCodesParam overload", true)]
        //IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamWithLatencyCorrection(string signalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param, double latencyCorrection);

        //[Obsolete("Use GetEventsByEventCodesParam overload", true)]
        //IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamWithOffsetAndLatencyCorrection(string signalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param, double offset, double latencyCorrection);

        //[Obsolete("Use GetSignalEventsCountBetweenDates instead", true)]
        //int GetRecordCount(string signalId, DateTime startTime, DateTime endTime);

        //[Obsolete("Depreciated, use a specification instead")]
        //DateTime GetMostRecentRecordTimestamp(string signalID);

        #endregion
    }
}
