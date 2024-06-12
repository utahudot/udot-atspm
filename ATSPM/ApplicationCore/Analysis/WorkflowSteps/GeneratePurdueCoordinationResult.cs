#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.WorkflowSteps/GeneratePurdueCoordinationResult.cs
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
using ATSPM.Application.Analysis.PurdueCoordination;
using ATSPM.Domain.Workflows;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    /// <summary>
    /// Generates the <see cref="PurdueCoordinationResult"/> from a list of <see cref="CycleArrivals"/>
    /// </summary>
    public class GeneratePurdueCoordinationResult : TransformManyProcessStepBase<IReadOnlyList<CycleArrivals>, PurdueCoordinationResult>
    {
        /// <inheritdoc/>
        public GeneratePurdueCoordinationResult(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        /// <inheritdoc/>
        protected override Task<IEnumerable<PurdueCoordinationResult>> Process(IReadOnlyList<CycleArrivals> input, CancellationToken cancelToken = default)
        {
            var result = input.GroupBy(g => g.LocationIdentifier, (Location, x) =>
            x.GroupBy(g => g.PhaseNumber, (phase, y) => new PurdueCoordinationResult()
            {
                LocationIdentifier = Location,
                PhaseNumber = phase,
                Start = y.Min(m => m.Start),
                End = y.Max(m => m.End),
                Plans = new List<PurdueCoordinationPlan>() {
                    new PurdueCoordinationPlan()
                    {
                        LocationIdentifier = Location,
                        Start = y.Min(m => m.Start),
                        End = y.Max(m => m.End),
                        ArrivalCycles = y.ToList()
                    }
                }
            })).SelectMany(m => m);

            return Task.FromResult(result);
        }
    }
}
