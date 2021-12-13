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
using ATSPM.Application.Specifications;
using ATSPM.Domain.Services;
using ATSPM.Domain.Specifications;
using System.IO;

namespace ATSPM.Infrasturcture.Repositories
{
    public class ControllerEventLogParquetRepository : ATSPMRepositoryParquetBase<ControllerLogArchive>, IControllerEventLogRepository
    {
        public ControllerEventLogParquetRepository(DbContext db, ILogger<ControllerEventLogParquetRepository> log) : base(db, log) { }

        protected override string GenerateFileName(ControllerLogArchive item)
        {
            var folder = new DirectoryInfo(Path.Combine("C:", "ControlLogs", $"{item.ArchiveDate.Year}", $"{item.ArchiveDate.Month}", $"{item.ArchiveDate.Day}"));

            if (!folder.Exists)
                folder.Create();

            return Path.Combine(folder.FullName, $"{_db.CreateKeyValueName(item)}.json");
        }

        private IQueryable<ControllerEventLog> GetFromDateRange(string signalId, DateTime startTime, DateTime endTime)
        {
            var result = table
                .FromSpecification(new ControllerLogDateRangeSpecification(signalId, startTime, endTime))
                .AsNoTracking()
                .AsEnumerable()
                .SelectMany(s => s.LogData)
                .AsQueryable();

            return result;
        }















        [Obsolete("This method isn't currently being used")]
        public bool CheckForRecords(string signalId, DateTime startTime, DateTime endTime)
        {
            throw new NotImplementedException();

            //return _db.ControllerEventLogs.Any(r => r.SignalID == signalId
            //                                             && r.Timestamp >= startTime
            //                                             && r.Timestamp < endTime);
        }

        public IReadOnlyList<ControllerEventLog> GetAllAggregationCodes(string signalId, DateTime startTime, DateTime endTime)
        {
            var codes = new List<int> { 150, 114, 113, 112, 105, 102, 1 };

            var result = GetFromDateRange(signalId, startTime, endTime)
                .FromSpecification(new ControllerLogCodeAndParamSpecification(codes))
                .ToList();

            return result;
        }

        public int GetApproachEventsCountBetweenDates(int approachId, DateTime startTime, DateTime endTime, int phaseNumber)
        {
            var approachCodes = new List<int> { 1, 8, 10 };

            //HACK: this should come from IServiceLocator
            IApproachRepository ar = new ApproachEFRepository(_db, (ILogger<ApproachEFRepository>)_log);
            Approach approach = ar.Lookup(new Approach() { ApproachId = approachId });

            return GetEventsByEventCodesParam(approach.SignalId, startTime, endTime, approachCodes, phaseNumber).Count;
        }

        public int GetDetectorActivationCount(string signalId, DateTime startTime, DateTime endTime, int detectorChannel)
        {
            var result = GetFromDateRange(signalId, startTime, endTime)
                .FromSpecification(new ControllerLogCodeAndParamSpecification(82, detectorChannel))
                .ToList().Count;

            return result;
        }

        [Obsolete("This method isn't currently being used")]
        public int GetEventCountByEventCodesParamDateTimeRange(string signalId, DateTime startTime, DateTime endTime, int startHour, int startMinute, int endHour, int endMinute, IEnumerable<int> eventCodes, int param)
        {
            return GetEventsByEventCodesParamDateTimeRange(signalId, startTime, endTime, startHour, startMinute, endHour, endMinute, eventCodes, param).Count;
        }

        [Obsolete("This method isn't currently being used")]
        public IReadOnlyList<ControllerEventLog> GetEventsBetweenDates(DateTime startTime, DateTime endTime)
        {
            return GetFromDateRange(null, startTime, endTime).ToList();
        }






        public IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParam(string signalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param)
        {
            var result = GetFromDateRange(signalId, startTime, endTime)
                .FromSpecification(new ControllerLogCodeAndParamSpecification(eventCodes, param))
                .OrderBy(o => o.EventParam)
                .ToList();

            return result;
        }

        public IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParam(string signalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param, double latencyCorrection)
        {
            var result = GetEventsByEventCodesParam(signalId, startTime, endTime, eventCodes, param);

            foreach (var item in result)
            {
                item.Timestamp = item.Timestamp.AddSeconds(0 - latencyCorrection);
            }

            return result;
        }

        public IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParam(string signalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param, double offset, double latencyCorrection)
        {
            var result = GetEventsByEventCodesParam(signalId, startTime, endTime, eventCodes, param);

            foreach (var item in result)
            {
                item.Timestamp = item.Timestamp.AddMilliseconds(offset);
                item.Timestamp = item.Timestamp.AddSeconds(0 - latencyCorrection);
            }

            return result;
        }






        [Obsolete("This method isn't currently being used")]
        public IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamDateTimeRange(string signalId, DateTime startTime, DateTime endTime, int startHour, int startMinute, int endHour, int endMinute, IEnumerable<int> eventCodes, int param)
        {
            var result = GetFromDateRange(signalId, startTime, endTime)
                .FromSpecification(new ControllerLogCodeAndParamSpecification(eventCodes, param))
                .FromSpecification(new ControllerLogDateTimeRangeSpecification(startHour, startMinute, endHour, endMinute))
                .OrderBy(o => o.EventParam)
                .ToList();

            return result;
        }


        [Obsolete("User GetEventsByEventCodesParam overload", true)]
        public IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamWithLatencyCorrection(string signalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param, double latencyCorrection) 
        {
            throw new NotImplementedException();
        }

        [Obsolete("User GetEventsByEventCodesParam overload", true)]
        public IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamWithOffsetAndLatencyCorrection(string signalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes, int param, double offset, double latencyCorrection)
        {
            throw new NotImplementedException();
        }



