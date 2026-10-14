#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/CalculateSplitFailOccupancyStep.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Business.Common;

namespace Utah.Udot.Atspm.Analysis.WorkflowSteps
{
    /// <summary>
    /// Represents the evaluated metrics and split-failure status for an individual cycle.
    /// </summary>
    public class SplitFailCycleResult
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

        /// <summary>
        /// Gets or sets the duration of detector occupancy during green, in seconds.
        /// </summary>
        public double GreenOccupancySeconds { get; set; }

        /// <summary>
        /// Gets or sets the duration of detector occupancy during the first 5 seconds of red, in seconds.
        /// </summary>
        public double RedOccupancySeconds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this cycle represents a split failure.
        /// </summary>
        public bool IsSplitFailure { get; set; }

        /// <summary>
        /// Gets the total duration of the green interval, in seconds.
        /// </summary>
        public double GreenDuration => (YellowStart - GreenStart).TotalSeconds;
    }

    /// <summary>
    /// Calculates detector occupancy for green and red intervals of each cycle, identifying split failures.
    /// </summary>
    public class CalculateSplitFailOccupancyStep : TransformProcessStepBase<Tuple<Location, Dictionary<PhaseDetail, List<CycleTimestamps>>, Dictionary<PhaseDetail, List<Tuple<DateTime, DateTime>>>>, Tuple<Location, Dictionary<PhaseDetail, List<SplitFailCycleResult>>>>
    {
        private readonly double _redAnalysisSeconds;

        /// <summary>
        /// Initializes a new instance of the CalculateSplitFailOccupancyStep class with dataflow options.
        /// </summary>
        public CalculateSplitFailOccupancyStep(double redAnalysisSeconds = 5.0, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            _redAnalysisSeconds = redAnalysisSeconds;
        }

        /// <inheritdoc/>
        protected override Task<Tuple<Location, Dictionary<PhaseDetail, List<SplitFailCycleResult>>>> Process(Tuple<Location, Dictionary<PhaseDetail, List<CycleTimestamps>>, Dictionary<PhaseDetail, List<Tuple<DateTime, DateTime>>>> input, CancellationToken cancelToken = default)
        {
            var (location, phaseCycles, phasePresence) = input;

            var phaseResults = new Dictionary<PhaseDetail, List<SplitFailCycleResult>>();

            foreach (var phaseDetail in phaseCycles.Keys)
            {
                var cycles = phaseCycles[phaseDetail];
                var intervals = phasePresence[phaseDetail];

                var results = new List<SplitFailCycleResult>();

                foreach (var cycle in cycles)
                {
                    var greenOccupancy = AtspmMath.GetIntersectionDuration(cycle.GreenStart, cycle.YellowStart, intervals);
                    var redOccupancy = AtspmMath.GetIntersectionDuration(cycle.RedStart, cycle.RedStart.AddSeconds(_redAnalysisSeconds), intervals);

                    var greenDuration = (cycle.YellowStart - cycle.GreenStart).TotalSeconds;
                    var greenOccupancyPercent = greenDuration > 0 ? (greenOccupancy / greenDuration) * 100 : 0;
                    var redOccupancyPercent = _redAnalysisSeconds > 0 ? (redOccupancy / _redAnalysisSeconds) * 100 : 0;

                    var isSplitFail = greenOccupancyPercent > 79 && redOccupancyPercent > 79;

                    results.Add(new SplitFailCycleResult
                    {
                        GreenStart = cycle.GreenStart,
                        YellowStart = cycle.YellowStart,
                        RedStart = cycle.RedStart,
                        GreenEnd = cycle.GreenEnd,
                        GreenOccupancySeconds = greenOccupancy,
                        RedOccupancySeconds = redOccupancy,
                        IsSplitFailure = isSplitFail
                    });
                }

                phaseResults.Add(phaseDetail, results);
            }

            return Task.FromResult(Tuple.Create(location, phaseResults));
        }
    }
}
