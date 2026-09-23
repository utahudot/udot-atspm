#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.WorkflowSteps/GenerateSignalPlansStep.cs
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
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.NetStandardToolkit.Workflows;

namespace Utah.Udot.Atspm.Infrastructure.WorkflowSteps
{
    /// <summary>
    /// A workflow step that groups Indiana events by location and parameter to generate initial signal timing plans.
    /// </summary>
    public class GenerateSignalPlansStep(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : TransformManyProcessStepBaseAsync<IEnumerable<IndianaEvent>, IEnumerable<SignalTimingPlan>>(dataflowBlockOptions)
    {
        protected override async IAsyncEnumerable<IEnumerable<SignalTimingPlan>> Process(IEnumerable<IndianaEvent> input, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            var groups = input
                .FromSpecification(new IndianaPlanDataSpecification())
                .GroupBy(e => (e.LocationIdentifier, e.EventParam));

            foreach (var g in groups)
            {
                var unique = g.KeepFirstSequentialParam().ToList();

                if (unique.Count > 0)
                {
                    var chunk = unique.Select(s => new SignalTimingPlan
                    {
                        LocationIdentifier = s.LocationIdentifier,
                        PlanNumber = s.EventParam,
                        Start = s.Timestamp,
                        End = DateTime.MinValue
                    }).ToList();

                    yield return chunk;
                }
            }
        }
    }
}
