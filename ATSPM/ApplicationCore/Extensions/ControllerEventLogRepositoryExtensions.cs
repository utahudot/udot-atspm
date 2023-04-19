using ATSPM.Application.Repositories;
using ATSPM.Application.Specifications;
using ATSPM.Data.Models;
using ATSPM.Domain.Extensions;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Channels;

namespace ATSPM.Application.Extensions
{
    public static class ControllerEventLogRepositoryExtensions
    {
        public static IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParam(
            this IControllerEventLogRepository repo,
            string signalId,
            DateTime startTime,
            DateTime endTime,
            IEnumerable<int> eventCodes,
            int param)
        {
            var result = repo.GetSignalEventsBetweenDates(signalId, startTime, endTime)
                .FromSpecification(new ControllerLogCodeAndParamSpecification(eventCodes, param))

                //this orderby was in the original version but it's always the same param so ordering is pointless
                //.OrderBy(o => o.EventCode)
                
                .ToList();

            return result;
        }

        public static IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParam(
            this IControllerEventLogRepository repo,
            string signalId,
            DateTime startTime,
            DateTime endTime,
            IEnumerable<int> eventCodes,
            int param,
            double latencyCorrection)
        {
            var result = repo.GetEventsByEventCodesParam(signalId, startTime, endTime, eventCodes, param);

            foreach (var item in result)
            {
                item.Timestamp = item.Timestamp.AddSeconds(0 - latencyCorrection);
            }

            return result;
        }

        

        public static IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParam(
            this IControllerEventLogRepository repo,
            string signalId,
            DateTime startTime,
            DateTime endTime,
            IEnumerable<int> eventCodes,
            int param,
            double offset,
            double latencyCorrection)
        {
            var result = repo.GetEventsByEventCodesParam(signalId, startTime, endTime, eventCodes, param);

            foreach (var item in result)
            {
                item.Timestamp = item.Timestamp.AddMilliseconds(offset);
                item.Timestamp = item.Timestamp.AddSeconds(0 - latencyCorrection);
            }

            return result;
        }

        public static IReadOnlyList<ControllerEventLog> GetRecordsByParameterAndEvent(
            this IControllerEventLogRepository repo,
            string signalId,
            DateTime startTime,
            DateTime endTime,
            IEnumerable<int> eventParameters,
            IEnumerable<int> eventCodes)
        {
            var result = repo.GetSignalEventsBetweenDates(signalId, startTime, endTime)
                .FromSpecification(new ControllerLogCodeAndParamSpecification(eventCodes, eventParameters))
                .OrderBy(o => o.EventParam)
                .ToList();

            return result;
        }

        public static int GetRecordCountByParameterAndEvent(
            this IControllerEventLogRepository repo,
            string signalId,
            DateTime startTime,
            DateTime endTime,
            IEnumerable<int> eventParameters,
            IEnumerable<int> eventCodes)
        {
            return repo.GetRecordsByParameterAndEvent(signalId, startTime, endTime, eventParameters, eventCodes).Count;
        }

        public static IReadOnlyList<ControllerEventLog> GetAllAggregationCodes(
            this IControllerEventLogRepository repo,
            string signalId,
            DateTime startTime,
            DateTime endTime)
        {
            var codes = new List<int> { 150, 114, 113, 112, 105, 102, 1 };

            var result = repo.GetSignalEventsBetweenDates(signalId, startTime, endTime)
                .FromSpecification(new ControllerLogCodeAndParamSpecification(codes))
                .ToList();

            return result;
        }

        public static int GetApproachEventsCountBetweenDates(
            this IControllerEventLogRepository repo,
            int approachId,
            DateTime startTime,
            DateTime endTime,
            int phaseNumber)
        {
            //var approachCodes = new List<int> { 1, 8, 10 };

            ////HACK: this should come from IServiceLocator
            //IApproachRepository ar = new ApproachEFRepository(_db, (ILogger<ApproachEFRepository>)_log);
            //Approach approach = ar.Lookup(new Approach() { ApproachId = approachId });

            //return GetEventsByEventCodesParam(approach.signalId, startTime, endTime, approachCodes, phaseNumber).Count;

            throw new NotImplementedException();
        }

        public static int GetDetectorActivationCount(
            this IControllerEventLogRepository repo,
            string signalId,
            DateTime startTime,
            DateTime endTime,
            int detectorChannel)
        {
            var result = repo.GetSignalEventsBetweenDates(signalId, startTime, endTime)
                .FromSpecification(new ControllerLogCodeAndParamSpecification(82, detectorChannel))
                .ToList().Count;

            return result;
        }

