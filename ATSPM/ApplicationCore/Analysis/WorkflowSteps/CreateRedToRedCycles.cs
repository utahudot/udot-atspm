#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.WorkflowSteps/CreateRedToRedCycles.cs
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
using ATSPM.Application.Analysis.Common;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    /// <summary>
    /// Creates a list of <see cref="RedToRedCycle"/>
    /// <list type="number">
    /// <listheader>Steps to create the <see cref="RedToRedCycle"/></listheader>
    /// 
    /// <item>
    /// <term>Identify the Beginning of Each Cycle</term>
    /// <description>
    /// The beginning of the cycle
    /// for a given phase is defined as the end of <see cref="9"/>. The
    /// event log is queried to find the records where the Event Code is 9. Each instance
    /// of <see cref="9"/> is indicated as the start of the cycle.
    /// </description>
    /// </item>
    /// 
    /// <item>
    /// <term>Identify the Change to Green for Each Cycle</term>
    /// <description>
    /// During this step, the event log is queried to find the records where the Event Code <see cref="1"/>.
    /// The duration from the beginning of the cycle to when the given phasechanges to green(total red interval)
    /// is calculated in reference to the first redevent (begin) of the cycle
    /// </description>
    /// </item>
    /// 
    /// <item>
    /// <term>Identify the Change to Yellow for Each Cycle</term>
    /// <description>
    /// During this step, the event log is queried to find the record where the Event Code <see cref="8"/>.
    /// The duration from the beginning of the cycle to when the given phase
    /// changes to yellow(total green interval) is calculated in reference to the first red event (begin) of the cycle
    /// </description>
    /// </item>
    /// 
    /// <item>
    /// <term>Identify the Change to Red at the End of Each Cycle</term>
    /// <description>
    /// During this step, the event log is queried to find the records where the Event Code <see cref="9"/>. 
    /// The duration from the beginning of the cycle to when the given phase changes to red(yellow clearance interval)
    /// is calculated in reference to the firstred event (begin) of the cycle
    /// </description>
    /// </item>
    /// 
    /// </list>
    /// </summary>
    public class CreateRedToRedCycles : TransformProcessStepBase<Tuple<Approach, IEnumerable<ControllerEventLog>>, Tuple<Approach, IEnumerable<RedToRedCycle>>>
    {
        /// <inheritdoc/>
        public CreateRedToRedCycles(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        /// <inheritdoc/>
        protected override Task<Tuple<Approach, IEnumerable<RedToRedCycle>>> Process(Tuple<Approach, IEnumerable<ControllerEventLog>> input, CancellationToken cancelToken = default)
        {
            //TODO: get the correct phase from approach here
            var result = Tuple.Create(input.Item1, input.Item2?
                .Where(w => w.SignalIdentifier == input?.Item1?.Location.LocationIdentifier)
                .Where(w => w.EventParam == input?.Item1?.ProtectedPhaseNumber)
                .Where(w => w.EventCode == (int)1 || w.EventCode == (int)8 || w.EventCode == (int)9)
                .OrderBy(o => o.Timestamp)
                .GroupBy(g => g.SignalIdentifier, (s, x) => x
                .GroupBy(g => g.EventParam, (p, y) => y
                .Where((w, i) => y.Count() > 3 && i <= y.Count() - 3)
                .Where((w, i) => w.EventCode == 9 && y.ElementAt(i + 1).EventCode == 1 && y.ElementAt(i + 2).EventCode == 8 && y.ElementAt(i + 3).EventCode == 9)
                .Select((s, i) => y.Skip(y.ToList().IndexOf(s)).Take(4))
                .Select(m => new RedToRedCycle()
                {
                    Start = m.ElementAt(0).Timestamp,
                    End = m.ElementAt(3).Timestamp,
                    GreenEvent = m.ElementAt(1).Timestamp,
                    YellowEvent = m.ElementAt(2).Timestamp,
                    PhaseNumber = p,
                    LocationIdentifier = s
                }))
                .SelectMany(m => m))
                .SelectMany(m => m)) ?? Tuple.Create<Approach, IEnumerable<RedToRedCycle>>(null, new List<RedToRedCycle>());

            return Task.FromResult(result);
        }
    }
}
