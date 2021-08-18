using ATSPM.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATSPM.Application.Repositories
{
    public interface IControllerEventLogRepository
    {
        double GetTmcVolume(DateTime startDate, DateTime endDate, string signalId, int phase);
        IReadOnlyList<ControllerEventLog> GetSplitEvents(string signalId, DateTime startTime, DateTime endTime);

        IReadOnlyList<ControllerEventLog> GetSignalEventsByEventCode(string signalId,DateTime startTime, DateTime endTime, int eventCode);

        IReadOnlyList<ControllerEventLog> GetSignalEventsByEventCodes(string signalId,DateTime startTime, DateTime endTime, IReadOnlyList<int> eventCodes);

        IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParam(string signalId,DateTime startTime, DateTime endTime, IReadOnlyList<int> eventCodes, int param);

        IReadOnlyList<ControllerEventLog> GetTopEventsAfterDateByEventCodesParam(string signalId, DateTime timestamp,IReadOnlyList<int> eventCodes, int param, int top);

        int GetEventCountByEventCodesParamDateTimeRange(string signalId,DateTime startTime, DateTime endTime, int startHour, int startMinute, int endHour, int endMinute,IReadOnlyList<int> eventCodes, int param);

        IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamDateTimeRange(string signalId,DateTime startTime, DateTime endTime, int startHour, int startMinute, int endHour, int endMinute,IReadOnlyList<int> eventCodes, int param);

        IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamWithOffsetAndLatencyCorrection(string signalId,DateTime startTime, DateTime endTime, IReadOnlyList<int> eventCodes, int param, double offset,double latencyCorrection);

        IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamWithLatencyCorrection(string signalId,DateTime startTime, DateTime endTime, IReadOnlyList<int> eventCodes, int param,double latencyCorrection);

        ControllerEventLog GetFirstEventBeforeDate(string signalId,int eventCode, DateTime date);

        IReadOnlyList<ControllerEventLog> GetSignalEventsBetweenDates(string signalId,DateTime startTime, DateTime endTime);

        IReadOnlyList<ControllerEventLog> GetTopNumberOfSignalEventsBetweenDates(string signalId, int numberOfRecords,DateTime startTime, DateTime endTime);

        int GetDetectorActivationCount(string signalId,DateTime startTime, DateTime endTime, int detectorChannel);

        int GetRecordCount(string signalId, DateTime startTime, DateTime endTime);

        int GetRecordCountByParameterAndEvent(string signalId, DateTime startTime, DateTime endTime,IReadOnlyList<int> eventParameters, IReadOnlyList<int> events);

        IReadOnlyList<ControllerEventLog> GetRecordsByParameterAndEvent(string signalId, DateTime startTime, DateTime endTime,IReadOnlyList<int> eventParameters, IReadOnlyList<int> eventCodes);

        IReadOnlyList<ControllerEventLog> GetAllAggregationCodes(string signalId, DateTime startTime, DateTime endTime);

        ControllerEventLog GetFirstEventBeforeDateByEventCodeAndParameter(string signalId, int eventCode,int eventParam, DateTime date);

        IReadOnlyList<ControllerEventLog> GetEventsBetweenDates(DateTime startTime, DateTime endTime);

        int GetSignalEventsCountBetweenDates(string signalId, DateTime startTime, DateTime endTime);

        int GetApproachEventsCountBetweenDates(int approachId, DateTime startTime, DateTime endTime,int phaseNumber);

        DateTime GetMostRecentRecordTimestamp(string signalID);

        [Obsolete("This method isn't currently being used, Also can't use this becuase it has times not dates")]
        bool CheckForRecords(string signalId, DateTime startTime, DateTime endTime);
    }
}
