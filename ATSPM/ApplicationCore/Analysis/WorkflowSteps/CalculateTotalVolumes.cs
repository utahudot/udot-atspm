#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.WorkflowSteps/CalculateTotalVolumes.cs
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
using ATSPM.Application.Common;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using ATSPM.Domain.Extensions;
using ATSPM.Domain.Workflows;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ATSPM.Application.Extensions;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class CalculateTotalVolumes : TransformProcessStepBase<Tuple<Tuple<Approach, Volumes>, Tuple<Approach, Volumes>>, Tuple<Approach, TotalVolumes>>
    {
        protected override Task<Tuple<Approach, TotalVolumes>> Process(Tuple<Tuple<Approach, Volumes>, Tuple<Approach, Volumes>> input, CancellationToken cancelToken = default)
        {
            var p = input.Item1.Item1;
            var o = input.Item2.Item1;

            if (p.DirectionTypeId == new OpposingDirection(o.DirectionTypeId) && p.Location.LocationIdentifier == o.Location.LocationIdentifier)
            {
                var start1 = input.Item1.Item2.Start;
                var start2 = input.Item2.Item2.Start;



                var volumes = input.Item1.Item2.Segments.Union(input.Item2.Item2.Segments);

                var result = new TotalVolumes(volumes, TimeSpan.FromMinutes(15))
                {
                    LocationIdentifier = p.Location.LocationIdentifier,
                };

                result.Segments.ToList().ForEach(f =>
                {
                    f.LocationIdentifier = p.Location.LocationIdentifier;
                    f.Primary = input.Item1.Item2.Segments.FirstOrDefault(d => f.InRange(d));
                    f.Opposing = input.Item2.Item2.Segments.FirstOrDefault(d => f.InRange(d));
                });

                return Task.FromResult(Tuple.Create(p, result));
            }

            return Task.FromResult(Tuple.Create<Approach, TotalVolumes>(null, null));
        }
    }
}
