#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/IdentifyTerminationTypesAndTimes.cs
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

namespace Utah.Udot.Atspm.Analysis.WorkflowSteps
{
    public abstract class PhaseTerminationEventBase
    {
        public DateTime StartTime { get; set; }
        public int PhaseNumber { get; set; }
    }

    public class IdentifyTerminationTypesAndTimes : TransformProcessStepBase<Tuple<Approach, int, IEnumerable<IndianaEvent>>, Tuple<Approach, int, PhaseTerminations>>
    {
        private readonly int _consecutiveCounts;

        public IdentifyTerminationTypesAndTimes(int consecutiveCounts = 3, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            _consecutiveCounts = consecutiveCounts;
        }

        protected override Task<Tuple<Approach, int, PhaseTerminations>> Process(Tuple<Approach, int, IEnumerable<IndianaEvent>> input, CancellationToken cancelToken = default)
        {
            var filters = new List<int>()
            {
                4,
                5,
                6,
                (int)IndianaEnumerations.PhaseGreenTermination
            };

            var approach = input.Item1;
            var phase = input.Item2;
            var logs = input.Item3
                .Where(w => w.EventParam == phase)
                .Where(w => filters.Contains(w.EventCode))
                .OrderBy(o => o.Timestamp).ToList();

            //if there are two consecutive IndianaEnumerations.PhaseGreenTermination then the second denotes an unknown termination
            var consecGreenTerminations = logs.GetLastConsecutiveEvent(2).Where(w => w.EventCode == (int)IndianaEnumerations.PhaseGreenTermination).ToList();

            //remove IndianaEnumerations.PhaseGreenTermination and get the consecutive terminations
            var consecTerminations = logs.Where(r => r.EventCode != (int)IndianaEnumerations.PhaseGreenTermination).GetLastConsecutiveEvent(_consecutiveCounts).ToList();

            var terminations = new PhaseTerminations(consecTerminations.Union(consecGreenTerminations))
            {
                LocationIdentifier = approach.Location.LocationIdentifier,
                PhaseNumber = phase,
            };

            var result = Tuple.Create(approach, phase, terminations);

            return Task.FromResult(result);
        }
    }
}
