#region license
// Copyright 2024 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.PreemptServiceRequest/PreemptServiceRequestService.cs
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
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Business.SplitMonitor;
using Utah.Udot.Atspm.Business.TransitSignalPriority;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Extensions;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Repositories.EventLogRepositories;

namespace Utah.Udot.Atspm.Business.TransitSignalPriorityRequest
{
    public class TransitSignalPriorityService
    {
        private static readonly short[] EventCodes =
            (new short[] { 1, 4, 5, 6, 7, 8, 11, 21, 23 })
            .Concat(Enumerable.Range(130, 20).Select(x => (short)x))
            .ToArray();

        private static readonly int _maxDegreeOfParallelism = 5;

        private readonly IServiceProvider _serviceProvider;
        private readonly PlanService _planService;
        private readonly SplitMonitorService _splitMonitorService;

        public TransitSignalPriorityService(IServiceProvider serviceProvider, PlanService planService, SplitMonitorService splitMonitorService)
        {
            _serviceProvider = serviceProvider;
            _planService = planService;
            _splitMonitorService = splitMonitorService;
        }

        public async Task<TransitSignalPriorityResult> GetChartDataAsync(TransitSignalPriorityOptions options)
        {
            var results = new ConcurrentDictionary<string, IEnumerable<SplitMonitorResult>>();
            var eventCollection = new ConcurrentBag<IndianaEvent>();

            var startDate = options.Dates.Min().Date;
            var endDate = options.Dates.Max().Date.AddDays(1).AddTicks(-1);

            var dateLocationBlock = new TransformManyBlock<DateTime, (Location, DateTime)>(async date =>
            {
                using var scope = _serviceProvider.CreateScope();
                var locationRepository = scope.ServiceProvider.GetRequiredService<ILocationRepository>();
                var locations = locationRepository.GetVersionOfLocations(options.LocationIdentifiers.ToList(), date).ToList();
                return locations.Select(location => (location, date));
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism });

            var eventBlock = new TransformManyBlock<(Location, DateTime), (Location, IndianaEvent)>(tuple =>
            {
                var (location, date) = tuple;
                var events = new List<IndianaEvent>();

                using var scope = _serviceProvider.CreateScope();
                var eventLogRepository = scope.ServiceProvider.GetRequiredService<IIndianaEventLogRepository>();
                events = eventLogRepository.GetEventsByEventCodes(location.LocationIdentifier, startDate, endDate, EventCodes).ToList();

                return events.Select(e => (location, e));
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism });

            var classifyEventsBlock = new TransformBlock<(Location, IndianaEvent), (Location, Dictionary<string, List<IndianaEvent>>)>
            (tuple =>
            {
                var (location, ev) = tuple;

                var categorizedEvents = new Dictionary<string, List<IndianaEvent>>
                {
            { "planEvents", new List<IndianaEvent>() },
            { "pedEvents", new List<IndianaEvent>() },
            { "cycleEvents", new List<IndianaEvent>() },
            { "splitsEvents", new List<IndianaEvent>() },
            { "terminationEvents", new List<IndianaEvent>() }
                };

                if (new List<short> { 130, 131 }.Contains(ev.EventCode))
                    categorizedEvents["planEvents"].Add(ev);

                if (new List<short> { 21, 23 }.Contains(ev.EventCode))
                    categorizedEvents["pedEvents"].Add(ev);

                if (new List<short> { 1, 4, 5, 6, 7, 8, 11 }.Contains(ev.EventCode))
                    categorizedEvents["cycleEvents"].Add(ev);

                if (Enumerable.Range(130, 20).Contains(ev.EventCode))
                    categorizedEvents["splitsEvents"].Add(ev);

                if (new List<short> { 4, 5, 6, 7 }.Contains(ev.EventCode))
                    categorizedEvents["terminationEvents"].Add(ev);

                return (location, categorizedEvents);
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism });

            var processSignalBlock = new ActionBlock<(Location, Dictionary<string, List<IndianaEvent>>)>(async tuple =>
            {
                var (location, eventGroups) = tuple;

                var splitMonitorResult = await _splitMonitorService.GetChartData(
                    new SplitMonitorOptions { 
                        LocationIdentifier = location.LocationIdentifier, 
                        PercentileSplit = 50, 
                        Start = eventGroups.SelectMany(kvp => kvp.Value).Min(e => e.Timestamp).Date,
                        End = eventGroups.SelectMany(kvp => kvp.Value).Max(e => e.Timestamp).Date.AddDays(1)
                    },
                    eventGroups["planEvents"],
                    eventGroups["cycleEvents"],
                    eventGroups["pedEvents"],
                    eventGroups["splitsEvents"],
                    eventGroups["terminationEvents"],
                    location);

                results[location.LocationIdentifier] = splitMonitorResult;
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism });

            // Link dataflow blocks
            dateLocationBlock.LinkTo(eventBlock, new DataflowLinkOptions { PropagateCompletion = true });
            eventBlock.LinkTo(classifyEventsBlock, new DataflowLinkOptions { PropagateCompletion = true });
            classifyEventsBlock.LinkTo(processSignalBlock, new DataflowLinkOptions { PropagateCompletion = true });

            // Post all dates to the pipeline
            foreach (var date in options.Dates)
                dateLocationBlock.Post(date);

            dateLocationBlock.Complete();
            await processSignalBlock.Completion;
            var result = new TransitSignalPriorityResult { SplitMonitorResults = results.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) };
            return result;
        }



    }
}







