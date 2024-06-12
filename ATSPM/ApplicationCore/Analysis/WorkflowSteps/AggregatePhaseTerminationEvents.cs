#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.WorkflowSteps/AggregatePhaseTerminationEvents.cs
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
using ATSPM.Data.Models;
using ATSPM.Data.Models.AggregationModels;
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
    public class AggregatePhaseTerminationEvents : TransformProcessStepBase<Tuple<Approach, int, PhaseTerminations>, IEnumerable<PhaseTerminationAggregation>>
    {
        /// <inheritdoc/>
        public AggregatePhaseTerminationEvents(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        /// <inheritdoc/>
        protected override Task<IEnumerable<PhaseTerminationAggregation>> Process(Tuple<Approach, int, PhaseTerminations> input, CancellationToken cancelToken = default)
        {
            var approach = input.Item1;
            var phase = input.Item2;
            var events = input.Item3;

            var tl = new Timeline<PhaseTerminationAggregation>(events.TerminationEvents, TimeSpan.FromMinutes(15));

            tl.Segments.ToList().ForEach(f =>
            {
                f.LocationIdentifier = approach.Location.LocationIdentifier;
                f.PhaseNumber = phase;
                f.GapOuts = events.GapOuts.Count(c => f.InRange(c));
                f.ForceOffs = events.ForceOffs.Count(c => f.InRange(c));
                f.MaxOuts = events.MaxOuts.Count(c => f.InRange(c));
                f.Unknown = events.UnknownTerminations.Count(c => f.InRange(c));
            });

            //TODO: change this and all aggregations to ireadonlylist
            var result = tl.Segments.AsEnumerable();

            return Task.FromResult(result);
        }
    }
}
