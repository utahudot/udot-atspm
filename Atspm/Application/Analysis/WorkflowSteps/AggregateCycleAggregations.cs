#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/AggregateSpeedItemEvents.cs
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

using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using RedToRedCycle = Utah.Udot.Atspm.Business.Common.RedToRedCycle;

namespace Utah.Udot.Atspm.Analysis.WorkflowSteps
{
    /// <summary>
    /// Transforms <see cref="IndianaEvent"/> into <see cref="AggregateCycleAggregations"/>
    /// where <see cref="IndianaEvent.EventCode"/> equals <see cref="IndianaEnumerations.VehicleDetectorOn"/>
    /// and <see cref="IndianaEvent.EventParam"/> equals <see cref="Detector.DetectorChannel"/>.
    /// </summary>
    public class AggregateCycleAggregations : TransformProcessStepBase<Tuple<Location, IEnumerable<IndianaEvent>>, IEnumerable<PhaseCycleAggregation>>
    {
        private readonly TimeSpan _binSize;

        /// <inheritdoc/>
        public AggregateCycleAggregations(TimeSpan binSize, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            _binSize = binSize;
        }

        /// <inheritdoc/>
        protected override Task<IEnumerable<PhaseCycleAggregation>> Process(Tuple<Location, IEnumerable<IndianaEvent>> input, CancellationToken cancelToken = default)
        {
            return (Task<IEnumerable<PhaseCycleAggregation>>)PhaseCycleAgg(input);
        }

        private IEnumerable<PhaseCycleAggregation> PhaseCycleAgg(Tuple<Location, IEnumerable<IndianaEvent>> input)
        {
            var indianaEvents = input.Item2;
            var location = input.Item1;
            var locationIdentifier = location.LocationIdentifier;
            DateTime binStart = indianaEvents.Select(i => i.Timestamp).OrderBy(i => i).FirstOrDefault();
            DateTime binEnd = indianaEvents.Select(i => i.Timestamp).OrderBy(i => i).LastOrDefault();
            var phaseGroups = indianaEvents.GroupBy(e => e.EventParam).ToList();
            var phaseDetails = GetPhases(location);
            var phaseBeginCount = indianaEvents.Count(i => i.EventCode == 0);

            var results = new ConcurrentBag<PhaseCycleAggregation>();
            Parallel.ForEach(phaseGroups, group =>
            {
                var phase = group.Key;
                var ordered = group.OrderBy(e => e.Timestamp).ToList();
                var approaches = phaseDetails.Where(i => i.PhaseNumber == phase).Select(i => i.Approach).ToList();

                var redCycles = GetRedToRedCycles(binStart, binEnd, ordered);
                var greenCycles = GetGreenToGreenCycles(binStart, binEnd, ordered);

                var redTime = (int)Math.Round(redCycles.Sum(c => c.TotalRedTimeSeconds));
                var yellowTime = (int)Math.Round(redCycles.Sum(c => c.TotalYellowTimeSeconds));
                var greenTime = (int)Math.Round(redCycles.Sum(c => c.TotalGreenTimeSeconds));
                var totalRedToRedCycles = redCycles.Count;
                var totalGreenToGreenCycles = greenCycles.Count;
                foreach (var approach in approaches)
                {
                    results.Add(new PhaseCycleAggregation
                    {
                        BinStartTime = binStart,
                        LocationIdentifier = locationIdentifier,
                        Start = binStart,
                        End = binEnd,
                        PhaseNumber = phase,
                        ApproachId = approach.Id,
                        RedTime = redTime,
                        YellowTime = yellowTime,
                        GreenTime = greenTime,
                        TotalRedToRedCycles = totalRedToRedCycles,
                        TotalGreenToGreenCycles = totalGreenToGreenCycles,
                        PhaseBeginCount = phaseBeginCount
                    });
                }

            });
            var enumerable = results.AsEnumerable();
            return enumerable;
        }

