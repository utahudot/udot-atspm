#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.WorkflowSteps/AssignRangeToPlan.cs
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
using ATSPM.Application.Analysis.Plans;
using ATSPM.Data.Interfaces;
using ATSPM.Domain.Common;
using ATSPM.Domain.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class AssignRangeToPlan<T> : TransformProcessStepBase<Tuple<IReadOnlyList<T>, IReadOnlyList<IStartEndRange>>, IReadOnlyList<T>> where T : IPlan, new()
    {
        public AssignRangeToPlan(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IReadOnlyList<T>> Process(Tuple<IReadOnlyList<T>, IReadOnlyList<IStartEndRange>> input, CancellationToken cancelToken = default)
        {
            List<T> plans;

            if (input.Item1.Count == 0)
            {
                plans = input.Item2.Cast<ILocationLayer>().GroupBy(g => g.LocationIdentifier, (s, i) => new T()
                {
                    LocationIdentifier = s,
                    Start = i.Cast<IStartEndRange>().Min(m => m.Start),
                    End = i.Cast<IStartEndRange>().Max(m => m.End)
                }).ToList();
            }
            else
            {
                plans = input.Item1.ToList();
            }

            foreach (var p in plans)
            {
                p.AssignToPlan(input.Item2);
            }

            return Task.FromResult<IReadOnlyList<T>>(plans);
        }
    }
}
