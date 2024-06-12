#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.WorkflowSteps/GroupApproachesByPhase.cs
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
using ATSPM.Application.Specifications;
using ATSPM.Data.Models;
using ATSPM.Domain.Extensions;
using ATSPM.Domain.Workflows;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class GroupApproachesByPhase : TransformManyProcessStepBase<Tuple<Approach, IEnumerable<ControllerEventLog>>, Tuple<Approach, int, IEnumerable<ControllerEventLog>>>
    {
        public GroupApproachesByPhase(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<Tuple<Approach, int, IEnumerable<ControllerEventLog>>>> Process(Tuple<Approach, IEnumerable<ControllerEventLog>> input, CancellationToken cancelToken = default)
        {
            var result = input.Item2
                .GroupBy(g => g.EventParam, (phase, i) => Tuple.Create(input.Item1, phase, i))
                .Where(w => w.Item1.ProtectedPhaseNumber == w.Item2);

            return Task.FromResult(result);
        }
    }
}
