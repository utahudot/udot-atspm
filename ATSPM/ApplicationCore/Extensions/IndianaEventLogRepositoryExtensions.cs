using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Application.Specifications;
using ATSPM.Data.Enums;
using ATSPM.Data.Models.EventLogModels;
using ATSPM.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Extensions
{
    public static class IndianaEventLogRepositoryExtensions
    {
        public static IReadOnlyList<IndianaEvent> GetEventsByEventCodesParam(this IIndianaEventLogRepository repo, string locationId, DateTime startTime, DateTime endTime, IEnumerable<DataLoggerEnum> eventCodes, int param)
        {
            var result = repo.GetEventsBetweenDates(locationId, startTime, endTime)
                .FromSpecification(new IndianaLogCodeAndParamSpecification(eventCodes, param))

                //this orderby was in the original version but it's always the same param so ordering is pointless
                //.OrderBy(o => o.EventCode)

                .ToList();

            return result;
        }

        public static IReadOnlyList<IndianaEvent> GetEventsByEventCodesParam(this IIndianaEventLogRepository repo, string locationId, DateTime startTime, DateTime endTime, IEnumerable<DataLoggerEnum> eventCodes, int param, double latencyCorrection)
        {
            var result = repo.GetEventsByEventCodesParam(locationId, startTime, endTime, eventCodes, param);

            foreach (var item in result)
            {
                item.Timestamp = item.Timestamp.AddSeconds(0 - latencyCorrection);
            }

            return result;
        }

        public static IReadOnlyList<IndianaEvent> GetEventsByEventCodesParam(this IIndianaEventLogRepository repo, string locationId, DateTime startTime, DateTime endTime, IEnumerable<DataLoggerEnum> eventCodes, int param, double offset, double latencyCorrection)
        {
            var result = repo.GetEventsByEventCodesParam(locationId, startTime, endTime, eventCodes, param);

            foreach (var item in result)
            {
                item.Timestamp = item.Timestamp.AddMilliseconds(offset);
                item.Timestamp = item.Timestamp.AddSeconds(0 - latencyCorrection);
            }

            return result;
        }

        public static IReadOnlyList<IndianaEvent> GetRecordsByParameterAndEvent(this IIndianaEventLogRepository repo, string locationId, DateTime startTime, DateTime endTime, IEnumerable<int> eventParameters, IEnumerable<DataLoggerEnum> eventCodes)
        {
            var result = repo.GetEventsBetweenDates(locationId, startTime, endTime)
                .FromSpecification(new IndianaLogCodeAndParamSpecification(eventCodes, eventParameters))
                .OrderBy(o => o.EventParam)
                .ToList();

            return result;
        }

        public static int GetRecordCountByParameterAndEvent(this IIndianaEventLogRepository repo, string locationId, DateTime startTime, DateTime endTime, IEnumerable<int> eventParameters, IEnumerable<DataLoggerEnum> eventCodes)
        {
            return repo.GetRecordsByParameterAndEvent(locationId, startTime, endTime, eventParameters, eventCodes).Count;
        }

        public static IReadOnlyList<IndianaEvent> GetAllAggregationCodes(this IIndianaEventLogRepository repo, string locationId, DateTime startTime, DateTime endTime)
        {
            var codes = new List<DataLoggerEnum>
            {
                DataLoggerEnum.Coordcyclestatechange,
                DataLoggerEnum.TSPAdjustmenttoExtendGreen,
                DataLoggerEnum.TSPAdjustmenttoEarlyGreen,
                DataLoggerEnum.TSPCheckIn,
                DataLoggerEnum.PreemptEntryStarted,
                DataLoggerEnum.PreemptCallInputOn,
                DataLoggerEnum.PhaseBeginGreen
            };

            var result = repo.GetEventsBetweenDates(locationId, startTime, endTime)
                .FromSpecification(new IndianaLogCodeAndParamSpecification(codes))
                .ToList();

            return result;
        }

        public static int GetDetectorActivationCount(this IIndianaEventLogRepository repo, string locationId, DateTime startTime, DateTime endTime, int detectorChannel)
        {
            var result = repo.GetEventsBetweenDates(locationId, startTime, endTime)
                .FromSpecification(new IndianaLogCodeAndParamSpecification(DataLoggerEnum.DetectorOn, detectorChannel))
                .ToList().Count;

            return result;
        }



        public static IReadOnlyList<IndianaEvent> GetLocationEventsByEventCode(this IIndianaEventLogRepository repo, string locationId, DateTime startTime, DateTime endTime, DataLoggerEnum eventCode)
        {
            var result = repo.GetEventsBetweenDates(locationId, startTime, endTime)
                .FromSpecification(new IndianaLogCodeAndParamSpecification(eventCode))
                .OrderBy(o => o.EventParam)
                .ToList();

            return result;
        }

        public static IReadOnlyList<IndianaEvent> GetLocationEventsByEventCodes(this IIndianaEventLogRepository repo, string locationId, DateTime startTime, DateTime endTime, IEnumerable<DataLoggerEnum> eventCodes)
        {
            var result = repo.GetEventsBetweenDates(locationId, startTime, endTime)
                .FromSpecification(new IndianaLogCodeAndParamSpecification(eventCodes))
                .OrderBy(o => o.EventParam)
                .ToList();

            return result;
        }

        public static IReadOnlyList<IndianaEvent> GetSplitEvents(this IIndianaEventLogRepository repo, string locationId, DateTime startTime, DateTime endTime)
        {
            var result = repo.GetEventsBetweenDates(locationId, startTime, endTime)
                .Where(i => (int)i.EventCode > 130 && (int)i.EventCode < 150)
                .OrderBy(o => o.EventParam)
                .ToList();

            return result;
        }



    }
}
