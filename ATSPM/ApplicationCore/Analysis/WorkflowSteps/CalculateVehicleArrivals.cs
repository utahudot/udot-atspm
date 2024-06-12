#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.WorkflowSteps/CalculateVehicleArrivals.cs
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
using ATSPM.Domain.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    /// <summary>
    /// Creates a list of <see cref="CycleArrivals"/>
    /// <list type="number">
    /// <listheader>Steps to create the <see cref="CycleArrivals"/></listheader>
    /// 
    /// <item>
    /// Identify <see cref="CorrectedDetectorEvent"/> arriving durring the green stage of <see cref="RedToRedCycle"/>
    /// </item>
    /// 
    /// <item>
    /// Identify <see cref="CorrectedDetectorEvent"/> arriving durring the yellow stage of <see cref="RedToRedCycle"/>
    /// </item>
    /// 
    /// <item>
    /// Identify <see cref="CorrectedDetectorEvent"/> arriving durring the red stage of <see cref="RedToRedCycle"/>
    /// </item>
    /// 
    /// </list>
    /// </summary>
    public class CalculateVehicleArrivals : TransformProcessStepBase<Tuple<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>, IReadOnlyList<CycleArrivals>>
    {
        /// <inheritdoc/>
        public CalculateVehicleArrivals(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        /// <inheritdoc/>
        protected override Task<IReadOnlyList<CycleArrivals>> Process(Tuple<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>> input, CancellationToken cancelToken = default)
        {
            //had to remove this to compile
            //var result = input.Item2.Select(s => new CycleArrivals(s)
            //{
            //    Vehicles = input.Item1.Where(w => w.Detector.Approach?.Location?.LocationIdentifier == s.LocationIdentifier && s.InRange(w.CorrectedTimeStamp))
            //    .Select(v => new Vehicle(v, s))
            //    .ToList()
            //}).ToList();

            //this is temp
            var result = new List<CycleArrivals>();

            return Task.FromResult<IReadOnlyList<CycleArrivals>>(result);
        }
    }
}