        public ControllerEventLog GetFirstEventBeforeDate(string signalId, int eventCode, DateTime date)
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
            //var lastEvent = _db.ControllerEventLogs.Where(c => c.SignalID == signalId &&
            //                                                    c.Timestamp >= tempDate &&
            //                                                    c.Timestamp < date &&
            //                                                    c.EventCode == eventCode)
            //    .OrderByDescending(c => c.Timestamp).FirstOrDefault();
            //return lastEvent;
        }

        public ControllerEventLog GetFirstEventBeforeDateByEventCodeAndParameter(string signalId, int eventCode, int eventParam, DateTime date)
        {
            throw new NotImplementedException();

            //_db.Database.CommandTimeout = 10;
            //var tempDate = date.AddDays(-1);
            //var lastEvent = _db.ControllerEventLogs.Where(c => c.SignalID == signalId &&
            //                                                    c.Timestamp >= tempDate &&
            //                                                    c.Timestamp < date &&
            //                                                    c.EventCode == eventCode &&
            //                                                    c.EventParam == eventParam)
            //    .OrderByDescending(c => c.Timestamp).FirstOrDefault();
            //return lastEvent;
        }

        [Obsolete("Depreciated, use a specification instead")]
        public DateTime GetMostRecentRecordTimestamp(string signalID)
        {
            throw new NotImplementedException();
        }

        [Obsolete("This is redundant with GetSignalEventsCountBetweenDates")]
        public int GetRecordCount(string signalId, DateTime startTime, DateTime endTime)
        {
            return GetFromDateRange(signalId, startTime, endTime).ToList().Count;
        }

        public int GetRecordCountByParameterAndEvent(string signalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventParameters, IEnumerable<int> eventCodes)
        {
            return GetRecordsByParameterAndEvent(signalId, startTime, endTime, eventParameters, eventCodes).Count;
        }

        public IReadOnlyList<ControllerEventLog> GetRecordsByParameterAndEvent(string signalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventParameters, IEnumerable<int> eventCodes)
        {
            var result = GetFromDateRange(signalId, startTime, endTime)
                .FromSpecification(new ControllerLogCodeAndParamSpecification(eventCodes, eventParameters))
                .OrderBy(o => o.EventParam)
                .ToList();

            return result;
        }

        [Obsolete("This method isn't currently being used")]
        public IReadOnlyList<ControllerEventLog> GetSignalEventsBetweenDates(string signalId, DateTime startTime, DateTime endTime)
        {
            return GetFromDateRange(signalId, startTime, endTime).ToList();
        }

        public IReadOnlyList<ControllerEventLog> GetSignalEventsByEventCode(string signalId, DateTime startTime, DateTime endTime, int eventCode)
        {
            var result = GetFromDateRange(signalId, startTime, endTime)
                .FromSpecification(new ControllerLogCodeAndParamSpecification(eventCode))
                .OrderBy(o => o.EventParam)
                .ToList();

            return result;
        }

        public IReadOnlyList<ControllerEventLog> GetSignalEventsByEventCodes(string signalId, DateTime startTime, DateTime endTime, IEnumerable<int> eventCodes)
        {
            var result = GetFromDateRange(signalId, startTime, endTime)
                .FromSpecification(new ControllerLogCodeAndParamSpecification(eventCodes))
                .OrderBy(o => o.EventParam)
                .ToList();

            return result;
        }

        [Obsolete("This is redundant with GetRecordCount")]
        public int GetSignalEventsCountBetweenDates(string signalId, DateTime startTime, DateTime endTime)
        {
            return GetSignalEventsBetweenDates(signalId, startTime, endTime).Count;
        }

        public IReadOnlyList<ControllerEventLog> GetSplitEvents(string signalId, DateTime startTime, DateTime endTime)
        {
            var result = GetFromDateRange(signalId, startTime, endTime)
                .Where(i => i.EventCode > 130 && i.EventCode < 150)
                .OrderBy(o => o.EventParam)
                .ToList();

            return result;
        }

        public double GetTmcVolume(DateTime startDate, DateTime endDate, string signalId, int phase)
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
            //                      && cel.SignalID == signalId
            //                      && tmcChannels.Contains(cel.EventParam)
            //                      && cel.EventCode == 82
            //                select cel).Count();

            //return count;
        }

        public IReadOnlyList<ControllerEventLog> GetTopEventsAfterDateByEventCodesParam(string signalId, DateTime timestamp, IEnumerable<int> eventCodes, int param, int top)
        {
            throw new NotImplementedException();

            //var endDate = timestamp.AddHours(1);
            //var events = _db.ControllerEventLogs.Where(c =>
            //    c.SignalID == signalId &&
            //    c.Timestamp > timestamp &&
            //    c.Timestamp < endDate &&
            //    c.EventParam == param &&
            //    eventCodes.Contains(c.EventCode)).ToList();
            //return events
            //    .OrderBy(s => s.Timestamp)
            //    .Take(top).ToList();
        }

        public IReadOnlyList<ControllerEventLog> GetTopNumberOfSignalEventsBetweenDates(string signalId, int numberOfRecords, DateTime startTime, DateTime endTime)
        {
            throw new NotImplementedException();

            //var events =
            //    (from r in _db.ControllerEventLogs
            //     where r.SignalID == signalId
            //           && r.Timestamp >= startTime
            //           && r.Timestamp < endTime
            //     select r).Take(numberOfRecords).ToList();

            //if (events != null)
            //    return events;
            //var emptyEvents = new List<ControllerEventLog>();
            //return emptyEvents;
        }

        
    }
}