        public List<RedToRedCycle> GetRedToRedCycles(
            DateTime startTime,
            DateTime endTime,
            List<IndianaEvent> cycleEvents)
        {
            var cycles = cycleEvents
                .Select((eventLog, index) => new { EventLog = eventLog, Index = index })
                .Where(item =>
                    item.Index < cycleEvents.Count - 3
                    && GetEventType(cycleEvents[item.Index].EventCode) == RedToRedCycle.EventType.ChangeToRed
                    && GetEventType(cycleEvents[item.Index + 1].EventCode) == RedToRedCycle.EventType.ChangeToGreen
                    && GetEventType(cycleEvents[item.Index + 2].EventCode) == RedToRedCycle.EventType.ChangeToYellow
                    && GetEventType(cycleEvents[item.Index + 3].EventCode) == RedToRedCycle.EventType.ChangeToRed)
                .Select(item => new RedToRedCycle(
                    cycleEvents[item.Index].Timestamp,
                    cycleEvents[item.Index + 1].Timestamp,
                    cycleEvents[item.Index + 2].Timestamp,
                    cycleEvents[item.Index + 3].Timestamp))
                .ToList();

            return cycles.Where(c => c.EndTime >= startTime && c.EndTime <= endTime || c.StartTime <= endTime && c.StartTime >= startTime).ToList();
        }

        public List<GreenToGreenCycle> GetGreenToGreenCycles(DateTime startTime, DateTime endTime, List<IndianaEvent> cycleEvents)
        {
            var cycles = new List<GreenToGreenCycle>();
            for (var i = 0; i < cycleEvents.Count; i++)
                if (i < cycleEvents.Count - 3
                    && GetEventType(cycleEvents[i].EventCode) == RedToRedCycle.EventType.ChangeToGreen
                    && GetEventType(cycleEvents[i + 1].EventCode) == RedToRedCycle.EventType.ChangeToYellow
                    && GetEventType(cycleEvents[i + 2].EventCode) == RedToRedCycle.EventType.ChangeToRed
                    && GetEventType(cycleEvents[i + 3].EventCode) == RedToRedCycle.EventType.ChangeToGreen)
                    cycles.Add(new GreenToGreenCycle(cycleEvents[i].Timestamp, cycleEvents[i + 1].Timestamp,
                        cycleEvents[i + 2].Timestamp, cycleEvents[i + 3].Timestamp));
            return cycles.Where(c => c.EndTime >= startTime && c.EndTime <= endTime || c.StartTime <= endTime && c.StartTime >= startTime).ToList();
        }

        private static RedToRedCycle.EventType GetEventType(short eventCode)
        {
            return eventCode switch
            {
                1 => RedToRedCycle.EventType.ChangeToGreen,
                3 => RedToRedCycle.EventType.ChangeToEndMinGreen,
                61 => RedToRedCycle.EventType.ChangeToGreen,
                8 => RedToRedCycle.EventType.ChangeToYellow,
                63 => RedToRedCycle.EventType.ChangeToYellow,
                9 => RedToRedCycle.EventType.ChangeToRed,
                11 => RedToRedCycle.EventType.ChangeToEndOfRedClearance,
                64 => RedToRedCycle.EventType.ChangeToRed,
                66 => RedToRedCycle.EventType.OverLapDark,
                _ => RedToRedCycle.EventType.Unknown,
            };
        }
        private static List<PhaseDetail> GetPhases(Location Location)
        {
            if (Location.Approaches == null || Location.Approaches.Count == 0)
            {
                return new List<PhaseDetail>();
            }

            var phaseDetails = new List<PhaseDetail>();
            foreach (var approach in Location.Approaches)
            {
                if (approach.ProtectedPhaseNumber != 0)
                {
                    phaseDetails.Add(new PhaseDetail
                    {
                        PhaseNumber = approach.ProtectedPhaseNumber,
                        UseOverlap = approach.IsProtectedPhaseOverlap,
                        IsPermissivePhase = false,
                        Approach = approach
                    });
                }
                if (approach.PermissivePhaseNumber != null)
                {
                    phaseDetails.Add(new PhaseDetail
                    {
                        PhaseNumber = approach.PermissivePhaseNumber.Value,
                        UseOverlap = approach.IsPermissivePhaseOverlap,
                        IsPermissivePhase = true,
                        Approach = approach
                    });
                }
            }
            return phaseDetails;
        }

    }
}
