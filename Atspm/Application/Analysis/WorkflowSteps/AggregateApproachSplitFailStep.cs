#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/AggregateApproachSplitFailStep.cs
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
using Utah.Udot.Atspm.Data.Models;

namespace Utah.Udot.Atspm.Analysis.WorkflowSteps
{
    /// <summary>
    /// Groups evaluated cycles into defined timeline segments to produce the final database-ready aggregations.
    /// </summary>
    public class AggregateApproachSplitFailStep : TransformProcessStepBase<Tuple<Location, Dictionary<PhaseDetail, List<SplitFailCycleResult>>>, IEnumerable<ApproachSplitFailAggregation>>
    {
        private readonly Timeline<StartEndRange> _timeline;

        /// <summary>
        /// Initializes a new instance of the AggregateApproachSplitFailStep class with the timeline and dataflow options.
        /// </summary>
        public AggregateApproachSplitFailStep(Timeline<StartEndRange> timeline, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            _timeline = timeline;
        }

        /// <inheritdoc/>
        protected override Task<IEnumerable<ApproachSplitFailAggregation>> Process(Tuple<Location, Dictionary<PhaseDetail, List<SplitFailCycleResult>>> input, CancellationToken cancelToken = default)
        {
            var (location, phaseResults) = input;

            var aggregations = new List<ApproachSplitFailAggregation>();

            foreach (var phaseDetail in phaseResults.Keys)
            {
                var cycles = phaseResults[phaseDetail];

                foreach (var segment in _timeline.Segments)
                {
                    var segmentCycles = cycles.Where(c => segment.InRange(c.GreenStart)).ToList();

                    var agg = new ApproachSplitFailAggregation
                    {
                        Start = segment.Start,
                        End = segment.End,
                        LocationIdentifier = location.LocationIdentifier,
                        ApproachId = phaseDetail.Approach.Id,
                        PhaseNumber = phaseDetail.PhaseNumber,
                        IsProtectedPhase = !phaseDetail.IsPermissivePhase,
                        Cycles = segmentCycles.Count,
                        SplitFailures = segmentCycles.Count(c => c.IsSplitFailure),
                        GreenOccupancySum = (int)Math.Round(segmentCycles.Sum(c => c.GreenOccupancySeconds), MidpointRounding.AwayFromZero),
                        RedOccupancySum = (int)Math.Round(segmentCycles.Sum(c => c.RedOccupancySeconds), MidpointRounding.AwayFromZero),
                        GreenTimeSum = (int)Math.Round(segmentCycles.Sum(c => c.GreenDuration), MidpointRounding.AwayFromZero),
                        RedTimeSum = segmentCycles.Count * 5
                    };

                    aggregations.Add(agg);
                }
            }

            return Task.FromResult(aggregations.AsEnumerable());
        }
    }
}
