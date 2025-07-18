#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/AggregateSpeedItemEvents.cs
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
    /// Transforms <see cref="IndianaEvent"/> into <see cref="DetectorEventCountAggregation"/>
    /// where <see cref="IndianaEvent.EventCode"/> equals <see cref="IndianaEnumerations.VehicleDetectorOn"/>
    /// and <see cref="IndianaEvent.EventParam"/> equals <see cref="Detector.DetectorChannel"/>.
    /// </summary>
    public class AggregateSpeedItemEvents : TransformProcessStepBase<Tuple<Location, Tuple<IEnumerable<IndianaEvent>, IEnumerable<SpeedEvent>>>, IEnumerable<ApproachSpeedAggregation>>
    {
        private readonly TimeSpan _binSize;

        /// <inheritdoc/>
        public AggregateSpeedItemEvents(TimeSpan binSize, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            _binSize = binSize;
        }

        /// <inheritdoc/>
        protected override Task<IEnumerable<ApproachSpeedAggregation>> Process(Tuple<Location, Tuple<IEnumerable<IndianaEvent>, IEnumerable<SpeedEvent>>> input, CancellationToken cancelToken = default)
        {
            var location = input.Item1;
            var approaches = location?.Approaches.ToList();
            if (approaches == null || !approaches.Any())
            {
                return Task.FromResult(Enumerable.Empty<ApproachSpeedAggregation>());
            }
            List<ApproachSpeedAggregation> approachSpeeds = new List<ApproachSpeedAggregation>();
            // For each approach go through each detector and pull out the speed logs
            foreach (var approach in approaches)
            {
                //var detectors = approach.Detectors;
                var detectors = approach.Detectors.Select(i => i.Id.ToString());
                var phaseNumber = approach.ProtectedPhaseNumber;
                var speedEventsInApproach = input.Item2.Item2
                    .Where(speedEvent => detectors.Contains(speedEvent.DetectorId))
                    .Select(speed => (double)speed.Mph)
                    .ToList();

                var startTime = input.Item2.Item2.OrderBy(i => i.Timestamp).Select(j => j.Timestamp).FirstOrDefault();

                double totalMph = input.Item2.Item2
                    .Where(speedEvent => detectors.Contains(speedEvent.DetectorId))
                    .Sum(speed => (double)speed.Mph);

                var approachSpeed = new ApproachSpeedAggregation
                {
                    LocationIdentifier = location.LocationIdentifier,
                    BinStartTime = startTime,
                    ApproachId = approach.Id,
                    SummedSpeed = totalMph,
                    SpeedVolume = speedEventsInApproach.Count(),
                    AverageSpeed = speedEventsInApproach.Count > 0 ? Math.Round(AtspmMath.GetAverage(speedEventsInApproach), 1) : 0,
                    Speed85th = speedEventsInApproach.Count > 0 ? Math.Round(AtspmMath.Percentile(speedEventsInApproach, 85), 1) : 0,
                    Speed15th = speedEventsInApproach.Count > 0 ? Math.Round(AtspmMath.Percentile(speedEventsInApproach, 15), 1) : 0
                };
                approachSpeeds.Add(approachSpeed);
            }

            var enumerable = approachSpeeds.AsEnumerable();
            return Task.FromResult(enumerable);
        }
    }
}
