using ATSPM.Data.Models;
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
        public List<ControllerEventLog> ConsecutiveForceOff { get; set; }
        public List<ControllerEventLog> ConsecutiveGapOuts { get; set; }
        public List<ControllerEventLog> ConsecutiveMaxOut { get; set; }
        public AnalysisPhaseCycleCollection Cycles { get; set; }
        public List<ControllerEventLog> PedestrianEvents { get; set; }
        public List<ControllerEventLog> TerminationEvents { get; set; }
        public List<ControllerEventLog> UnknownTermination { get; set; }
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
            IReadOnlyList<ControllerEventLog> pedestrianEvents,
            IReadOnlyList<ControllerEventLog> cycleEvents,
            IReadOnlyList<ControllerEventLog> terminationEvents,
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
            var cycleEventCodes = new List<int> { 1, 8, 11 };
            var phaseEvents = cycleEvents.ToList().Where(p => p.EventParam == phasenumber && cycleEventCodes.Contains(p.EventCode)).ToList();
            if (!pedestrianEvents.IsNullOrEmpty())
            {
                analysisPhaseData.PedestrianEvents = pedestrianEvents.Where(t => t.EventParam == phasenumber).ToList();
            }
            else
            {
                analysisPhaseData.PedestrianEvents = new List<ControllerEventLog>();
            }
            analysisPhaseData.Cycles = new AnalysisPhaseCycleCollection(phasenumber, analysisPhaseData.locationIdentifier, phaseEvents, analysisPhaseData.PedestrianEvents, cleanTerminationEventsForPhase);
            if (!cleanTerminationEventsForPhase.IsNullOrEmpty())
            {
                analysisPhaseData.TerminationEvents = cleanTerminationEventsForPhase.Where(t => t.EventParam == phasenumber && (t.EventCode == 4 || t.EventCode == 5 || t.EventCode == 6)).ToList();
            }
            else
            {
                analysisPhaseData.TerminationEvents = new List<ControllerEventLog>();
            }
            analysisPhaseData.ConsecutiveGapOuts = FindConsecutiveEvents(analysisPhaseData.TerminationEvents, 4, consecutiveCount) ?? new List<ControllerEventLog>();
            analysisPhaseData.ConsecutiveMaxOut = FindConsecutiveEvents(analysisPhaseData.TerminationEvents, 5, consecutiveCount) ?? new List<ControllerEventLog>();
            analysisPhaseData.ConsecutiveForceOff = FindConsecutiveEvents(analysisPhaseData.TerminationEvents, 6, consecutiveCount) ?? new List<ControllerEventLog>();
            analysisPhaseData.UnknownTermination = FindUnknownTerminationEvents(cleanTerminationEventsForPhase.ToList(), phasenumber) ?? new List<ControllerEventLog>();
            analysisPhaseData.PercentMaxOuts = FindPercentageConsecutiveEvents(analysisPhaseData.TerminationEvents, 5);
            analysisPhaseData.PercentForceOffs = FindPercentageConsecutiveEvents(analysisPhaseData.TerminationEvents, 6);
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
        //    List<ControllerEventLog> CycleEventsTable)
        //{
        //    var analysisPhaseData = new AnalysisPhaseData();
        //    analysisPhaseData.PhaseNumber = phasenumber;
        //    analysisPhaseData.locationIdentifier = Location.locationIdentifier;
        //    analysisPhaseData.IsOverlap = false;
        //    var pedEvents = FindPedEvents(CycleEventsTable, phasenumber);
        //    var phaseEvents = FindPhaseEvents(CycleEventsTable, phasenumber);
        //    analysisPhaseData.Cycles = new AnalysisPhaseCycleCollection(phasenumber, analysisPhaseData.locationIdentifier, phaseEvents, pedEvents);
        //    var approach = Location.Approaches.FirstOrDefault(a => a.ProtectedPhaseNumber == phasenumber);
        //    analysisPhaseData.Direction = approach != null ? approach.DirectionType.Description : "Unknown";
        //    analysisPhaseData.Location = Location;
        //    return analysisPhaseData;
        //}

        public List<ControllerEventLog> CleanTerminationEvents(IReadOnlyList<ControllerEventLog> terminationEvents,
            int phasenumber)
        {

            var sortedEvents = terminationEvents.Where(t => t.EventParam == phasenumber).OrderBy(x => x.Timestamp).ThenBy(y => y.EventCode).ToList();
            var duplicateList = new List<ControllerEventLog>();
            for (int i = 0; i < sortedEvents.Count - 1; i++)
            {
                var event1 = sortedEvents[i];
                var event2 = sortedEvents[i + 1];
                if (event1.Timestamp == event2.Timestamp)
                {
                    if (event1.EventCode == 7)
                        duplicateList.Add(event1);
                    if (event2.EventCode == 7)
                        duplicateList.Add(event2);
                }
            }

            foreach (var e in duplicateList)
            {
                sortedEvents.Remove(e);
            }
            return sortedEvents;
        }

        public List<ControllerEventLog> FindPedEvents(IReadOnlyList<ControllerEventLog> terminationeventstable,
            int phasenumber)
        {
            var events = (from row in terminationeventstable
                          where row.EventParam == phasenumber && (row.EventCode == 21 || row.EventCode == 23)
                          orderby row.Timestamp
                          select row).ToList();

            return events;
        }

        public List<ControllerEventLog> FindPhaseEvents(List<ControllerEventLog> PhaseEventsTable, int PhaseNumber)
        {
            var events = (from row in PhaseEventsTable
                          where row.EventParam == PhaseNumber
                          orderby row.Timestamp
                          select row).ToList();

            return events;
        }

        private List<ControllerEventLog> FindConsecutiveEvents(List<ControllerEventLog> terminationEvents,
            int eventtype, int consecutiveCount)
        {
            var ConsecutiveEvents = new List<ControllerEventLog>();
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

        private List<ControllerEventLog> FindUnknownTerminationEvents(List<ControllerEventLog> terminationEvents, int phaseNumber)
        {
            return terminationEvents.Where(t => t.EventCode == 7 && t.EventParam == phaseNumber).ToList();
        }


        private double FindPercentageConsecutiveEvents(List<ControllerEventLog> terminationEvents, int eventtype)
        {
            double percentile = 0;
            double total = terminationEvents.Count(t => t.EventCode != 7);
            //Get all termination events of the event type
            var terminationEventsOfType = terminationEvents.Count(terminationEvent => terminationEvent.EventCode == eventtype);

            if (terminationEvents.Any())
                percentile = terminationEventsOfType / total;
            return percentile;
        }
    }
}