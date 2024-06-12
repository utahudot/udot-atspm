#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.WorkflowSteps/CalculatePhaseVolume.cs
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
using ATSPM.Application.Extensions;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
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
    public class CalculatePhaseVolume : TransformProcessStepBase<Tuple<Approach, IEnumerable<CorrectedDetectorEvent>>, Tuple<Approach, Volumes>>
    {
        public CalculatePhaseVolume(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<Tuple<Approach, Volumes>> Process(Tuple<Approach, IEnumerable<CorrectedDetectorEvent>> input, CancellationToken cancelToken = default)
        {
            var eventFilter = input.FilterCorrectedDetectorEvents();

            if (eventFilter == null || eventFilter.Count == 0)
                return Task.FromResult(Tuple.Create<Approach, Volumes>(input.Item1, null));

            var result = Tuple.Create(input.Item1, new Volumes(eventFilter, TimeSpan.FromMinutes(15))
            {
                LocationIdentifier = input.Item1.Location.LocationIdentifier,
                PhaseNumber = input.Item1.ProtectedPhaseNumber,
                Direction = input.Item1.DirectionTypeId
            });

            result.Item2.Segments.ToList().ForEach((f =>
            {
                f.LocationIdentifier = input.Item1?.Location.LocationIdentifier;
                f.PhaseNumber = input.Item1?.ProtectedPhaseNumber ?? 0;
                f.Direction = input.Item1.DirectionTypeId;
                f.DetectorEvents.AddRange(eventFilter.Where(w => f.InRange(w)));
            }));

            return Task.FromResult(result);
        }
    }
}
