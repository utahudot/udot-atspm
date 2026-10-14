#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/CreateSplitFailCyclesStep.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Extensions;
using Utah.Udot.Atspm.TempExtensions;

namespace Utah.Udot.Atspm.Analysis.WorkflowSteps
{
    /// <summary>
    /// Represents the green, yellow, and red change interval boundary timestamps for a split failure cycle.
    /// </summary>
    public struct CycleTimestamps
    {
        /// <summary>
        /// Gets or sets the start time of the green interval.
        /// </summary>
        public DateTime GreenStart { get; set; }

        /// <summary>
        /// Gets or sets the start time of the yellow change interval.
        /// </summary>
        public DateTime YellowStart { get; set; }

        /// <summary>
        /// Gets or sets the start time of the red clearance interval.
        /// </summary>
        public DateTime RedStart { get; set; }

        /// <summary>
        /// Gets or sets the end time of the green interval in the next cycle.
        /// </summary>
        public DateTime GreenEnd { get; set; }
    }

    /// <summary>
    /// Identifies and extracts split failure cycle sequences for all approaches and phases at a location.
    /// </summary>
    public class CreateSplitFailCyclesStep : TransformProcessStepBase<Tuple<Location, IEnumerable<IndianaEvent>>, Tuple<Location, IEnumerable<IndianaEvent>, Dictionary<PhaseDetail, List<CycleTimestamps>>>>
    {
        /// <summary>
        /// Initializes a new instance of the CreateSplitFailCyclesStep class with dataflow options.
        /// </summary>
        public CreateSplitFailCyclesStep(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        /// <inheritdoc/>
        protected override Task<Tuple<Location, IEnumerable<IndianaEvent>, Dictionary<PhaseDetail, List<CycleTimestamps>>>> Process(Tuple<Location, IEnumerable<IndianaEvent>> input, CancellationToken cancelToken = default)
        {
            var (location, rawEvents) = input;
            var locationEvents = rawEvents.ToList();

            var phaseService = new PhaseService();
            var phaseDetails = phaseService.GetPhases(location);

            var phaseCycles = new Dictionary<PhaseDetail, List<CycleTimestamps>>();

            if (!locationEvents.Any())
            {
                foreach (var phaseDetail in phaseDetails)
                {
                    phaseCycles.Add(phaseDetail, new List<CycleTimestamps>());
                }
                return Task.FromResult(Tuple.Create(location, rawEvents, phaseCycles));
            }

            foreach (var phaseDetail in phaseDetails)
            {
                var cycleEvents = locationEvents.GetCycleEventsWithTimeExtension(
                    phaseDetail.PhaseNumber,
                    phaseDetail.UseOverlap,
                    locationEvents.Min(e => e.Timestamp),
                    locationEvents.Max(e => e.Timestamp));

                if (cycleEvents == null || !cycleEvents.Any())
                {
                    phaseCycles.Add(phaseDetail, new List<CycleTimestamps>());
                    continue;
                }

                var sortedEvents = cycleEvents.OrderBy(e => e.Timestamp).ToList();

                var cycles = sortedEvents.SlidingWindow(4)
                    .Where(w => IsGreen(w[0].EventCode) &&
                                IsYellow(w[1].EventCode) &&
                                IsRed(w[2].EventCode) &&
                                (IsGreen(w[3].EventCode) || w[3].EventCode == (short)IndianaEnumerations.OverlapDark))
                    .Select(w => new CycleTimestamps
                    {
                        GreenStart = w[0].Timestamp,
                        YellowStart = w[1].Timestamp,
                        RedStart = w[2].Timestamp,
                        GreenEnd = w[3].Timestamp
                    })
                    .ToList();

                phaseCycles.Add(phaseDetail, cycles);
            }

            return Task.FromResult(Tuple.Create(location, rawEvents, phaseCycles));
        }

        private static bool IsGreen(short code) => code == (short)IndianaEnumerations.PhaseBeginGreen || code == (short)IndianaEnumerations.OverlapBeginGreen;
        private static bool IsYellow(short code) => code == (short)IndianaEnumerations.PhaseBeginYellowChange || code == (short)IndianaEnumerations.OverlapBeginYellow;
        private static bool IsRed(short code) => code == (short)IndianaEnumerations.PhaseEndYellowChange || code == (short)IndianaEnumerations.OverlapBeginRedClearance;
    }
}
