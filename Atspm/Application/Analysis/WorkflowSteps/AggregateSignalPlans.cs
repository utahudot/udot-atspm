#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/AggregateSignalPlans.cs
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
using Utah.Udot.Atspm.Analysis.Plans;

namespace Utah.Udot.Atspm.Analysis.WorkflowSteps
{
    public class AggregateLocationPlans : TransformProcessStepBase<Tuple<Location, int, IEnumerable<Plan>>, IEnumerable<SignalPlanAggregation>>
    {
        public AggregateLocationPlans(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<SignalPlanAggregation>> Process(Tuple<Location, int, IEnumerable<Plan>> input, CancellationToken cancelToken = default)
        {
            var result = input.Item3
                .Select(s => new SignalPlanAggregation()
                {
                    LocationIdentifier = s.LocationIdentifier,
                    PlanNumber = s.PlanNumber,
                    Start = s.Start,
                    End = s.End,
                });

            return Task.FromResult(result);
        }
    }
}
