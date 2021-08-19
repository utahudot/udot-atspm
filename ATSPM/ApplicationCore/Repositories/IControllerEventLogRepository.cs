using ATSPM.Application.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATSPM.Application.Repositories
{
    public interface IControllerEventLogRepository : IAsyncRepository<ControllerLogArchive>
    {
        [Obsolete("This method isn't currently being used")]
        bool CheckForRecords(string signalId, DateTime startTime, DateTime endTime);

        IReadOnlyList<ControllerEventLog> GetAllAggregationCodes(string signalId, DateTime startTime, DateTime endTime);

        int GetApproachEventsCountBetweenDates(int approachId, DateTime startTime, DateTime endTime, int phaseNumber);

        int GetDetectorActivationCount(string signalId, DateTime startTime, DateTime endTime, int detectorChannel);

        [Obsolete("This method isn't currently being used")]
        int GetEventCountByEventCodesParamDateTimeRange(string signalId, DateTime startTime, DateTime endTime, int startHour, int startMinute, int endHour, int endMinute, IEnumerable<int> eventCodes, int param);

        [Obsolete("This method isn't currently being used")]
        IReadOnlyList<ControllerEventLog> GetEventsBetweenDates(DateTime startTime, DateTime endTime);

        //Christian added overloads to this
        IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParam(string signalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param);
        IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParam(string signalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param, double latencyCorrection);
        IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParam(string signalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param, double offset, double latencyCorrection);

        [Obsolete("This method isn't currently being used")]
        IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamDateTimeRange(string signalId, DateTime startTime, DateTime endTime, int startHour, int startMinute, int endHour, int endMinute, IEnumerable<int> eventCodes, int param);

        [Obsolete("User GetEventsByEventCodesParam overload", true)]
        IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamWithLatencyCorrection(string signalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param, double latencyCorrection);

        [Obsolete("User GetEventsByEventCodesParam overload", true)]
        IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamWithOffsetAndLatencyCorrection(string signalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param, double offset, double latencyCorrection);

        ControllerEventLog GetFirstEventBeforeDate(string signalId, int eventCode, DateTime date);

        ControllerEventLog GetFirstEventBeforeDateByEventCodeAndParameter(string signalId, int eventCode, int eventParam, DateTime date);

        [Obsolete("Depreciated, use a specification instead")]
        DateTime GetMostRecentRecordTimestamp(string signalID);

        [Obsolete("This is redundant with GetSignalEventsCountBetweenDates")]
        int GetRecordCount(string signalId, DateTime startTime, DateTime endTime);

        int GetRecordCountByParameterAndEvent(string signalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventParameters, IEnumerable<int> eventCodes);

        IReadOnlyList<ControllerEventLog> GetRecordsByParameterAndEvent(string signalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventParameters, IEnumerable<int> eventCodes);

        [Obsolete("This method isn't currently being used")]
        IReadOnlyList<ControllerEventLog> GetSignalEventsBetweenDates(string signalId, DateTime startTime, DateTime endTime);

        IReadOnlyList<ControllerEventLog> GetSignalEventsByEventCode(string signalId, DateTime startTime, DateTime endTime, int eventCode);

        IReadOnlyList<ControllerEventLog> GetSignalEventsByEventCodes(string signalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes);

        [Obsolete("This is redundant with GetRecordCount")]
        int GetSignalEventsCountBetweenDates(string signalId, DateTime startTime, DateTime endTime);

        IReadOnlyList<ControllerEventLog> GetSplitEvents(string signalId, DateTime startTime, DateTime endTime);

        double GetTmcVolume(DateTime startDate, DateTime endDate, string signalId, int phase);

        IReadOnlyList<ControllerEventLog> GetTopEventsAfterDateByEventCodesParam(string signalId, DateTime timestamp, IEnumerable<int> eventCodes, int param, int top);

        IReadOnlyList<ControllerEventLog> GetTopNumberOfSignalEventsBetweenDates(string signalId, int numberOfRecords, DateTime startTime, DateTime endTime);
    }
}
