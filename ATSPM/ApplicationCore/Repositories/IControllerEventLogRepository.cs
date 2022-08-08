using ATSPM.Data.Models;
using ATSPM.Application.ValueObjects;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATSPM.Application.Repositories
{
    public interface IControllerEventLogRepository : IAsyncRepository<ControllerLogArchive>
    {
        IQueryable<ControllerEventLog> GetSignalEventsBetweenDates(string SignalID, DateTime startTime, DateTime endTime);

        #region ExtensionMethods

        //int GetSignalEventsCountBetweenDates(string SignalID, DateTime startTime, DateTime endTime);

        //IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParam(string SignalID, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param);

        //IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParam(string SignalID, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param, double latencyCorrection);

        //IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParam(string SignalID, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param, double offset, double latencyCorrection);

        //IReadOnlyList<ControllerEventLog> GetRecordsByParameterAndEvent(string SignalID, DateTime startTime, DateTime endTime, IEnumerable<int> eventParameters, IEnumerable<int> eventCodes);

        //int GetRecordCountByParameterAndEvent(string SignalID, DateTime startTime, DateTime endTime, IEnumerable<int> eventParameters, IEnumerable<int> eventCodes);

        //IReadOnlyList<ControllerEventLog> GetAllAggregationCodes(string SignalID, DateTime startTime, DateTime endTime);

        //int GetApproachEventsCountBetweenDates(int approachId, DateTime startTime, DateTime endTime, int phaseNumber);

        //int GetDetectorActivationCount(string SignalID, DateTime startTime, DateTime endTime, int detectorChannel);

        //ControllerEventLog GetFirstEventBeforeDate(string SignalID, int eventCode, DateTime date);

        //ControllerEventLog GetFirstEventBeforeDateByEventCodeAndParameter(string SignalID, int eventCode, int eventParam, DateTime date);

        //IReadOnlyList<ControllerEventLog> GetSignalEventsByEventCode(string SignalID, DateTime startTime, DateTime endTime, int eventCode);

        //IReadOnlyList<ControllerEventLog> GetSignalEventsByEventCodes(string SignalID, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes);

        //IReadOnlyList<ControllerEventLog> GetSplitEvents(string SignalID, DateTime startTime, DateTime endTime);

        //double GetTmcVolume(DateTime startDate, DateTime endDate, string SignalID, int phase);

        //IReadOnlyList<ControllerEventLog> GetTopEventsAfterDateByEventCodesParam(string SignalID, DateTime timestamp, IEnumerable<int> eventCodes, int param, int top);

        //IReadOnlyList<ControllerEventLog> GetTopNumberOfSignalEventsBetweenDates(string SignalID, int numberOfRecords, DateTime startTime, DateTime endTime);

        #endregion

        #region Obsolete

        //[Obsolete("This method isn't currently being used")]
        //bool CheckForRecords(string SignalID, DateTime startTime, DateTime endTime);

        //[Obsolete("This method isn't currently being used")]
        //int GetEventCountByEventCodesParamDateTimeRange(string SignalID, DateTime startTime, DateTime endTime, int startHour, int startMinute, int endHour, int endMinute, IEnumerable<int> eventCodes, int param);

        //[Obsolete("This method isn't currently being used")]
        //IReadOnlyList<ControllerEventLog> GetEventsBetweenDates(DateTime startTime, DateTime endTime);

        //[Obsolete("This method isn't currently being used")]
        //IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamDateTimeRange(string SignalID, DateTime startTime, DateTime endTime, int startHour, int startMinute, int endHour, int endMinute, IEnumerable<int> eventCodes, int param);

        //[Obsolete("Use GetEventsByEventCodesParam overload", true)]
        //IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamWithLatencyCorrection(string SignalID, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param, double latencyCorrection);

        //[Obsolete("Use GetEventsByEventCodesParam overload", true)]
        //IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamWithOffsetAndLatencyCorrection(string SignalID, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param, double offset, double latencyCorrection);

        //[Obsolete("Use GetSignalEventsCountBetweenDates instead", true)]
        //int GetRecordCount(string SignalID, DateTime startTime, DateTime endTime);

        //[Obsolete("Depreciated, use a specification instead")]
        //DateTime GetMostRecentRecordTimestamp(string SignalID);

        #endregion
    }
}
