#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/IdentifyTerminationTypesAndTimes.cs
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
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace Utah.Udot.Atspm.Analysis.WorkflowSteps
{
    /// <summary>
    /// A consolidated workflow step that aggregates phase termination events from raw event logs
    /// into <see cref="PhaseTerminationAggregation"/> results segmented by timeline.
    /// </summary>
    /// <remarks>
    /// This step replaces multiple legacy steps (<c>GroupPhaseTerminationsByApproaches</c>, 
    /// <c>IdentifyTerminationTypesAndTimes</c>, and <c>AggregatePhaseTerminationEvents</c>)
    /// into a single, high-performance, allocation-optimized process.
    /// It implements a parallelized LINQ stream over approaches to compute counts for Gap Outs, 
    /// Max Outs, Force Offs, and Unknown terminations.
    /// </remarks>
    public class AggregatePhaseTerminationStep(Timeline<StartEndRange> timeline, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : TransformProcessStepBase<Tuple<Location, IEnumerable<IndianaEvent>>, IEnumerable<PhaseTerminationAggregation>>(dataflowBlockOptions)
    {
        private readonly Timeline<StartEndRange> _timeline = timeline;

        /// <summary>
        /// Processes raw location event logs by filtering and grouping them into chronological segment buckets.
        /// </summary>
        /// <param name="input">A tuple containing the target <see cref="Location"/> and its associated raw <see cref="IndianaEvent"/> logs.</param>
        /// <param name="cancelToken">A cancellation token used to cancel the process if requested.</param>
        /// <returns>A collection of high-fidelity <see cref="PhaseTerminationAggregation"/> results segmented by the provided timeline.</returns>
        protected override Task<IEnumerable<PhaseTerminationAggregation>> Process(Tuple<Location, IEnumerable<IndianaEvent>> input, CancellationToken cancelToken = default)
        {
            var (location, rawEvents) = input;

            var locationEvents = rawEvents
                .FromSpecification(new EventLogSpecification(location))
                .Cast<IndianaEvent>()
                .ToList();

            var filters = new List<short>
        {
            (short)IndianaEnumerations.PhaseGapOut,
            (short)IndianaEnumerations.PhaseMaxOut,
            (short)IndianaEnumerations.PhaseForceOff,
            (short)IndianaEnumerations.PhaseGreenTermination
        };

            var result = location.Approaches
                .AsParallel()
                .SelectMany(approach =>
                {
                    var phase = approach.ProtectedPhaseNumber;

                    var logs = locationEvents
                        .Where(w => w.EventParam == phase)
                        .Where(w => filters.Contains(w.EventCode))
                        .OrderBy(o => o.Timestamp)
                        .ToList();

                    var consecGreenTerminations = logs
                        .Where((w, i) => w.EventCode == (int)IndianaEnumerations.PhaseGreenTermination &&
                                         i < logs.Count - 1 &&
                                         logs[i + 1].EventCode == (int)IndianaEnumerations.PhaseGreenTermination)
                        .ToList();

                    var nonGreenLogs = logs
                        .Where(r => r.EventCode != (int)IndianaEnumerations.PhaseGreenTermination)
                        .ToList();

                    var consecTerminations = nonGreenLogs.GetLastConsecutiveEvent(3);

                    var combinedTerminations = consecTerminations.Concat(consecGreenTerminations).ToList();

                    var gapOuts = combinedTerminations.Where(e => e.EventCode == (int)IndianaEnumerations.PhaseGapOut).ToList();
                    var maxOuts = combinedTerminations.Where(e => e.EventCode == (int)IndianaEnumerations.PhaseMaxOut).ToList();
                    var forceOffs = combinedTerminations.Where(e => e.EventCode == (int)IndianaEnumerations.PhaseForceOff).ToList();
                    var unknown = combinedTerminations.Where(e => e.EventCode == (int)IndianaEnumerations.PhaseGreenTermination).ToList();

                    return _timeline.Segments.Select(segment => new PhaseTerminationAggregation
                    {
                        Start = segment.Start,
                        End = segment.End,
                        LocationIdentifier = location.LocationIdentifier,
                        PhaseNumber = phase,
                        GapOuts = gapOuts.Count(c => segment.InRange(c)),
                        ForceOffs = forceOffs.Count(c => segment.InRange(c)),
                        MaxOuts = maxOuts.Count(c => segment.InRange(c)),
                        Unknown = unknown.Count(c => segment.InRange(c))
                    });
                })
                .ToList()
                .AsEnumerable();

            return Task.FromResult(result);
        }
    }
}
