#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.WorkflowSteps/PreemptiveProcessBase.cs
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
using ATSPM.Application.Analysis.PreemptionDetails;
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
    public abstract class PreemptiveProcessBase<T> : TransformManyProcessStepBase<Tuple<Location, IEnumerable<ControllerEventLog>, int>, T> where T : PreempDetailValueBase, new()
    {
        protected short first;
        protected short second;

        public PreemptiveProcessBase(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<T>> Process(Tuple<Location, IEnumerable<ControllerEventLog>, int> input, CancellationToken cancelToken = default)
        {
            //var result = Tuple.Create(input.Item1, input.Item2
            //    .Where(w => w.LocationIdentifier == input.Item1.LocationIdentifier)
            //    .Where(w => w.EventParam == input.Item3)
            //    .TimeSpanFromConsecutiveCodes(first, second)
            //    .Select(s => new T()
            //    {
            //        LocationIdentifier = input.Item1.LocationIdentifier,
            //        PreemptNumber = input.Item3,
            //        Start = s.Item1[0].Timestamp,
            //        End = s.Item1[1].Timestamp,
            //        Seconds = s.Item2
            //    }), input.Item3);

            var result = input.Item2
                .Where(w => w.SignalIdentifier == input.Item1.LocationIdentifier)
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

    //public abstract class PreemptiveProcessBase<T> : TransformManyProcessStepBase<IEnumerable<ControllerEventLog>, IEnumerable<T>> where T : PreempDetailValueBase, new()
    //{
    //    protected IndianaEnumerations first;
    //    protected IndianaEnumerations second;

    //    public PreemptiveProcessBase(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

    //    protected override Task<IEnumerable<IEnumerable<T>>> Process(IEnumerable<ControllerEventLog> input, CancellationToken cancelToken = default)
    //    {
    //        var result = input.GroupBy(g => g.LocationIdentifier)
    //            .SelectMany(s => s.GroupBy(g => g.EventParam)
    //            .Select(s => s.TimeSpanFromConsecutiveCodes(first, second)
    //            .Select(s => new T()
    //            {
    //                LocationIdentifier = s.Item1[0].LocationIdentifier == s.Item1[1].LocationIdentifier ? s.Item1[0].LocationIdentifier : string.Empty,
    //                PreemptNumber = Convert.ToInt32(s.Item1.Average(a => a.EventParam)),
    //                Start = s.Item1[0].Timestamp,
    //                End = s.Item1[1].Timestamp,
    //                Seconds = s.Item2
    //            })));

    //        return Task.FromResult(result);
    //    }
    //}
}