        public static IReadOnlyList<ControllerEventLog> GetDetectorEvents(
            this IControllerEventLogRepository repo,
            int metricTypeId,
            Approach approach,
            DateTime start,
            DateTime end,
            bool detectorOn,
            bool detectorOff)
        {
            var eventCodes = new List<int>();
            if(detectorOn)
                eventCodes.Add(82);
            if(detectorOff)
                eventCodes.Add(81);
            if (!detectorOn && !detectorOff)
                throw new ArgumentException("At least one detector event code must be true (detectorOn or detectorOff");
            var events = new List<ControllerEventLog>();
            var detectorsForMetric = approach.GetDetectorsForMetricType(metricTypeId);
            foreach (var d in detectorsForMetric)
                events.AddRange(repo.GetEventsByEventCodesParam(
                    approach.Signal.SignalId,
                    start,
                    end,
                    eventCodes,
                    d.DetChannel,
                    d.GetOffset(),
                    d.LatencyCorrection));
            return events.OrderBy(e => e.Timestamp).ToList();
        }

        public static IReadOnlyList<ControllerEventLog> GetPlanEvents(
           this IControllerEventLogRepository repo,
           string signalId,
           DateTime start,
           DateTime end)
        {
            var events = repo.GetSignalEventsByEventCode(
                signalId,
                start.AddHours(-12),
                end.AddHours(12),
                131)
                .OrderBy(e => e.Timestamp)
                .ToList();

            var uniqueEvents = new List<ControllerEventLog>();
            // Iterate over the original events list
            for (int i = 0; i < events.Count; i++)
            {
                // Check if the current event has the same EventParam value as the previous event
                if (i == 0 || events[i].EventParam != events[i - 1].EventParam)
                {
                    // If not, add it to the uniqueEvents list
                    uniqueEvents.Add(events[i]);
                }
            }
            UpdateEventsBeforeDateForPlans(uniqueEvents, start);
            UpdateEventsAfterDateForPlans(uniqueEvents, end);
            // Return the uniqueEvents list
            return uniqueEvents; ;
        }

        public static void UpdateEventsAfterDateForPlans(List<ControllerEventLog> events, DateTime date)
        {
            // Find the first event that occurred after the specified date
            var index = events.FindIndex(e => e.Timestamp > date);

            if (index >= 0)
            {
                // If an event was found, remove all events after it
                events.RemoveRange(index, events.Count - index);

                // Change the timestamp of the found event to match the specified date
                events[index].Timestamp = date;
            }
            else
            {
                // If no event was found, create a new event with event param 0, event code 131, and the specified date as the timestamp
                var newEvent = new ControllerEventLog
                {
                    SignalId = "0",
                    Timestamp = date,
                    EventCode = 131,
                    EventParam = 0
                };

                // Add the new event to the end of the list
                events.Add(newEvent);
            }
        }


        public static void UpdateEventsBeforeDateForPlans(List<ControllerEventLog> events, DateTime date)
        {
            // Find the first event that occurred before the specified date
            var index = events.FindIndex(e => e.Timestamp < date);

            if (index >= 0)
            {
                // If an event was found, change its timestamp to match the specified date
                events[index].Timestamp = date;

                // Remove all events before the found event
                events.RemoveRange(0, index);
            }
            else
            {
                // If no event was found, create a new event with event param 0, event code 131, and the specified date as the timestamp
                var newEvent = new ControllerEventLog
                {
                    SignalId = "0",
                    Timestamp = date,
                    EventCode = 131,
                    EventParam = 0
                };

                // Add the new event to the beginning of the list
                events.Insert(0, newEvent);
            }
        }


        public static IReadOnlyList<ControllerEventLog> GetCycleEventsWithTimeExtension(
           this IControllerEventLogRepository repo,
           Approach approach,
           bool getPermissivePhase,
           DateTime start,
           DateTime end)
        {
            return repo.GetEventsByEventCodesParam(
                approach.SignalId,
                start.AddSeconds(-900),
                end.AddSeconds(900),
                approach.GetCycleEventCodes(getPermissivePhase),
                getPermissivePhase ? approach.PermissivePhaseNumber.Value : approach.ProtectedPhaseNumber).OrderBy(e => e.Timestamp).ToList();
        }



