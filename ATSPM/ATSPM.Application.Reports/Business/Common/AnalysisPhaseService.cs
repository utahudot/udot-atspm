using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using Legacy.Common.Business;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Reports.Business.Common
{
    public class AnalysisPhaseData
    {
        public int PhaseNumber { get; set; }
        public string SignalId { get; set; }
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
        public Signal Signal { get; set; }
    }

    public class AnalysisPhaseService
    {
        private readonly ISignalRepository _signalRepository;



        public AnalysisPhaseService(ISignalRepository signalRepository)
        {
            _signalRepository = signalRepository;
        }

        public AnalysisPhaseData GetAnalysisPhaseData(
            int phasenumber,
            IReadOnlyList<ControllerEventLog> terminationeventstable,
            int consecutiveCount,
            Signal signal
            )
        {
            var analysisPhaseData = new AnalysisPhaseData();
            analysisPhaseData.PhaseNumber = phasenumber;
            analysisPhaseData.TerminationEvents = FindTerminationEvents(terminationeventstable, analysisPhaseData.PhaseNumber);
            analysisPhaseData.PedestrianEvents = FindPedEvents(terminationeventstable, analysisPhaseData.PhaseNumber);
            analysisPhaseData.ConsecutiveGapOuts = FindConsecutiveEvents(analysisPhaseData.TerminationEvents, 4, consecutiveCount);
            analysisPhaseData.ConsecutiveMaxOut = FindConsecutiveEvents(analysisPhaseData.TerminationEvents, 5, consecutiveCount);
            analysisPhaseData.ConsecutiveForceOff = FindConsecutiveEvents(analysisPhaseData.TerminationEvents, 6, consecutiveCount);
            analysisPhaseData.UnknownTermination = FindUnknownTerminationEvents(analysisPhaseData.TerminationEvents);
            analysisPhaseData.PercentMaxOuts = FindPercentageConsecutiveEvents(analysisPhaseData.TerminationEvents, 5, consecutiveCount);
            analysisPhaseData.PercentForceOffs = FindPercentageConsecutiveEvents(analysisPhaseData.TerminationEvents, 6, consecutiveCount);
            analysisPhaseData.TotalPhaseTerminations = analysisPhaseData.TerminationEvents.Count;
            analysisPhaseData.Signal = signal;
            return analysisPhaseData;
        }


        /// <summary>
        ///     Constructor Used for Split monitor
        /// </summary>
        /// <param name="phasenumber"></param>
        /// <param name="signalID"></param>
        /// <param name="CycleEventsTable"></param>
        public AnalysisPhaseData GetAnalysisPhaseData(
            int phasenumber,
            Signal signal,
            List<ControllerEventLog> CycleEventsTable)
        {
            var analysisPhaseData = new AnalysisPhaseData();
            analysisPhaseData.PhaseNumber = phasenumber;
            analysisPhaseData.SignalId = signal.SignalId;
            analysisPhaseData.IsOverlap = false;
            var pedEvents = FindPedEvents(CycleEventsTable, phasenumber);
            var phaseEvents = FindPhaseEvents(CycleEventsTable, phasenumber);
            analysisPhaseData.Cycles = new AnalysisPhaseCycleCollection(phasenumber, analysisPhaseData.SignalId, phaseEvents, pedEvents);
            var approach = signal.Approaches.FirstOrDefault(a => a.ProtectedPhaseNumber == phasenumber);
            analysisPhaseData.Direction = approach != null ? approach.DirectionType.Description : "Unknown";
            analysisPhaseData.Signal = signal;
            return analysisPhaseData;
        }

        public List<ControllerEventLog> FindTerminationEvents(IReadOnlyList<ControllerEventLog> terminationeventstable,
            int phasenumber)
        {
            var events = (from row in terminationeventstable
                          where row.EventParam == phasenumber && (row.EventCode == 4 ||
                                                                  row.EventCode == 5 || row.EventCode == 6
                                                                  || row.EventCode == 7
                                )
                          select row).ToList();

            var sortedEvents = events.OrderBy(x => x.Timestamp).ThenBy(y => y.EventCode).ToList();
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
                if (termEvent.EventCode != 7)
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

        private List<ControllerEventLog> FindUnknownTerminationEvents(List<ControllerEventLog> terminationEvents)
        {
            return terminationEvents.Where(t => t.EventCode == 7).ToList();
        }


        private double FindPercentageConsecutiveEvents(List<ControllerEventLog> terminationEvents, int eventtype,
            int consecutiveCount)
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