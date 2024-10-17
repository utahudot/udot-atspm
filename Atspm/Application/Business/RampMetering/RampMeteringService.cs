using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Business.TimeSpaceDiagram;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Business.RampMetering
{
    public class RampMeteringService
    {
        private readonly double speedMultiplier = 0.621371;
        public RampMeteringService() { }

        public RampMeteringResult GetChartData(Location location, RampMeteringOptions options, IReadOnlyList<IndianaEvent> events)
        {
            var activeRateCodes = new List<int>() { 1058, 1059, 1060, 1061 };
            var baseRateCodes = new List<int>() { 1042, 1043, 1044, 1045 };
            var queueOnCodes = new List<int>() { 1171, 1173, 1175 };
            var queueOffCodes = new List<int>() { 1170, 1172, 1174 };
            var startupAndShutdownCodes = new List<int>() { 1004, 1014 };
            //codes for active rate 1058-1073
            //codes for base rate 1042-1057
            // codes for start up warning 1004
            // codes for shut down warning 1014
            // codes for L1 Queue On 1171
            // codes for L2 Queue On 1173
            // codes for L3 Queue On 1175
            // codes for L1 Queue Off 1170
            // codes for L2 Queue Off 1172
            // codes for L3 Queue Off 1174

            var activeRateEvents = events.Where(e => activeRateCodes.Contains(e.EventCode));
            var baseRateEvents = events.Where(e => baseRateCodes.Contains(e.EventCode));
            var queueOnEvents = events.Where(e => queueOnCodes.Contains(e.EventCode));
            var queueOffEvents = events.Where(e => queueOffCodes.Contains(e.EventCode));

            var mainlineAvgSpeedEvents = events.Where(e => e.EventCode == 1371);
            var mainlineAvgOccurrenceEvents = events.Where(e => e.EventCode == 1372);
            var mainlineAvgFlowEvents = events.Where(e => e.EventCode == 1373);
            var startUpAndShutdownWarningEvents = events.Where(e => startupAndShutdownCodes.Contains(e.EventCode));

            var (startup, shutdown) = GetStartUpAndShutdownEvents(startUpAndShutdownWarningEvents);

            var mainlineAvgSpeedsList = mainlineAvgSpeedEvents.Select(e => new DataPointForDouble(e.Timestamp, (e.EventParam * speedMultiplier))).ToList();
            var mainlineAvgOccurrenceList = mainlineAvgOccurrenceEvents.Select(e => new DataPointForDouble(e.Timestamp, (e.EventParam / 10))).ToList();
            var mainlineAvgFlowList = mainlineAvgFlowEvents.Select(e => new DataPointForDouble(e.Timestamp, e.EventParam)).ToList();

            var lanesActiveRateList = GetDescriptionWithDataPoints(activeRateEvents);
            var lanesBaseRateList = GetDescriptionWithDataPoints(baseRateEvents);
            var queueOnList = GetDescriptionWithDataPoints(queueOnEvents);
            var queueOffList = GetDescriptionWithDataPoints(queueOffEvents);

            return new RampMeteringResult(location.LocationIdentifier, options.Start, options.End)
            {
                MainlineAvgFlow = mainlineAvgFlowList,
                MainlineAvgOcc = mainlineAvgOccurrenceList,
                MainlineAvgSpeed = mainlineAvgSpeedsList,
                StartUpWarning = startup,
                ShutdownWarning = shutdown,
                LanesActiveRate = lanesActiveRateList,
                LanesBaseRate = lanesBaseRateList,
                LanesQueueOffEvents = queueOffList,
                LanesQueueOnEvents = queueOnList,
            };
        }

        private List<DescriptionWithDataPoints> GetDescriptionWithDataPoints(IEnumerable<IndianaEvent> events)
        {
            var descriptWithDataPointsEvents = new List<DescriptionWithDataPoints>();
            var eventsByCodes = events.GroupBy(e => e.EventCode);
            int laneNumber = 1;

            foreach (var eventsByCode in eventsByCodes)
            {
                var codeEvents = eventsByCode.Select(e => new DataPointForDouble(e.Timestamp, e.EventParam)).ToList();
                descriptWithDataPointsEvents.Add(new DescriptionWithDataPoints()
                {
                    Description = laneNumber.ToString(),
                    Value = codeEvents
                });
            }

            return descriptWithDataPointsEvents;
        }

        private (List<TimeSpaceEventBase>, List<TimeSpaceEventBase>) GetStartUpAndShutdownEvents(IEnumerable<IndianaEvent> events)
        {
            var startUpList = new List<TimeSpaceEventBase>();
            var shutDownList = new List<TimeSpaceEventBase>();

            var groupingByParam = events.GroupBy(e => e.EventParam).FirstOrDefault();
            if (groupingByParam != null)
            {
                for (var i = 0; i < groupingByParam.Count() - 1; i++)
                {
                    if (groupingByParam.ElementAt(i).EventCode == 1004 && groupingByParam.ElementAt(i+1).EventCode == 1014)
                    {
                        var start = groupingByParam.ElementAt(i).Timestamp;
                        var stop = groupingByParam.ElementAt(i + 1).Timestamp;
                        var startUp = new TimeSpaceEventBase(start, stop, null);
                    }
                    if(groupingByParam.ElementAt(i).EventCode == 1004 && groupingByParam.ElementAt(i + 1).EventCode == 1014)
                    {
                        var start = groupingByParam.ElementAt(i).Timestamp;
                        var stop = groupingByParam.ElementAt(i + 1).Timestamp;
                        var startUp = new TimeSpaceEventBase(start, stop, null);
                    }
                }
            }

            return (startUpList, shutDownList);
        }
    }
}
