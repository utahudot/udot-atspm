#region license
// Copyright 2024 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/AggregateDetectorEvents.cs
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
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace Utah.Udot.Atspm.Analysis.WorkflowSteps
{
    /// <summary>
    /// Transforms <see cref="IndianaEvent"/> into <see cref="DetectorEventCountAggregation"/>
    /// where <see cref="IndianaEvent.EventCode"/> equals <see cref="IndianaEnumerations.VehicleDetectorOn"/>
    /// and <see cref="IndianaEvent.EventParam"/> equals <see cref="Detector.DetectorChannel"/>.
    /// </summary>
    public class AggregateDetectorEvents : TransformProcessStepBase<Tuple<Detector, int, IEnumerable<IndianaEvent>>, IEnumerable<DetectorEventCountAggregation>>
    {
        private readonly TimeSpan _binSize;
        
        /// <inheritdoc/>
        public AggregateDetectorEvents(TimeSpan binSize, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) 
        {
            _binSize = binSize;
        }

        /// <inheritdoc/>
        protected override Task<IEnumerable<DetectorEventCountAggregation>> Process(Tuple<Detector, int, IEnumerable<IndianaEvent>> input, CancellationToken cancelToken = default)
        {
            var detector = input.Item1;
            var detectorChannel = input.Item2;
            var events = input.Item3
                .Where(w => w.EventCode == (int)IndianaEnumerations.VehicleDetectorOn)
                .Where(w => w.EventParam == detectorChannel)
                .FromSpecification(new EventLogSpecification(detector.Approach?.Location))
                .Cast<IndianaEvent>()
                .ToList();

            var tl = new Timeline<DetectorEventCountAggregation>(events, _binSize);

            tl.Segments.ToList().ForEach(f =>
            {
                f.LocationIdentifier = detector.Approach?.Location?.LocationIdentifier;
                f.ApproachId = detector.ApproachId;
                f.DetectorPrimaryId = detector.Id;
                f.EventCount = events.Count(w => f.InRange(w));
            });

            var result = tl.Segments.AsEnumerable();

            return Task.FromResult(result);
        }
    }
}
