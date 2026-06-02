#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.WorkflowSteps/ReconcileSignalPlansStep.cs
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

using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.NetStandardToolkit.Workflows;

namespace Utah.Udot.Atspm.Infrastructure.WorkflowSteps
{
    /// <summary>
    /// A workflow step that reconciles timing plans by setting the end time of a plan to the start time of the subsequent plan.
    /// </summary>
    public class ReconcileSignalPlansStep(ExecutionDataflowBlockOptions options) : TransformManyProcessStepBaseAsync<IEnumerable<SignalTimingPlan>, IEnumerable<SignalTimingPlan>>(options)
    {
        protected override async IAsyncEnumerable<IEnumerable<SignalTimingPlan>> Process(IEnumerable<SignalTimingPlan> input, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            var groups = input.GroupBy(g => g.LocationIdentifier);

            foreach (var g in groups)
            {
                var ordered = g.OrderBy(p => p.Start).ToList();

                var finalized = ordered.Zip(ordered.Skip(1).Append(null), (current, next) =>
                {
                    current.End = next?.Start ?? DateTime.MinValue;
                    return current;
                });

                yield return finalized;
            }
        }
    }
}
