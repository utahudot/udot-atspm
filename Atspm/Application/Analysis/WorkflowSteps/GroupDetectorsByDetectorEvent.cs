﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.WorkflowSteps/GroupDetectorsByDetectorEvent.cs
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
    /// <summary>
    /// Breaks out all the <see cref="Detector"/> from <see cref="Approach"/>
    /// and returns separate Tuples of <see cref="Detector"/>/<see cref="Detector.DetectorChannel"/>/<see cref="ControllerEventLog"/> sets
    /// where the <see cref="ControllerEventLog.EventCode"/> equals <see cref="IndianaEnumerations.VehicleDetectorOn"/>
    /// and <see cref="ControllerEventLog.EventParam"/> equals <see cref="Detector.DetectorChannel"/>
    /// sorted by <see cref="ControllerEventLog.Timestamp"/>.
    /// </summary>
    public class GroupDetectorsByDetectorEvent : TransformManyProcessStepBase<Tuple<Approach, IEnumerable<IndianaEvent>>, Tuple<Detector, int, IEnumerable<IndianaEvent>>>
    {
        /// <inheritdoc/>
        public GroupDetectorsByDetectorEvent(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        /// <inheritdoc/>
        protected override Task<IEnumerable<Tuple<Detector, int, IEnumerable<IndianaEvent>>>> Process(Tuple<Approach, IEnumerable<IndianaEvent>> input, CancellationToken cancelToken = default)
        {
            var approach = input.Item1;
            var logs = input.Item2;

            var result = approach.Detectors
                .GroupJoin(logs
                .Where(w => w.LocationIdentifier == approach?.Location?.LocationIdentifier)
                .Where(w => w.EventCode == (int)IndianaEnumerations.VehicleDetectorOn),
                o => o.DetectorChannel, i => i.EventParam, (o, i) => Tuple.Create(o, o.DetectorChannel, i.OrderBy(o => o.Timestamp).AsEnumerable()));

            return Task.FromResult(result);
        }
    }
}