        public static ControllerEventLog GetFirstEventBeforeDate(this IControllerEventLogRepository repo, string signalId, int eventCode, DateTime date)
        {
            throw new NotImplementedException();

            //var result = table
            //     .FromSpecification(new ControllerLogDateRangeSpecification(signalId, date, date))
            //     .AsNoTracking()
            //     .AsEnumerable()
            //     .SelectMany(s => s.LogData)
            //     .AsQueryable()
            //    .FromSpecification(new ControllerLogCodeAndParamSpecification(eventCode, param))
            //    .OrderBy(o => o.EventParam)
            //    .ToList();

            //use GetSignalEventsByEventCode() here!!!

            //return result;


            //var tempDate = date.AddHours(-1);
            //var lastEvent = _db.ControllerEventLogs.Where(c => c.signalId == signalId &&
            //                                                    c.Timestamp >= tempDate &&
            //                                                    c.Timestamp < date &&
            //                                                    c.EventCode == eventCode)
            //    .OrderByDescending(c => c.Timestamp).FirstOrDefault();
            //return lastEvent;
        }

        public static ControllerEventLog GetFirstEventBeforeDateByEventCodeAndParameter(
            this IControllerEventLogRepository repo,
            string signalId,
            int eventCode,
            int eventParam,
            DateTime date)
        {
            throw new NotImplementedException();

            //_db.Database.CommandTimeout = 10;
            //var tempDate = date.AddDays(-1);
            //var lastEvent = _db.ControllerEventLogs.Where(c => c.signalId == signalId &&
            //                                                    c.Timestamp >= tempDate &&
            //                                                    c.Timestamp < date &&
            //                                                    c.EventCode == eventCode &&
            //                                                    c.EventParam == eventParam)
            //    .OrderByDescending(c => c.Timestamp).FirstOrDefault();
            //return lastEvent;
        }

        public static IReadOnlyList<ControllerEventLog> GetSignalEventsByEventCode(
            this IControllerEventLogRepository repo,
            string signalId,
            DateTime startTime,
            DateTime endTime,
            int eventCode)
        {
            var result = repo.GetSignalEventsBetweenDates(signalId, startTime, endTime)
                .FromSpecification(new ControllerLogCodeAndParamSpecification(eventCode))
                .OrderBy(o => o.EventParam)
                .ToList();

            return result;
        }

        public static IReadOnlyList<ControllerEventLog> GetSignalEventsByEventCodes(
            this IControllerEventLogRepository repo,
            string signalId,
            DateTime startTime,
            DateTime endTime,
            IEnumerable<int> eventCodes)
        {
            var result = repo.GetSignalEventsBetweenDates(signalId, startTime, endTime)
                .FromSpecification(new ControllerLogCodeAndParamSpecification(eventCodes))
                .OrderBy(o => o.EventParam)
                .ToList();

            return result;
        }

        public static IReadOnlyList<ControllerEventLog> GetSplitEvents(
            this IControllerEventLogRepository repo,
            string signalId,
            DateTime startTime,
            DateTime endTime)
        {
            var result = repo.GetSignalEventsBetweenDates(signalId, startTime, endTime)
                .Where(i => i.EventCode > 130 && i.EventCode < 150)
                .OrderBy(o => o.EventParam)
                .ToList();

            return result;
        }

        public static double GetTmcVolume(
            this IControllerEventLogRepository repo,
            DateTime startDate,
            DateTime endDate,
            string signalId,
            int phase)
        {
            throw new NotImplementedException();

            //HACK: this should come from IServiceLocator
            //ISignalRepository sr = new SignalEFRepository(db, (ILogger<SignalEFRepository>)log);
            //Signal signal = sr.GetVersionOfSignalByDate(signalId, startDate);
            //var graphDetectors = signal.GetDetectorsForSignalByPhaseNumber(phase);



            //var tmcChannels = new List<int>();
            //foreach (var gd in graphDetectors)
            //    foreach (var dt in gd.DetectionTypes)
            //        if (dt.DetectionTypeID == 4)
            //            tmcChannels.Add(gd.DetChannel);


            //double count = (from cel in _db.ControllerEventLogs
            //                where cel.Timestamp >= startDate
            //                      && cel.Timestamp < endDate
            //                      && cel.signalId == signalId
            //                      && tmcChannels.Contains(cel.EventParam)
            //                      && cel.EventCode == 82
            //                select cel).Count();

            //return count;
        }

        public static IReadOnlyList<ControllerEventLog> GetTopEventsAfterDateByEventCodesParam(this IControllerEventLogRepository repo, string signalId, DateTime timestamp, IEnumerable<int> eventCodes, int param, int top)
        {
            throw new NotImplementedException();

            //var endDate = timestamp.AddHours(1);
            //var events = _db.ControllerEventLogs.Where(c =>
            //    c.signalId == signalId &&
            //    c.Timestamp > timestamp &&
            //    c.Timestamp < endDate &&
            //    c.EventParam == param &&
            //    eventCodes.Contains(c.EventCode)).ToList();
            //return events
            //    .OrderBy(s => s.Timestamp)
            //    .Take(top).ToList();
        }

