#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.WorkflowSteps/GroupSignalByParameter.cs
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class GroupLocationByParameter : TransformManyProcessStepBase<Tuple<Location, IEnumerable<ControllerEventLog>>, Tuple<Location, int, IEnumerable<ControllerEventLog>>>
    {
        public GroupLocationByParameter(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<Tuple<Location, int, IEnumerable<ControllerEventLog>>>> Process(Tuple<Location, IEnumerable<ControllerEventLog>> input, CancellationToken cancelToken = default)
        {
            var result = input.Item2
                .FromSpecification(new ControllerLogLocationFilterSpecification(input.Item1))
                .GroupBy(g => g.EventParam)
                .Select(s => Tuple.Create(input.Item1, s.Key, s.AsEnumerable()));

            return Task.FromResult(result);
        }
    }
}
