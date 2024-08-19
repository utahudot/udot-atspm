﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.TimeSpaceDiagram/TimeSpaceAverageService.cs
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

using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Business.TimingAndActuation;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Business.TimeSpaceDiagram
{
    public class TimeSpaceAverageService
    {
        private readonly CycleService _cycleService;
        private readonly Dictionary<int, short> phaseToProgramPhases = new Dictionary<int, short>()
        {
            { 1, 134 },
            { 2, 135 },
            { 3, 136 },
            { 4, 137 },
            { 5, 138 },
            { 6, 139 },
            { 7, 140 },
            { 8, 141},
        };

        public TimeSpaceAverageService(CycleService cycleService)
        {
            _cycleService = cycleService;
        }

        public TimeSpaceDiagramAverageResult GetChartData(
           TimeSpaceDiagramAverageOptions options,
           PhaseDetail phaseDetail,
           List<IndianaEvent> controllerEventLogs,
           int[][] sequence,
           int[] coordPhases,
           List<IndianaEvent> programSplits,
           int offset,
           int cycleLength,
           double distanceToNextLocation,
           bool isLastElement,
           bool isCoordPhasesMatchRoutePhases
           )
        {
            int[] topSequence = sequence[0];
            int[] bottomSequence = sequence[1];
            int topSequenceIndex = getIndexOfCoordPhaseInSequence(topSequence, coordPhases);
            int bottomSequenceIndex = getIndexOfCoordPhaseInSequence(bottomSequence, coordPhases);

            var (selectedSequence, selectedPhase, sequenceAdjustment) = CalculateSelectedSequenceAdjustment(topSequence,
                bottomSequence,
                programSplits,
                topSequenceIndex,
                bottomSequenceIndex,
                phaseDetail);
            int programmedSplit = programSplits.Find(s => s.EventCode == phaseToProgramPhases[selectedPhase]).EventParam;

            GreenToGreenCycle percentileSplitCycle = GetPercentileSplitCycle(options, controllerEventLogs, selectedPhase, phaseDetail.UseOverlap);
            double startOfRefPoint = 0;

            if (isCoordPhasesMatchRoutePhases)
            {
                startOfRefPoint = CalculateStartOfRefPointForCoordPhases(offset, programmedSplit, percentileSplitCycle);
            }
            else
            {
                startOfRefPoint = CalculateStartOfRefPointForNonCoordPhases(options, offset, selectedPhase, selectedSequence, programSplits, controllerEventLogs, phaseDetail);
            }
            var cycleEvents = CreateCyclesEvents(startOfRefPoint, options.StartDate.ToDateTime(options.EndTime), options.StartDate.ToDateTime(options.StartTime), cycleLength, percentileSplitCycle);

            var greenTimeEventsResult = new List<TimeSpaceEventBase>();
            var speedLimit = options.SpeedLimit ?? phaseDetail.Approach.Mph ?? 0;

            if (speedLimit == 0)
            {
                throw new ArgumentNullException($"Speed not configured in route for all phases");
            }

            if (!isLastElement)
            {
                greenTimeEventsResult = TimeSpaceService.GetGreenTimeEvents(cycleEvents, speedLimit, distanceToNextLocation);
            }

            var phaseNumberSort = TimeSpaceService.GetPhaseSort(phaseDetail);
            return new TimeSpaceDiagramAverageResult(
                phaseDetail.Approach.Id,
                phaseDetail.Approach.Location.LocationIdentifier,
                options.StartDate.ToDateTime(options.StartTime),
                options.EndDate.ToDateTime(options.EndTime),
                phaseDetail.PhaseNumber,
                phaseNumberSort,
                distanceToNextLocation,
                speedLimit,
                offset,
                programmedSplit,
                isCoordPhasesMatchRoutePhases,
                cycleLength,
                cycleEvents,
                greenTimeEventsResult);
        }

        private double CalculateStartOfRefPointForNonCoordPhases(TimeSpaceDiagramAverageOptions options,
            int offset,
            int selectedPhase,
            int[] selectedSequence,
            List<IndianaEvent> programSplits,
            List<IndianaEvent> controllerEventLogs,
            PhaseDetail phaseDetail)
        {
            var selectedPhaseIndex = Array.FindIndex(selectedSequence, e => e == selectedPhase);
            var sumOfPercentileSplits = getSumOfPercentileSplits(options, selectedSequence, selectedPhaseIndex, controllerEventLogs, phaseDetail.UseOverlap);
            var sumOfProgramSplits = CalculateSplitSumBeforePhase(selectedSequence, programSplits, selectedPhaseIndex);

            return offset + (sumOfPercentileSplits - sumOfProgramSplits);
        }

        private double getSumOfPercentileSplits(TimeSpaceDiagramAverageOptions options,
            int[] selectedSequence,
            int selectedPhaseIndex,
            List<IndianaEvent> controllerEventLogs,
            bool useOverlap)
        {
            double sumOfPercentileSplits = 0;
            for (int i = 0; i < selectedPhaseIndex; i++)
            {
                var phase = selectedSequence[i];
                var percentileSplit = GetPercentileSplitCycle(options, controllerEventLogs, phase, useOverlap);

                sumOfPercentileSplits += percentileSplit.TotalGreenTime + percentileSplit.TotalYellowTime;
            }

            return sumOfPercentileSplits;
        }

        private (int[], int, int) CalculateSelectedSequenceAdjustment(int[] topSequence,
            int[] bottomSequence,
            List<IndianaEvent> programSplits,
            int topSequenceIndex,
            int bottomSequenceIndex,
            PhaseDetail phaseDetail)
        {
            var timeBeforeFirstPhase = CalculateSplitSumBeforePhase(topSequence, programSplits, topSequenceIndex);
            var timeBeforeSecondPhase = CalculateSplitSumBeforePhase(bottomSequence, programSplits, bottomSequenceIndex);

            var minTimeBeforePhase = Math.Min(timeBeforeSecondPhase, timeBeforeFirstPhase);

            if (topSequence.Contains(phaseDetail.PhaseNumber))
            {
                return (topSequence, topSequence[topSequenceIndex], timeBeforeFirstPhase - minTimeBeforePhase);
            }
            else
            {
                return (bottomSequence, bottomSequence[bottomSequenceIndex], timeBeforeSecondPhase - minTimeBeforePhase);
            }
        }

        private int getIndexOfCoordPhaseInSequence(int[] sequence, int[] coordPhases)
        {
            var index = Array.IndexOf(sequence, coordPhases[0]);
            if (index == -1)
            {
                index = Array.IndexOf(sequence, coordPhases[1]);
            }
            return index;
        }

        private List<CycleEventsDto> CreateCyclesEvents(double startOfGreen,
            DateTime end,
            DateTime start,
            int cycleLength,
            GreenToGreenCycle percentileSplitCycle)
        {
            List<GreenToGreenCycle> cycles = new List<GreenToGreenCycle>();
            var events = new List<CycleEventsDto>();
            if (percentileSplitCycle == null)
            {
                return events;
            }
            var startTime = start.AddSeconds(startOfGreen);
            var greenTime = percentileSplitCycle.TotalGreenTime;
            var yellowTime = percentileSplitCycle.TotalYellowTime;
            var redTime = cycleLength - (greenTime + yellowTime) > 0 ? cycleLength - (greenTime + yellowTime) : percentileSplitCycle.TotalRedTime;



            while (startTime <= end.AddMinutes(2))
            {
                var startOfYellowTime = startTime.AddSeconds(greenTime);
                var startOfRedTime = startOfYellowTime.AddSeconds(yellowTime);
                var startOfLastGreenTime = startOfRedTime.AddSeconds(redTime);

                cycles.Add(new GreenToGreenCycle(startTime, startOfYellowTime, startOfRedTime, startOfLastGreenTime));
                startTime = startOfLastGreenTime;
            }

            for (int i = 0; i < cycles.Count; i++)
            {
                var cycle = cycles[i];

                events.Add(new CycleEventsDto(cycle.StartTime, 1));
                events.Add(new CycleEventsDto(cycle.YellowEvent, 8));
                events.Add(new CycleEventsDto(cycle.RedEvent, 9));
            }


            return events;
        }

        private int GetPhaseToCalculateEventsFor(int[] topSequence, int[] bottomSequence, List<IndianaEvent> programSplits, int topSequenceIndex, int bottomSequenceIndex)
        {
            var timeBeforeFirstPhase = CalculateSplitSumBeforePhase(topSequence, programSplits, topSequenceIndex);
            var timeBeforeSecondPhase = CalculateSplitSumBeforePhase(bottomSequence, programSplits, bottomSequenceIndex);

            return timeBeforeFirstPhase < timeBeforeSecondPhase ? topSequence[topSequenceIndex] : bottomSequence[bottomSequenceIndex];
        }

        private int CalculateSplitSumBeforePhase(int[] sequence, List<IndianaEvent> programSplits, int phaseIndex)
        {
            var timeBeforePhase = 0;
            for (var i = 0; i < phaseIndex; i++)
            {
                var programPhase = phaseToProgramPhases[sequence[i]];
                timeBeforePhase += programSplits.Find(s => s.EventCode == programPhase)?.EventParam ?? 0;
            }
            return timeBeforePhase;
        }

        private GreenToGreenCycle GetPercentileSplitCycle(TimeSpaceDiagramAverageOptions options, List<IndianaEvent> controllerEventLogs, int phaseToCalculateEventsFor, bool overlap)
        {
            var cycleEventCodes = TimeSpaceService.GetCycleCodes(overlap);
            var events = TimeSpaceService.GetEvents(phaseToCalculateEventsFor, controllerEventLogs, cycleEventCodes);
            var cycles = _cycleService.GetGreenToGreenCycles(options.StartDate.ToDateTime(options.StartTime), options.EndDate.ToDateTime(options.EndTime), events);

            cycles.Sort((x, y) => x.TotalGreenTime.CompareTo(y.TotalGreenTime));

            if (cycles.Count <= 0)
            {
                return null;
            }
            int medianIndex = cycles.Count / 2;
            if (medianIndex < 0)
            {
                return null;
            }

            return cycles[medianIndex];
        }

        private double CalculateStartOfRefPointForCoordPhases(int offset, int programmedSplit, GreenToGreenCycle percentileSplit)
        {
            if (percentileSplit == null)
            {
                return offset - programmedSplit;
            }
            return offset - (percentileSplit.TotalGreenTime + percentileSplit.TotalYellowTime - programmedSplit);
        }
    }
}
