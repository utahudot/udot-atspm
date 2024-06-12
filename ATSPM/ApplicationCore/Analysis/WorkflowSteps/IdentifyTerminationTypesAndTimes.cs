#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.WorkflowSteps/IdentifyTerminationTypesAndTimes.cs
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
using ATSPM.Data.Interfaces;
using ATSPM.Data.Models;
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
    public class IdentifyTerminationTypesAndTimes : TransformProcessStepBase<Tuple<Approach, int, IEnumerable<ControllerEventLog>>, Tuple<Approach, int, PhaseTerminations>>
    {
        private readonly int _consecutiveCounts;

        public IdentifyTerminationTypesAndTimes(int consecutiveCounts = 3, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) 
        {
            _consecutiveCounts = consecutiveCounts;
        }

        protected override Task<Tuple<Approach, int, PhaseTerminations>> Process(Tuple<Approach, int, IEnumerable<ControllerEventLog>> input, CancellationToken cancelToken = default)
        {
            var filters = new List<int>()
            {
                (int)4,
                (int)5,
                (int)6,
                (int)IndianaEnumerations.PhaseGreenTermination
            };
            
            var approach = input.Item1;
            var phase = input.Item2;
            var logs = input.Item3
                .Where(w => w.EventParam == phase)
                .Where(w => filters.Contains(w.EventCode))
                .OrderBy(o => o.Timestamp).ToList();

            //if there are two consecutive )IndianaEnumerations.PhaseGreenTermination then the second denotes an unknown termination
            var consecGreenTerminations = logs.GetLastConsecutiveEvent(2).Where(w => w.EventCode == (int)IndianaEnumerations.PhaseGreenTermination).ToList();

            //remove IndianaEnumerations.PhaseGreenTermination and get the consecutive terminations
            var consecTerminations = logs.Where(r => r.EventCode != (int)IndianaEnumerations.PhaseGreenTermination).GetLastConsecutiveEvent(_consecutiveCounts).ToList();

            var stuff = new PhaseTerminations(consecTerminations.Union(consecGreenTerminations))
            {
                LocationIdentifier = approach.Location.LocationIdentifier,
                PhaseNumber = phase,
            };

            var result = Tuple.Create(approach, phase, stuff);

            return Task.FromResult(result);
        }
    }
}