        public static IReadOnlyList<ControllerEventLog> GetTopEventsBeforeDateByEventCodesParam(this IControllerEventLogRepository repo, string signalId,
           DateTime timestamp, List<int> eventCodes, int param, int top)
        {
            throw new NotImplementedException();
            //var settings = _db.GeneralSettings.FirstOrDefault();
            //var secondsToCompleteCycle = 900;
            //if (settings != null)
            //    secondsToCompleteCycle = Convert.ToInt32(settings.CycleCompletionSeconds);
            //try
            //{
            //    var start = timestamp.AddSeconds(secondsToCompleteCycle * -1);
            //    var events = _db.Controller_Event_Log.Where(c =>
            //        c.SignalID == signalId &&
            //        c.Timestamp < timestamp &&
            //        c.Timestamp > start &&
            //        c.EventParam == param &&
            //        eventCodes.Contains(c.EventCode)).ToList();
            //    return events
            //        .OrderByDescending(s => s.Timestamp)
            //        .Take(top).ToList();
            //}
            //catch (Exception e)
            //{
            //    var errorLog = ApplicationEventRepositoryFactory.Create();
            //    errorLog.QuickAdd(Assembly.GetExecutingAssembly().FullName,
            //        GetType().Name, e.TargetSite.ToString(), ApplicationEvent.SeverityLevels.Low, e.Message);
            //    return null;
            //}
        }

        public static IReadOnlyList<ControllerEventLog> GetTopNumberOfSignalEventsBetweenDates(this IControllerEventLogRepository repo, string signalId, int numberOfRecords, DateTime startTime, DateTime endTime)
        {
            throw new NotImplementedException();

            //var events =
            //    (from r in _db.ControllerEventLogs
            //     where r.signalId == signalId
            //           && r.Timestamp >= startTime
            //           && r.Timestamp < endTime
            //     select r).Take(numberOfRecords).ToList();

            //if (events != null)
            //    return events;
            //var emptyEvents = new List<ControllerEventLog>();
            //return emptyEvents;
        }

        #region Obsolete

        [Obsolete("Use GetSignalEventsBetweenDates(signalId, startTime, endTime).Count()", true)]
        public static int GetSignalEventsCountBetweenDates(this IControllerEventLogRepository repo, string signalId, DateTime startTime, DateTime endTime)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use GetEventsByEventCodesParam overload", true)]
        public static IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamWithLatencyCorrection(this IControllerEventLogRepository repo, string signalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param, double latencyCorrection)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use GetEventsByEventCodesParam overload", true)]
        public static IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamWithOffsetAndLatencyCorrection(this IControllerEventLogRepository repo, string signalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param, double offset, double latencyCorrection)
        {
            throw new NotImplementedException();
        }

        [Obsolete("This method isn't currently being used")]
        public static bool CheckForRecords(this IControllerEventLogRepository repo, string signalId, DateTime startTime, DateTime endTime)
        {
            throw new NotImplementedException();
        }

        [Obsolete("This method isn't currently being used")]
        public static int GetEventCountByEventCodesParamDateTimeRange(this IControllerEventLogRepository repo, string signalId, DateTime startTime, DateTime endTime, int startHour, int startMinute, int endHour, int endMinute, IEnumerable<int> eventCodes, int param)
        {
            throw new NotImplementedException();
        }

        [Obsolete("This method isn't currently being used")]
        public static IReadOnlyList<ControllerEventLog> GetEventsBetweenDates(this IControllerEventLogRepository repo, DateTime startTime, DateTime endTime)
        {
            return repo.GetSignalEventsBetweenDates(null, startTime, endTime).ToList();
        }

        [Obsolete("This method isn't currently being used")]
        public static IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamDateTimeRange(this IControllerEventLogRepository repo, string signalId, DateTime startTime, DateTime endTime, int startHour, int startMinute, int endHour, int endMinute, IEnumerable<int> eventCodes, int param)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use GetSignalEventsCountBetweenDates instead", true)]
        public static int GetRecordCount(this IControllerEventLogRepository repo, string signalId, DateTime startTime, DateTime endTime)
        {
            return repo.GetSignalEventsBetweenDates(signalId, startTime, endTime).ToList().Count;
        }

        [Obsolete("Depreciated, use a specification instead")]
        public static DateTime GetMostRecentRecordTimestamp(this IControllerEventLogRepository repo, string signalId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
