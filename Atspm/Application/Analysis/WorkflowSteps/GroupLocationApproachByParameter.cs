#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/GroupSignalByParameter.cs
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
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Analysis.WorkflowSteps
{
    public class GroupLocationApproachByParameter : TransformManyProcessStepBase<Tuple<Location, Tuple<IEnumerable<IndianaEvent>, IEnumerable<SpeedEvent>>>, Tuple<Location, Tuple<IEnumerable<IndianaEvent>, IEnumerable<SpeedEvent>>>>
    {
        public GroupLocationApproachByParameter(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<Tuple<Location, Tuple<IEnumerable<IndianaEvent>, IEnumerable<SpeedEvent>>>>> Process(Tuple<Location, Tuple<IEnumerable<IndianaEvent>, IEnumerable<SpeedEvent>>> input, CancellationToken cancelToken = default)
        {
            var location = input.Item1;

            // Step 1: Filter and remove consecutive duplicate EventCodes
            var filteredIndianaEvents = input.Item2.Item1
                .Where(e => e.EventCode == 1 || e.EventCode == 8)
                .OrderBy(e => e.Timestamp)
                .Aggregate(
                    new List<IndianaEvent>(),
                    (acc, current) =>
                    {
                        if (acc.Count == 0 || acc.Last().EventCode != current.EventCode)
                            acc.Add(current);
                        return acc;
                    });

            // Step 2: Pair (start, end) IndianaEvents and collect matching SpeedEvents
            var tuples = new List<Tuple<IEnumerable<IndianaEvent>, IEnumerable<SpeedEvent>>>();
            IndianaEvent startEvent = filteredIndianaEvents.FirstOrDefault();

            foreach (var indianaEvent in filteredIndianaEvents)
            {
                if (indianaEvent.EventCode == 1)
                {
                    startEvent = indianaEvent;
                }
                else if (indianaEvent.EventCode == 8 && startEvent != null)
                {
                    var start = startEvent.Timestamp.AddSeconds(15);
                    var end = indianaEvent.Timestamp;

                    if (end > start)
                    {
                        var matchingSpeeds = input.Item2.Item2
                            .Where(s => s.Timestamp >= start && s.Timestamp <= end);

                        tuples.Add(Tuple.Create<IEnumerable<IndianaEvent>, IEnumerable<SpeedEvent>>(
                            new[] { startEvent, indianaEvent }, matchingSpeeds));
                    }
                }
            }

            // Step 3: Bin by 15-minute intervals based on startEvent timestamp
            var binnedTuples = tuples
                .GroupBy(t => RoundDownTo15Min(t.Item1.First().Timestamp))
                .ToDictionary(g => g.Key, g => g.ToList());

            // Step 4: Flatten and wrap result
            var result = binnedTuples.Select(bin =>
                Tuple.Create(
                    location,
                    Tuple.Create(
                        bin.Value.SelectMany(t => t.Item1),
                        bin.Value.SelectMany(t => t.Item2)
                    )
                ));

            return Task.FromResult(result);
        }

        DateTime RoundDownTo15Min(DateTime dt)
        {
            var minutes = (dt.Minute / 15) * 15;
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, minutes, 0);
        }
    }
}
