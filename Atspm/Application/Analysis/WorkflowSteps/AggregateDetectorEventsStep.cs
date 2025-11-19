#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/AggregateDetectorEventsStep.cs
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
using Utah.Udot.Atspm.Extensions;
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace Utah.Udot.Atspm.Analysis.WorkflowSteps
{
    public class AggregateDetectorEventsStep(Timeline<StartEndRange> timeline, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : TransformProcessStepBase<Tuple<Location, IEnumerable<IndianaEvent>>, IEnumerable<DetectorEventCountAggregation>>(dataflowBlockOptions)
    {
        private readonly Timeline<StartEndRange> _timeline = timeline;

        protected override Task<IEnumerable<DetectorEventCountAggregation>> Process(Tuple<Location, IEnumerable<IndianaEvent>> input, CancellationToken cancelToken = default)
        {
            var (location, rawEvents) = input;

            var eventsByParam = rawEvents
                .FromSpecification(new IndianaLogLocationFilterSpecification(location))
                .FromSpecification(new IndianaDetectorDataSpecification())
                .Where(e => e.EventCode == (short)IndianaEnumerations.VehicleDetectorOn)
                .ToParamLookup();

            var result = location.Approaches
                .SelectMany(a => a.Detectors)
                .SelectMany(d => _timeline.Segments.Select(s => new DetectorEventCountAggregation
                {
                    LocationIdentifier = location.LocationIdentifier,
                    ApproachId = d.ApproachId,
                    DetectorPrimaryId = d.Id,
                    Start = s.Start,
                    End = s.End,
                    EventCount = eventsByParam[(short)d.DetectorChannel].Count(e => s.InRange(e))
                }))
                .ToList()
                .AsEnumerable();

            return Task.FromResult(result);
        }

    }
}
