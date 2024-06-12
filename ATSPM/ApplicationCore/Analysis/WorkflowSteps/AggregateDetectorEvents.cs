#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.WorkflowSteps/AggregateDetectorEvents.cs
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
    /// <summary>
    /// Transforms <see cref="ControllerEventLog"/> into <see cref="DetectorEventCountAggregation"/>
    /// where <see cref="ControllerEventLog.EventCode"/> equals <see cref="IndianaEnumerations.VehicleDetectorOn"/>
    /// and <see cref="ControllerEventLog.EventParam"/> equals <see cref="Detector.DetectorChannel"/>.
    /// </summary>
    public class AggregateDetectorEvents : TransformProcessStepBase<Tuple<Detector, int, IEnumerable<ControllerEventLog>>, IEnumerable<DetectorEventCountAggregation>>
    {
        /// <inheritdoc/>
        public AggregateDetectorEvents(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        /// <inheritdoc/>
        protected override Task<IEnumerable<DetectorEventCountAggregation>> Process(Tuple<Detector, int, IEnumerable<ControllerEventLog>> input, CancellationToken cancelToken = default)
        {
            var detector = input.Item1;
            var detectorChannel = input.Item2;
            var logs = input.Item3
                .Where(w => w.SignalIdentifier == detector.Approach?.Location?.LocationIdentifier)
                .Where(w => w.EventCode == (int)IndianaEnumerations.VehicleDetectorOn)
                .Where(w => w.EventParam == detectorChannel);

            var tl = new Timeline<DetectorEventCountAggregation>(logs, TimeSpan.FromMinutes(15));

            tl.Segments.ToList().ForEach(f =>
            {
                f.LocationIdentifier = detector.Approach?.Location?.LocationIdentifier;
                f.ApproachId = detector.ApproachId;
                f.DetectorPrimaryId = detector.Id;
                f.EventCount = logs.Count(w => f.InRange(w));
            });

            var result = tl.Segments.AsEnumerable();

            return Task.FromResult(result);
        }
    }
}
