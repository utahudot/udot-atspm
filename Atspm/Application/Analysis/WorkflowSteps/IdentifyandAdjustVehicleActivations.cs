#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/IdentifyandAdjustVehicleActivations.cs
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
    /// <c>Identify and Adjust Vehicle Actuations</c>
    /// During this step, the event log is queried to find detector activations for the subject phase
    /// (the records where the Event Code is <see cref="IndianaEnumerations.VehicleDetectorOn"/> and Event Parameter is a detector channel assigned to the subject phase). 
    /// The timestamps of the EC 82 events are noted.
    /// Timestamps for detector on events may need to be adjusted to represent vehicle arrivals at the stop bar
    /// rather than at the detector location or toadjust based on possible detector latency differences.
    /// </summary>
    public class IdentifyandAdjustVehicleActivations : TransformProcessStepBase<Tuple<Approach, IEnumerable<IndianaEvent>>, Tuple<Approach, IEnumerable<CorrectedDetectorEvent>>>
    {
        /// <inheritdoc/>
        public IdentifyandAdjustVehicleActivations(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        /// <inheritdoc/>
        protected override Task<Tuple<Approach, IEnumerable<CorrectedDetectorEvent>>> Process(Tuple<Approach, IEnumerable<IndianaEvent>> input, CancellationToken cancelToken = default)
        {
            var result = Tuple.Create(input.Item1, input.Item1?.Detectors.GroupJoin(input.Item2, o => o.DetectorChannel, i => i.EventParam, (o, i) =>
            i.Where(w => w.LocationIdentifier == input.Item1?.Location?.LocationIdentifier && w.EventCode == (int)IndianaEnumerations.VehicleDetectorOn)
            .Select(s => new CorrectedDetectorEvent()
            {
                LocationIdentifier = s.LocationIdentifier,
                PhaseNumber = o.Approach.ProtectedPhaseNumber,
                Direction = o.Approach.DirectionTypeId,
                DetectorChannel = o.DetectorChannel,
                Timestamp = AtspmMath.AdjustTimeStamp(s.Timestamp, input.Item1?.Mph ?? 0, o?.DistanceFromStopBar ?? 0, o?.LatencyCorrection ?? 0)
            }))
            .SelectMany(m => m));

            return Task.FromResult(result);
        }
    }
}