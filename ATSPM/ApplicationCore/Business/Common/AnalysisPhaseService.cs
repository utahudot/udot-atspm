using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.Common
{
    public class AnalysisPhaseData
    {
        public int PhaseNumber { get; set; }
        public string PhaseDescription { get; set; }
        public string locationId { get; set; }
        public string locationIdentifier { get; set; }
        public double PercentMaxOuts { get; set; }
        public double PercentForceOffs { get; set; }
        public int TotalPhaseTerminations { get; set; }
        public string Direction { get; set; }
        public bool IsOverlap { get; set; }
        public List<IndianaEvent> ConsecutiveForceOff { get; set; }
        public List<IndianaEvent> ConsecutiveGapOuts { get; set; }
        public List<IndianaEvent> ConsecutiveMaxOut { get; set; }
        public AnalysisPhaseCycleCollection Cycles { get; set; }
        public List<IndianaEvent> PedestrianEvents { get; set; }
        public List<IndianaEvent> TerminationEvents { get; set; }
        public List<IndianaEvent> UnknownTermination { get; set; }
        public Location Location { get; set; }
    }

    public class AnalysisPhaseService
    {
        private readonly PhaseService phaseService;

        public AnalysisPhaseService(PhaseService phaseService)
        {
            this.phaseService = phaseService;
        }

        public AnalysisPhaseData GetAnalysisPhaseData(
            int phasenumber,
            IReadOnlyList<IndianaEvent> pedestrianEvents,
            IReadOnlyList<IndianaEvent> cycleEvents,
            IReadOnlyList<IndianaEvent> terminationEvents,
            int consecutiveCount,
            Location Location
            )
        {
            var cleanTerminationEventsForPhase = CleanTerminationEvents(terminationEvents, phasenumber);
            if (Location.Approaches.IsNullOrEmpty())
            {
                return null;
            }
            var analysisPhaseData = new AnalysisPhaseData();
            var phase = phaseService.GetPhases(Location).Find(p => p.PhaseNumber == phasenumber);
            if (phase == null)
            {
                return null;
            }
            analysisPhaseData.PhaseDescription = phase.Approach.Description;
            analysisPhaseData.PhaseNumber = phasenumber;
            var cycleEventCodes = new List<DataLoggerEnum> { DataLoggerEnum.PhaseBeginGreen, DataLoggerEnum.PhaseBeginYellowChange, DataLoggerEnum.PhaseEndRedClearance };
            var phaseEvents = cycleEvents.ToList().Where(p => p.EventParam == phasenumber && cycleEventCodes.Contains(p.EventCode)).ToList();
            if (!pedestrianEvents.IsNullOrEmpty())
            {
                analysisPhaseData.PedestrianEvents = pedestrianEvents.Where(t => t.EventParam == phasenumber).ToList();
            }
            else
            {
                analysisPhaseData.PedestrianEvents = new List<IndianaEvent>();
            }
            analysisPhaseData.Cycles = new AnalysisPhaseCycleCollection(phasenumber, analysisPhaseData.locationIdentifier, phaseEvents, analysisPhaseData.PedestrianEvents, cleanTerminationEventsForPhase);
            if (!cleanTerminationEventsForPhase.IsNullOrEmpty())
            {
                analysisPhaseData.TerminationEvents = cleanTerminationEventsForPhase.Where(t => t.EventParam == phasenumber && (t.EventCode == DataLoggerEnum.PhaseGapOut || t.EventCode == DataLoggerEnum.PhaseMaxOut || t.EventCode == DataLoggerEnum.PhaseForceOff)).ToList();
            }
            else
            {
                analysisPhaseData.TerminationEvents = new List<IndianaEvent>();
            }
            analysisPhaseData.ConsecutiveGapOuts = FindConsecutiveEvents(analysisPhaseData.TerminationEvents, DataLoggerEnum.PhaseGapOut, consecutiveCount) ?? new List<IndianaEvent>();
            analysisPhaseData.ConsecutiveMaxOut = FindConsecutiveEvents(analysisPhaseData.TerminationEvents, DataLoggerEnum.PhaseMaxOut, consecutiveCount) ?? new List<IndianaEvent>();
            analysisPhaseData.ConsecutiveForceOff = FindConsecutiveEvents(analysisPhaseData.TerminationEvents, DataLoggerEnum.PhaseForceOff, consecutiveCount) ?? new List<IndianaEvent>();
            analysisPhaseData.UnknownTermination = FindUnknownTerminationEvents(cleanTerminationEventsForPhase.ToList(), phasenumber) ?? new List<IndianaEvent>();
            analysisPhaseData.PercentMaxOuts = FindPercentageConsecutiveEvents(analysisPhaseData.TerminationEvents, DataLoggerEnum.PhaseMaxOut);
            analysisPhaseData.PercentForceOffs = FindPercentageConsecutiveEvents(analysisPhaseData.TerminationEvents, DataLoggerEnum.PhaseForceOff);
            analysisPhaseData.TotalPhaseTerminations = analysisPhaseData.TerminationEvents.Count;
            analysisPhaseData.Location = Location;
            return analysisPhaseData;
        }


        /// <summary>
        ///     Constructor Used for Split monitor
        /// </summary>
        /// <param name="phasenumber"></param>
        /// <param name="locationId"></param>
        /// <param name="CycleEventsTable"></param>
        //public AnalysisPhaseData GetAnalysisPhaseData(
        //    int phasenumber,
        //    Location Location,
        //    List<IndianaEvent> CycleEventsTable)
        //{
        //    var analysisPhaseData = new AnalysisPhaseData();
        //    analysisPhaseData.PhaseNumber = phasenumber;
        //    analysisPhaseData.LocationIdentifier = Location.LocationIdentifier;
        //    analysisPhaseData.IsOverlap = false;
        //    var pedEvents = FindPedEvents(CycleEventsTable, phasenumber);
        //    var phaseEvents = FindPhaseEvents(CycleEventsTable, phasenumber);
        //    analysisPhaseData.Cycles = new AnalysisPhaseCycleCollection(phasenumber, analysisPhaseData.LocationIdentifier, phaseEvents, pedEvents);
        //    var approach = Location.Approaches.FirstOrDefault(a => a.ProtectedPhaseNumber == phasenumber);
        //    analysisPhaseData.Direction = approach != null ? approach.DirectionType.Description : "Unknown";
        //    analysisPhaseData.Location = Location;
        //    return analysisPhaseData;
        //}

        public List<IndianaEvent> CleanTerminationEvents(IReadOnlyList<IndianaEvent> terminationEvents,
            int phasenumber)
        {

            var sortedEvents = terminationEvents.Where(t => t.EventParam == phasenumber).OrderBy(x => x.Timestamp).ThenBy(y => y.EventCode).ToList();
            var duplicateList = new List<IndianaEvent>();
            for (int i = 0; i < sortedEvents.Count - 1; i++)
            {
                var event1 = sortedEvents[i];
                var event2 = sortedEvents[i + 1];
                if (event1.Timestamp == event2.Timestamp)
                {
                    if (event1.EventCode == DataLoggerEnum.PhaseGreenTermination)
                        duplicateList.Add(event1);
                    if (event2.EventCode == DataLoggerEnum.PhaseGreenTermination)
                        duplicateList.Add(event2);
                }
            }

            foreach (var e in duplicateList)
            {
                sortedEvents.Remove(e);
            }
            return sortedEvents;
        }

        public List<IndianaEvent> FindPedEvents(IReadOnlyList<IndianaEvent> terminationeventstable,
            int phasenumber)
        {
            var events = (from row in terminationeventstable
                          where row.EventParam == phasenumber && (row.EventCode == DataLoggerEnum.PedestrianBeginWalk || row.EventCode == DataLoggerEnum.PedestrianBeginSolidDontWalk)
                          orderby row.Timestamp
                          select row).ToList();

            return events;
        }

        public List<IndianaEvent> FindPhaseEvents(List<IndianaEvent> PhaseEventsTable, int PhaseNumber)
        {
            var events = (from row in PhaseEventsTable
                          where row.EventParam == PhaseNumber
                          orderby row.Timestamp
                          select row).ToList();

            return events;
        }

        private List<IndianaEvent> FindConsecutiveEvents(List<IndianaEvent> terminationEvents,
            DataLoggerEnum eventtype, int consecutiveCount)
        {
            var ConsecutiveEvents = new List<IndianaEvent>();
            var runningConsecCount = 0;
            // Order the events by datestamp
            var eventsInOrder = terminationEvents.OrderBy(TerminationEvent => TerminationEvent.Timestamp);
            foreach (var termEvent in eventsInOrder)
            {
                if (termEvent.EventCode == eventtype)
                    runningConsecCount++;
                else
                    runningConsecCount = 0;

                if (runningConsecCount >= consecutiveCount)
                    ConsecutiveEvents.Add(termEvent);
            }
            return ConsecutiveEvents;
        }

        private List<IndianaEvent> FindUnknownTerminationEvents(List<IndianaEvent> terminationEvents, int phaseNumber)
        {
            return terminationEvents.Where(t => t.EventCode == DataLoggerEnum.PhaseGreenTermination && t.EventParam == phaseNumber).ToList();
        }


        private double FindPercentageConsecutiveEvents(List<IndianaEvent> terminationEvents, DataLoggerEnum eventtype)
        {
            double percentile = 0;
            double total = terminationEvents.Count(t => t.EventCode != DataLoggerEnum.PhaseGreenTermination);
            //Get all termination events of the event type
            var terminationEventsOfType = terminationEvents.Count(terminationEvent => terminationEvent.EventCode == eventtype);

            if (terminationEvents.Any())
                percentile = terminationEventsOfType / total;
            return percentile;
        }
    }
}