#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/AggregatePedestrianPhases.cs
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

using System.Linq;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Business.PedDelay;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Extensions;
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace Utah.Udot.Atspm.Analysis.WorkflowSteps
{
    public class AggregatePedestrianPhases : TransformProcessStepBase<Tuple<Location, IEnumerable<IndianaEvent>>, IEnumerable<PhasePedAggregation>>
    {
        private readonly TimeSpan _binSize;

        public AggregatePedestrianPhases(TimeSpan binSize, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            _binSize = binSize;
        }

        protected override Task<IEnumerable<PhasePedAggregation>> Process(Tuple<Location, IEnumerable<IndianaEvent>> input, CancellationToken cancelToken = default)
        {
            var location = input.Item1;

            var events = input.Item2
                .Where(w => w.LocationIdentifier == location.LocationIdentifier)
                .WhereCode([
                    (short)IndianaEnumerations.PedestrianBeginWalk,
                    (short)IndianaEnumerations.PedestrianBeginChangeInterval,
                    (short)IndianaEnumerations.PedestrianOverlapBeginWalk,
                    (short)IndianaEnumerations.PedestrianOverlapBeginClearance,
                    (short)IndianaEnumerations.PedDetectorOn,
                    (short)IndianaEnumerations.PedestrianCallRegistered
                ])
                .ToList();


            var aggDate = DateTime.Parse("5/16/2023");
            var startSlot = aggDate.Date;
            var endSlot = aggDate.Date.AddDays(1).AddTicks(-1);

            var eventsByParam = events.ToParamLookup();

            var result = location.Approaches.SelectMany(o =>
            {
                var matchingEvents = eventsByParam[(short)o.ProtectedPhaseNumber];

                var pedCycles = matchingEvents.IdentifyPedCycles(o.IsPedestrianPhaseOverlap);

                var tl = new Timeline<PhasePedAggregation>(startSlot, endSlot, _binSize);

                foreach (var f in tl.Segments)
                {
                    f.LocationIdentifier = o.Location.LocationIdentifier;
                    f.PhaseNumber = o.ProtectedPhaseNumber;

                    var inRangeCycles = pedCycles.Where(c => f.InRange(c.BeginWalk)).ToList();

                    if (inRangeCycles.Any())
                    {
                        f.PedCycles = inRangeCycles.Count;
                        f.PedDelay = inRangeCycles.Sum(s => s.PedDelay);

                        f.MinPedDelay = inRangeCycles
                        .Where(w => w.PedDelay > 0)
                        .Select(w => w.PedDelay)
                        .DefaultIfEmpty(0)
                        .Min();

                        f.MaxPedDelay = inRangeCycles
                        .Select(w => w.PedDelay)
                        .DefaultIfEmpty(0)
                        .Max();
                    }

                    if (matchingEvents.Any())
                    {
                        f.PedRequests = matchingEvents.Count(w => w.EventCode == 90 && f.InRange(w.Timestamp));

                        var imputedCalls = matchingEvents.Where(w => w.EventCode is 90 or 21 or 67 or 0).OrderBy(o => o.Timestamp).ToList();
                        f.ImputedPedCallsRegistered = imputedCalls.Where((w, i) =>
                            i > 0 &&
                            f.InRange(w.Timestamp) &&
                            w.EventCode == 90 &&
                            imputedCalls[i - 1].EventCode is 21 or 67 or 0).Count();

                        var uniquePedDetections = matchingEvents.Where(w => w.EventCode == 90).OrderBy(o => o.Timestamp).ToList();
                        f.UniquePedDetections = uniquePedDetections.SlidingWindow(2)
                            .Where(x => x[1].Timestamp - x[0].Timestamp > _binSize)
                            .Count(w => f.InRange(w[1].Timestamp));

                        f.PedBeginWalkCount = matchingEvents.Count(w => w.EventCode == 21 && f.InRange(w.Timestamp));
                        f.PedCallsRegisteredCount = matchingEvents.Count(w => w.EventCode == 45 && f.InRange(w.Timestamp));
                    }
                }

                return tl.Segments;
            })
                .ToList()
                .AsEnumerable();


            return Task.FromResult(result);
        }
    }

    /// <summary>
    /// Breaks out all <see cref="Approach"/> from <see cref="Location"/>
    /// and returns separate Tuples of <see cref="Approach"/>/<see cref="IEnumerable{IndianaEvent}"/> pairs
    /// sorted by <see cref="ITimestamp.Timestamp"/>.
    /// </summary>
    public class GroupApproachesByLocation : TransformManyProcessStepBase<Tuple<Location, IEnumerable<IndianaEvent>>, Tuple<Approach, int, IEnumerable<IndianaEvent>>>
    {
        /// <inheritdoc/>
        public GroupApproachesByLocation(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        /// <inheritdoc/>
        protected override Task<IEnumerable<Tuple<Approach, int, IEnumerable<IndianaEvent>>>> Process(Tuple<Location, IEnumerable<IndianaEvent>> input, CancellationToken cancelToken = default)
        {
            var location = input.Item1;
            var events = input.Item2
                .FromSpecification(new EventLogSpecification(location))
                .Cast<IndianaEvent>()
                .ToList()
                .AsEnumerable();

            //foreach (var e in events)
            //{
            //    var en = (IndianaEnumerations)e.EventCode;
            //    var att = en.GetAttributeOfType<IndianaEventLayerAttribute>();

            //    if (att.IndianaEventParamType == IndianaEventParamType.PhaseNumber && )
            //    {

            //    }
            //}

            var result = location.Approaches.Select(s => Tuple.Create(s, s.ProtectedPhaseNumber, events));

            return Task.FromResult(result);
        }
    }
}
