#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/GroupPhaseTerminationsByApproaches.cs
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
    /// Breaks out all the Phase numbers from <see cref="Approach"/>
    /// and returns separate Tuples of <see cref="Approach"/>/<see cref="Approach.ProtectedPhaseNumber"/>/<see cref="IEnumerable{IndianaEvent}"/> sets
    /// where the <see cref="IndianaEvent.EventCode"/> is equal to one of the following:
    /// <list type="bullet">
    /// <item><see cref="IndianaEnumerations.PhaseGapOut"/></item>
    /// <item><see cref="IndianaEnumerations.PhaseMaxOut"/></item>
    /// <item><see cref="IndianaEnumerations.PhaseForceOff"/></item>
    /// <item><see cref="IndianaEnumerations.PhaseGreenTermination"/></item>
    /// </list>
    /// and the <see cref="IndianaEvent.EventParam"/> is equal to <see cref="Approach.ProtectedPhaseNumber"/>
    /// </summary>
    public class GroupPhaseTerminationsByApproaches : TransformManyProcessStepBase<Tuple<Approach, IEnumerable<IndianaEvent>>, Tuple<Approach, int, IEnumerable<IndianaEvent>>>
    {
        /// <inheritdoc/>
        public GroupPhaseTerminationsByApproaches(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        /// <inheritdoc/>
        protected override Task<IEnumerable<Tuple<Approach, int, IEnumerable<IndianaEvent>>>> Process(Tuple<Approach, IEnumerable<IndianaEvent>> input, CancellationToken cancelToken = default)
        {
            var approach = input.Item1;
            var events = input.Item2
                .Where(w => w.EventCode >= (int)IndianaEnumerations.PhaseGapOut && w.EventCode <= (int)IndianaEnumerations.PhaseGreenTermination)
                .FromSpecification(new EventLogSpecification(approach?.Location))
                .Cast<IndianaEvent>()
                .ToList();

            var result = input.Item2.GroupBy(g => g.EventParam, (phase, i) => Tuple.Create(input.Item1, Convert.ToInt32(phase), i.Where(w => w.EventParam == phase)));

            return Task.FromResult(result);
        }
    }
}
