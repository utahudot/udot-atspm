#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.WorkflowSteps/GenerateApproachDelayResults.cs
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
using ATSPM.Application.Analysis.ApproachDelay;
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.Plans;
using ATSPM.Application.Analysis.PurdueCoordination;
using ATSPM.Data.Interfaces;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using ATSPM.Domain.Workflows;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class GenerateApproachDelayResults : TransformProcessStepBase<Tuple<Approach, IEnumerable<Vehicle>>, Tuple<Approach, ApproachDelayResult>>
    {
        public GenerateApproachDelayResults(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<Tuple<Approach, ApproachDelayResult>> Process(Tuple<Approach, IEnumerable<Vehicle>> input, CancellationToken cancelToken = default)
        {
            var result = Tuple.Create(input.Item1, new ApproachDelayResult());
            
            //var result = input.Item2.GroupBy(g => g.LocationIdentifier, (Location, x) =>
            //x.GroupBy(g => g.PhaseNumber, (phase, y) =>
            //y.GroupBy(g => g.DetectorChannel, (det, z) => new ApproachDelayResult()
            //{
            //    LocationIdentifier = Location,
            //    PhaseNumber = phase,
            //    Start = z.Min(m => m.Start),
            //    End = z.Max(m => m.End),
            //    Plans = new List<ApproachDelayPlan>() {
            //        new ApproachDelayPlan()
            //        {
            //            LocationIdentifier = Location,
            //            Start = z.Min(m => m.Start),
            //            End = z.Max(m => m.End),
            //            Vehicles = z.ToList()
            //        }
            //    }
            //})).SelectMany(m => m))
            //    .SelectMany(m => m);

            return Task.FromResult(result);
        }
    }
}
