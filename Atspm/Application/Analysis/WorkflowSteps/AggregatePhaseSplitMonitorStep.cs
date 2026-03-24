#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/AggregatePhaseSplitMonitorStep.cs
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

using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Extensions;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace Utah.Udot.Atspm.Analysis.WorkflowSteps
{
    /// <summary>
    /// A workflow step that aggregates phase split monitor data from 
    /// <see cref="IndianaEvent"/> logs into <see cref="PhaseSplitMonitorAggregation"/> results.
    /// </summary>
    /// <remarks>
    /// This step processes raw event logs for a given location, identifies phase start–end
    /// intervals based on <see cref="IndianaEnumerations.PhaseBeginGreen"/> and 
    /// <see cref="IndianaEnumerations.PhaseEndRedClearance"/> transitions, and computes 
    /// aggregated statistics such as cycle counts, 85th percentile split durations, and 
    /// skipped phase counts.
    /// Results are grouped by phase number and segmented according to the provided 
    /// <see cref="Timeline{T}"/>.
    /// </remarks>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AggregatePhaseSplitMonitorStep"/> class
    /// with the specified timeline and dataflow block options.
    /// </remarks>
    /// <param name="timeline">
    /// The timeline used to segment aggregation results into defined start and end ranges.
    /// </param>
    /// <param name="dataflowBlockOptions">
    /// Options that configure execution behavior of the dataflow block, such as cancellation 
    /// and parallelism. Defaults to <c>null</c> if not provided.
    /// </param>
    public class AggregatePhaseSplitMonitorStep(Timeline<StartEndRange> timeline, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : TransformProcessStepBase<Tuple<Location, IEnumerable<IndianaEvent>>, IEnumerable<PhaseSplitMonitorAggregation>>(dataflowBlockOptions)
    {
        private readonly Timeline<StartEndRange> _timeline = timeline;

        /// <summary>
        /// Processes the input tuple by aggregating phase split monitor data for the given location.
        /// </summary>
        /// <param name="input">
        /// A tuple containing the <see cref="Location"/> and a collection of 
        /// <see cref="IndianaEvent"/> instances to be analyzed.
        /// </param>
        /// <param name="cancelToken">
        /// A cancellation token used to cancel the operation if requested.
        /// </param>
        /// <returns>
        /// A task that produces a collection of <see cref="PhaseSplitMonitorAggregation"/> results,
        /// segmented by the provided timeline.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The method identifies phase start–end pairs by scanning for event code sequences 
        /// corresponding to <see cref="IndianaEnumerations.PhaseBeginGreen"/> followed by 
        /// <see cref="IndianaEnumerations.PhaseEndRedClearance"/>. These represent the 
        /// beginning and end of individual phase split intervals.
        /// </para>
        /// <para>
        /// For each timeline segment, the method computes:
        /// <list type="bullet">
        /// <item><description>The number of phase splits beginning within the segment.</description></item>
        /// <item><description>The 85th percentile of split durations (in seconds), or <c>-1</c> when no cycles occur.</description></item>
        /// <item><description>The skipped count, calculated as the difference between the 
        /// maximum cycle count across all phases for the segment and the count for the 
        /// current phase.</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// The results are ordered by phase number and segment start time before being returned.
        /// </para>
        /// </remarks>
        protected override Task<IEnumerable<PhaseSplitMonitorAggregation>> Process(Tuple<Location, IEnumerable<IndianaEvent>> input, CancellationToken cancelToken = default)
        {
            var (location, rawEvents) = input;

            var cycleStarts = rawEvents
                .Where(w => w.EventCode == (short)IndianaEnumerations.PhaseBeginGreen || w.EventCode == (short)IndianaEnumerations.PhaseEndRedClearance)
                .OrderBy(o => o.Timestamp)
                .GroupBy(g => g.EventParam)
                .SelectMany(g =>
                g.SlidingWindow(2)
                .Where(window => window.Select(e => e.EventCode).SequenceEqual(new List<short>()
                {
                    (short)IndianaEnumerations.PhaseBeginGreen,
                    (short)IndianaEnumerations.PhaseEndRedClearance
                }))
                .Select(w => new
                {
                    PhaseNumber = g.Key,
                    Start = w[0].Timestamp,
                    End = w[1].Timestamp
                }))
                .GroupBy(x => x.PhaseNumber)
                .ToDictionary(g => g.Key, g => g);

            var bin = location.Approaches
                .AsParallel()
                .SelectMany(approach =>
                {
                    var phase = approach.ProtectedPhaseNumber;

                    var phaseCycles = cycleStarts.TryGetValue((short)phase, out var cycles) ? cycles : Enumerable.Empty<dynamic>();

                    return _timeline.Segments.Select(s =>
                    {
                        var durations = phaseCycles
                            .Where(w => s.InRange(w.Start))
                            .Select(x => (x.End - x.Start).TotalSeconds)
                            .Cast<double>()
                            .ToList();

                        var count = durations.Count;

                        var agg = new PhaseSplitMonitorAggregation
                        {
                            LocationIdentifier = location.LocationIdentifier,
                            PhaseNumber = phase,
                            Start = s.Start,
                            End = s.End,
                            EightyFifthPercentileSplit = count == 0 ? -1 : Math.Round(AtspmMath.Percentile(durations, 85), 1, MidpointRounding.AwayFromZero)
                        };

                        return (agg, count);
                    });
                })
                .ToList();

            var maxCounts = bin.GroupBy(g => g.agg.Start).ToDictionary(d => d.Key, d => d.Max(x => x.count));

            foreach (var (agg, count) in bin)
            {
                agg.SkippedCount = maxCounts[agg.Start] - count;
            }

            var result = bin
                .Select(s => s.agg)
                .OrderBy(o => o.PhaseNumber)
                .ThenBy(t => t.Start)
                .ToList()
                .AsEnumerable();

            return Task.FromResult(result);
        }
    }
}