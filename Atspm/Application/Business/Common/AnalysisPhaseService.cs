﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.Common/AnalysisPhaseService.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.IdentityModel.Tokens;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Business.Common
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
            int phaseNumber,
            IReadOnlyList<IndianaEvent> pedestrianEvents,
            IReadOnlyList<IndianaEvent> cycleEvents,
            IReadOnlyList<IndianaEvent> terminationEvents,
            int consecutiveCount,
            Location location
            )
        {
            var cleanTerminationEventsForPhase = CleanTerminationEvents(terminationEvents, phaseNumber);
            if (location.Approaches.IsNullOrEmpty())
            {
                return null;
            }
            var analysisPhaseData = new AnalysisPhaseData();
            var phase = phaseService.GetPhases(location).Find(p => p.PhaseNumber == phaseNumber);
            SetPhaseDescription(analysisPhaseData, phase, phaseNumber);
            analysisPhaseData.PhaseNumber = phaseNumber;
            var cycleEventCodes = new List<short> { 1, 8, 11 };
            var phaseEvents = cycleEvents.ToList().Where(p => p.EventParam == phaseNumber && cycleEventCodes.Contains(p.EventCode)).ToList();
            if (!pedestrianEvents.IsNullOrEmpty())
            {
                analysisPhaseData.PedestrianEvents = pedestrianEvents.Where(t => t.EventParam == phaseNumber && (t.EventCode == 21 || t.EventCode == 23)).ToList();
            }
            else
            {
                analysisPhaseData.PedestrianEvents = new List<IndianaEvent>();
            }
            analysisPhaseData.Cycles = new AnalysisPhaseCycleCollection(phaseNumber, analysisPhaseData.locationIdentifier, phaseEvents, analysisPhaseData.PedestrianEvents, cleanTerminationEventsForPhase);
            if (!cleanTerminationEventsForPhase.IsNullOrEmpty())
            {
                analysisPhaseData.TerminationEvents = cleanTerminationEventsForPhase.Where(t => t.EventParam == phaseNumber && (t.EventCode == 4 || t.EventCode == 5 || t.EventCode == 6)).ToList();
            }
            else
            {
                analysisPhaseData.TerminationEvents = new List<IndianaEvent>();
            }
            analysisPhaseData.ConsecutiveGapOuts = FindConsecutiveEvents(analysisPhaseData.TerminationEvents, 4, consecutiveCount) ?? new List<IndianaEvent>();
            analysisPhaseData.ConsecutiveMaxOut = FindConsecutiveEvents(analysisPhaseData.TerminationEvents, 5, consecutiveCount) ?? new List<IndianaEvent>();
            analysisPhaseData.ConsecutiveForceOff = FindConsecutiveEvents(analysisPhaseData.TerminationEvents, 6, consecutiveCount) ?? new List<IndianaEvent>();
            analysisPhaseData.UnknownTermination = FindUnknownTerminationEvents(cleanTerminationEventsForPhase.ToList(), phaseNumber) ?? new List<IndianaEvent>();
            analysisPhaseData.PercentMaxOuts = FindPercentageConsecutiveEvents(analysisPhaseData.TerminationEvents, 5);
            analysisPhaseData.PercentForceOffs = FindPercentageConsecutiveEvents(analysisPhaseData.TerminationEvents, 6);
            analysisPhaseData.TotalPhaseTerminations = analysisPhaseData.TerminationEvents.Count;
            analysisPhaseData.Location = location;
            return analysisPhaseData;
        }

        private static void SetPhaseDescription(AnalysisPhaseData analysisPhaseData, PhaseDetail phase, int phaseNumber)
        {
            if (phase == null)
            {
                analysisPhaseData.PhaseDescription = $"Phase {phaseNumber} (Unconfigured)";
            }
            else
            {
                analysisPhaseData.PhaseDescription = phase.GetApproachDescription();
            }
        }


        /// <summary>
        ///     Constructor Used for Split monitor
        /// </summary>
        /// <param name="phasenumber"></param>
        /// <param name="LocationId"></param>
        /// <param name="CycleEventsTable"></param>
        //public AnalysisPhaseData GetAnalysisPhaseData(
        //    int phaseNumber,
        //    location location,
        //    List<IndianaEvent> CycleEventsTable)
        //{
        //    var analysisPhaseData = new AnalysisPhaseData();
        //    analysisPhaseData.PhaseNumber = phaseNumber;
        //    analysisPhaseData.LocationIdentifier = location.LocationIdentifier;
        //    analysisPhaseData.IsOverlap = false;
        //    var pedEvents = FindPedEvents(CycleEventsTable, phaseNumber);
        //    var phaseEvents = FindPhaseEvents(CycleEventsTable, phaseNumber);
        //    analysisPhaseData.Cycles = new AnalysisPhaseCycleCollection(phaseNumber, analysisPhaseData.LocationIdentifier, phaseEvents, pedEvents);
        //    var approach = location.Approaches.FirstOrDefault(a => a.ProtectedPhaseNumber == phaseNumber);
        //    analysisPhaseData.Direction = approach != null ? approach.DirectionType.Id : "Unknown";
        //    analysisPhaseData.location = location;
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

        public List<IndianaEvent> FindPedEvents(IReadOnlyList<IndianaEvent> terminationeventstable,
            int phasenumber)
        {
            var events = (from row in terminationeventstable
                          where row.EventParam == phasenumber && (row.EventCode == 21 || row.EventCode == 23)
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
            short eventtype, int consecutiveCount)
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
            return terminationEvents.Where(t => t.EventCode == 7 && t.EventParam == phaseNumber).ToList();
        }


        private double FindPercentageConsecutiveEvents(List<IndianaEvent> terminationEvents, short eventtype)
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