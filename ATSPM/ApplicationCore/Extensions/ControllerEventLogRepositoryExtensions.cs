using ATSPM.Application.Repositories;
using ATSPM.Application.Specifications;
using ATSPM.Data.Models;
using ATSPM.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Extensions
{
    public static class ControllerEventLogRepositoryExtensions
    {
        public static int GetSignalEventsCountBetweenDates(this IControllerEventLogRepository repo, string SignalId, DateTime startTime, DateTime endTime)
        {
            return repo.GetSignalEventsBetweenDates(SignalId, startTime, endTime).Count();
        }

        public static IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParam(this IControllerEventLogRepository repo, string SignalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param)
        {
            var result = repo.GetSignalEventsBetweenDates(SignalId, startTime, endTime)
                .FromSpecification(new ControllerLogCodeAndParamSpecification(eventCodes, param))
                .OrderBy(o => o.EventParam)
                .ToList();

            return result;
        }

        public static IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParam(this IControllerEventLogRepository repo, string SignalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param, double latencyCorrection)
        {
            var result = repo.GetEventsByEventCodesParam(SignalId, startTime, endTime, eventCodes, param);

            foreach (var item in result)
            {
                item.Timestamp = item.Timestamp.AddSeconds(0 - latencyCorrection);
            }

            return result;
        }

        public static IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParam(this IControllerEventLogRepository repo, string SignalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param, double offset, double latencyCorrection)
        {
            var result = repo.GetEventsByEventCodesParam(SignalId, startTime, endTime, eventCodes, param);

            foreach (var item in result)
            {
                item.Timestamp = item.Timestamp.AddMilliseconds(offset);
                item.Timestamp = item.Timestamp.AddSeconds(0 - latencyCorrection);
            }

            return result;
        }

        public static IReadOnlyList<ControllerEventLog> GetRecordsByParameterAndEvent(this IControllerEventLogRepository repo, string SignalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventParameters, IEnumerable<int> eventCodes)
        {
            var result = repo.GetSignalEventsBetweenDates(SignalId, startTime, endTime)
                .FromSpecification(new ControllerLogCodeAndParamSpecification(eventCodes, eventParameters))
                .OrderBy(o => o.EventParam)
                .ToList();

            return result;
        }

        public static int GetRecordCountByParameterAndEvent(this IControllerEventLogRepository repo, string SignalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventParameters, IEnumerable<int> eventCodes)
        {
            return repo.GetRecordsByParameterAndEvent(SignalId, startTime, endTime, eventParameters, eventCodes).Count;
        }

        public static IReadOnlyList<ControllerEventLog> GetAllAggregationCodes(this IControllerEventLogRepository repo, string SignalId, DateTime startTime, DateTime endTime)
        {
            var codes = new List<int> { 150, 114, 113, 112, 105, 102, 1 };

            var result = repo.GetSignalEventsBetweenDates(SignalId, startTime, endTime)
                .FromSpecification(new ControllerLogCodeAndParamSpecification(codes))
                .ToList();

            return result;
        }

        public static int GetApproachEventsCountBetweenDates(this IControllerEventLogRepository repo, int approachId, DateTime startTime, DateTime endTime, int phaseNumber)
        {
            //var approachCodes = new List<int> { 1, 8, 10 };

            ////HACK: this should come from IServiceLocator
            //IApproachRepository ar = new ApproachEFRepository(_db, (ILogger<ApproachEFRepository>)_log);
            //Approach approach = ar.Lookup(new Approach() { ApproachId = approachId });

            //return GetEventsByEventCodesParam(approach.SignalId, startTime, endTime, approachCodes, phaseNumber).Count;

            throw new NotImplementedException();
        }

        public static int GetDetectorActivationCount(this IControllerEventLogRepository repo, string SignalId, DateTime startTime, DateTime endTime, int detectorChannel)
        {
            var result = repo.GetSignalEventsBetweenDates(SignalId, startTime, endTime)
                .FromSpecification(new ControllerLogCodeAndParamSpecification(82, detectorChannel))
                .ToList().Count;

            return result;
        }

        public static ControllerEventLog GetFirstEventBeforeDate(this IControllerEventLogRepository repo, string SignalId, int eventCode, DateTime date)
        {
            throw new NotImplementedException();

            //var result = table
            //     .FromSpecification(new ControllerLogDateRangeSpecification(SignalId, date, date))
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
            //var lastEvent = _db.ControllerEventLogs.Where(c => c.SignalId == SignalId &&
            //                                                    c.Timestamp >= tempDate &&
            //                                                    c.Timestamp < date &&
            //                                                    c.EventCode == eventCode)
            //    .OrderByDescending(c => c.Timestamp).FirstOrDefault();
            //return lastEvent;
        }

        public static ControllerEventLog GetFirstEventBeforeDateByEventCodeAndParameter(this IControllerEventLogRepository repo, string SignalId, int eventCode, int eventParam, DateTime date)
        {
            throw new NotImplementedException();

            //_db.Database.CommandTimeout = 10;
            //var tempDate = date.AddDays(-1);
            //var lastEvent = _db.ControllerEventLogs.Where(c => c.SignalId == SignalId &&
            //                                                    c.Timestamp >= tempDate &&
            //                                                    c.Timestamp < date &&
            //                                                    c.EventCode == eventCode &&
            //                                                    c.EventParam == eventParam)
            //    .OrderByDescending(c => c.Timestamp).FirstOrDefault();
            //return lastEvent;
        }

        public static IReadOnlyList<ControllerEventLog> GetSignalEventsByEventCode(this IControllerEventLogRepository repo, string SignalId, DateTime startTime, DateTime endTime, int eventCode)
        {
            var result = repo.GetSignalEventsBetweenDates(SignalId, startTime, endTime)
                .FromSpecification(new ControllerLogCodeAndParamSpecification(eventCode))
                .OrderBy(o => o.EventParam)
                .ToList();

            return result;
        }

        public static IReadOnlyList<ControllerEventLog> GetSignalEventsByEventCodes(this IControllerEventLogRepository repo, string SignalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes)
        {
            var result = repo.GetSignalEventsBetweenDates(SignalId, startTime, endTime)
                .FromSpecification(new ControllerLogCodeAndParamSpecification(eventCodes))
                .OrderBy(o => o.EventParam)
                .ToList();

            return result;
        }

        public static IReadOnlyList<ControllerEventLog> GetSplitEvents(this IControllerEventLogRepository repo, string SignalId, DateTime startTime, DateTime endTime)
        {
            var result = repo.GetSignalEventsBetweenDates(SignalId, startTime, endTime)
                .Where(i => i.EventCode > 130 && i.EventCode < 150)
                .OrderBy(o => o.EventParam)
                .ToList();

            return result;
        }

        public static double GetTmcVolume(this IControllerEventLogRepository repo, DateTime startDate, DateTime endDate, string SignalId, int phase)
        {
            throw new NotImplementedException();

            //HACK: this should come from IServiceLocator
            //ISignalRepository sr = new SignalEFRepository(db, (ILogger<SignalEFRepository>)log);
            //Signal signal = sr.GetVersionOfSignalByDate(SignalId, startDate);
            //var graphDetectors = signal.GetDetectorsForSignalByPhaseNumber(phase);



            //var tmcChannels = new List<int>();
            //foreach (var gd in graphDetectors)
            //    foreach (var dt in gd.DetectionTypes)
            //        if (dt.DetectionTypeID == 4)
            //            tmcChannels.Add(gd.DetChannel);


            //double count = (from cel in _db.ControllerEventLogs
            //                where cel.Timestamp >= startDate
            //                      && cel.Timestamp < endDate
            //                      && cel.SignalId == SignalId
            //                      && tmcChannels.Contains(cel.EventParam)
            //                      && cel.EventCode == 82
            //                select cel).Count();

            //return count;
        }

        public static IReadOnlyList<ControllerEventLog> GetTopEventsAfterDateByEventCodesParam(this IControllerEventLogRepository repo, string SignalId, DateTime timestamp, IEnumerable<int> eventCodes, int param, int top)
        {
            throw new NotImplementedException();

            //var endDate = timestamp.AddHours(1);
            //var events = _db.ControllerEventLogs.Where(c =>
            //    c.SignalId == SignalId &&
            //    c.Timestamp > timestamp &&
            //    c.Timestamp < endDate &&
            //    c.EventParam == param &&
            //    eventCodes.Contains(c.EventCode)).ToList();
            //return events
            //    .OrderBy(s => s.Timestamp)
            //    .Take(top).ToList();
        }

        public static IReadOnlyList<ControllerEventLog> GetTopNumberOfSignalEventsBetweenDates(this IControllerEventLogRepository repo, string SignalId, int numberOfRecords, DateTime startTime, DateTime endTime)
        {
            throw new NotImplementedException();

            //var events =
            //    (from r in _db.ControllerEventLogs
            //     where r.SignalId == SignalId
            //           && r.Timestamp >= startTime
            //           && r.Timestamp < endTime
            //     select r).Take(numberOfRecords).ToList();

            //if (events != null)
            //    return events;
            //var emptyEvents = new List<ControllerEventLog>();
            //return emptyEvents;
        }

        #region Obsolete

        [Obsolete("User GetEventsByEventCodesParam overload", true)]
        public static IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamWithLatencyCorrection(this IControllerEventLogRepository repo, string SignalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param, double latencyCorrection)
        {
            throw new NotImplementedException();
        }

        [Obsolete("User GetEventsByEventCodesParam overload", true)]
        public static IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamWithOffsetAndLatencyCorrection(this IControllerEventLogRepository repo, string SignalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param, double offset, double latencyCorrection)
        {
            throw new NotImplementedException();
        }

        [Obsolete("This method isn't currently being used")]
        public static bool CheckForRecords(this IControllerEventLogRepository repo, string SignalId, DateTime startTime, DateTime endTime)
        {
            throw new NotImplementedException();

            //return _db.ControllerEventLogs.Any(r => r.SignalId == SignalId
            //                                             && r.Timestamp >= startTime
            //                                             && r.Timestamp < endTime);
        }

        [Obsolete("This method isn't currently being used")]
        public static int GetEventCountByEventCodesParamDateTimeRange(this IControllerEventLogRepository repo, string SignalId, DateTime startTime, DateTime endTime, int startHour, int startMinute, int endHour, int endMinute, IEnumerable<int> eventCodes, int param)
        {
            return repo.GetEventsByEventCodesParamDateTimeRange(SignalId, startTime, endTime, startHour, startMinute, endHour, endMinute, eventCodes, param).Count;
        }

        [Obsolete("This method isn't currently being used")]
        public static IReadOnlyList<ControllerEventLog> GetEventsBetweenDates(this IControllerEventLogRepository repo, DateTime startTime, DateTime endTime)
        {
            return repo.GetSignalEventsBetweenDates(null, startTime, endTime).ToList();
        }

        [Obsolete("This method isn't currently being used")]
        public static IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamDateTimeRange(this IControllerEventLogRepository repo, string SignalId, DateTime startTime, DateTime endTime, int startHour, int startMinute, int endHour, int endMinute, IEnumerable<int> eventCodes, int param)
        {
            var result = repo.GetSignalEventsBetweenDates(SignalId, startTime, endTime)
                .FromSpecification(new ControllerLogCodeAndParamSpecification(eventCodes, param))
                .FromSpecification(new ControllerLogDateTimeRangeSpecification(startHour, startMinute, endHour, endMinute))
                .OrderBy(o => o.EventParam)
                .ToList();

            return result;
        }

        [Obsolete("Use GetSignalEventsCountBetweenDates instead", true)]
        public static int GetRecordCount(this IControllerEventLogRepository repo, string SignalId, DateTime startTime, DateTime endTime)
        {
            return repo.GetSignalEventsBetweenDates(SignalId, startTime, endTime).ToList().Count;
        }

        [Obsolete("Depreciated, use a specification instead")]
        public static DateTime GetMostRecentRecordTimestamp(this IControllerEventLogRepository repo, string SignalId)
        {
            throw new NotImplementedException();
        }

        #endregion


    }
}
