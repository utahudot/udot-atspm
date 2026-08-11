#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/AggregatePriorityStep.cs
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
    public class AggregatePriorityStep(Timeline<StartEndRange> timeline, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : TransformProcessStepBase<Tuple<Location, IEnumerable<IndianaEvent>>, IEnumerable<PriorityAggregation>>(dataflowBlockOptions)
    {
        private readonly Timeline<StartEndRange> _timeline = timeline;

        protected override Task<IEnumerable<PriorityAggregation>> Process(Tuple<Location, IEnumerable<IndianaEvent>> input, CancellationToken cancelToken = default)
        {
            var (location, rawEvents) = input;

            var filteredLogs = rawEvents
                .FromSpecification(new EventLogSpecification(location))
                .Cast<IndianaEvent>()
                .Where(w => w.EventCode == (short)IndianaEnumerations.TSPCheckIn ||
                    w.EventCode == (short)IndianaEnumerations.TSPAdjustmenttoEarlyGreen ||
                    w.EventCode == (short)IndianaEnumerations.TSPAdjustmenttoExtendGreen)
                .ToList();

            var logsByPriority = filteredLogs
                .GroupBy(e => e.EventParam);

            var result = _timeline.Segments.SelectMany(s =>
            {
                return logsByPriority.Select(group =>
                {
                    short priority = group.Key;
                    var logs = group.ToList();

                    return new PriorityAggregation
                    {
                        Start = s.Start,
                        End = s.End,
                        LocationIdentifier = location.LocationIdentifier,
                        PriorityNumber = priority,
                        PriorityRequests = logs.Count(c => c.EventCode == (short)IndianaEnumerations.TSPCheckIn && s.InRange(c)),
                        PriorityServiceEarlyGreen = logs.Count(c => c.EventCode == (short)IndianaEnumerations.TSPAdjustmenttoEarlyGreen && s.InRange(c)),
                        PriorityServiceExtendedGreen = logs.Count(c => c.EventCode == (short)IndianaEnumerations.TSPAdjustmenttoExtendGreen && s.InRange(c))
                    };
                });
            }).ToList().AsEnumerable();

            return Task.FromResult(result);
        }
    }
}
