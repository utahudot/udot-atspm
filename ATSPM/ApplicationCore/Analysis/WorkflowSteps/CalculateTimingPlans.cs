#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.WorkflowSteps/CalculateTimingPlans.cs
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
using ATSPM.Application.Analysis.Plans;
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
    public class CalculateTimingPlans<T> : TransformProcessStepBase<Tuple<Location, int, IEnumerable<ControllerEventLog>>, Tuple<Location, int, IEnumerable<T>>> where T : IPlan, new()
    {
        public CalculateTimingPlans(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<Tuple<Location, int, IEnumerable<T>>> Process(Tuple<Location, int, IEnumerable<ControllerEventLog>> input, CancellationToken cancelToken = default)
        {
            var result = Tuple.Create(input.Item1, input.Item2, input.Item3
                .GroupBy(g => g.EventCode, (k, l) => 
                l.Select((s, i) => new T()
                {
                    LocationIdentifier = input.Item1.LocationIdentifier,
                    PlanNumber = input.Item2,
                    Start = l.ElementAt(i).Timestamp,
                    End = i < l.Count() - 1 ? l.ElementAt(i + 1).Timestamp : default
                }))
                .SelectMany(m => m));

            return Task.FromResult(result);
        }
    }
}
