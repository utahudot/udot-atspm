#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/PreemptiveProcessBase.cs
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
using Utah.Udot.Atspm.Analysis.PreemptionDetails;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Analysis.WorkflowSteps
{
    public abstract class PreemptiveProcessBase<T> : TransformManyProcessStepBase<Tuple<Location, IEnumerable<IndianaEvent>, int>, T> where T : PreempDetailValueBase, new()
    {
        protected short first;
        protected short second;

        public PreemptiveProcessBase(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<T>> Process(Tuple<Location, IEnumerable<IndianaEvent>, int> input, CancellationToken cancelToken = default)
        {
            var result = input.Item2
                .Where(w => w.LocationIdentifier == input.Item1.LocationIdentifier)
                .Where(w => w.EventParam == input.Item3)
                .TimeSpanFromConsecutiveCodes(first, second)
                .Select(s => new T()
                {
                    LocationIdentifier = input.Item1.LocationIdentifier,
                    PreemptNumber = input.Item3,
                    Start = s.Item1[0].Timestamp,
                    End = s.Item1[1].Timestamp,
                    Seconds = s.Item2
                });

            return Task.FromResult(result);
        }
    }
}
