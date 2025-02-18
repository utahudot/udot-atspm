#region license
// Copyright 2024 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.PreemptServiceRequest/PreemptServiceRequestService.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Business.SplitMonitor;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Repositories.EventLogRepositories;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Business.TransitSignalPriority;
using Utah.Udot.Atspm.Extensions;
using Utah.Udot.Atspm.Business.Common;

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
        private readonly AnalysisPhaseService _analysisPhaseService;

        //private readonly SplitMonitorService _splitMonitorService;
        private readonly ILogger<TransitSignalPriorityService> _logger;

        // New block that gets all data for a single location (across multiple dates)
        private readonly TransformBlock<(string LocationIdentifier, List<DateTime> Dates), (Location, List<IndianaEvent>)?> _loadLocationDataBlock;
        private readonly TransformBlock<(Location, List<IndianaEvent>), (Location, Dictionary<string, List<IndianaEvent>>)> _classifyEventsBlock;
        private readonly TransformBlock<(Location, Dictionary<string, List<IndianaEvent>>), List<SplitMonitorResult>> _processSignalBlock;
        private readonly TransformBlock<(Location, List<IndianaEvent>)?, (Location, List<IndianaEvent>)> _filteringBlock;

        public TransitSignalPriorityService(
            IServiceProvider serviceProvider,
            //SplitMonitorService splitMonitorService,
            AnalysisPhaseService analysisPhaseService,
            ILogger<TransitSignalPriorityService> logger)
        {
            _serviceProvider = serviceProvider;
            _analysisPhaseService = analysisPhaseService;
            //_splitMonitorService = splitMonitorService;
            _logger = logger;

            // Block to load location and all events (aggregated over all dates)
            _loadLocationDataBlock = new TransformBlock<(string, List<DateTime>), (Location, List<IndianaEvent>)?>(
                async input =>
                {
                    var (locationIdentifier, dates) = input;
                    Location location;

                    // Create a scope to get the location.
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var locationRepository = scope.ServiceProvider.GetRequiredService<ILocationRepository>();
                        location = locationRepository.GetLatestVersionOfLocation(locationIdentifier, dates.First());
                    }

                    if (location == null)
                    {
                        _logger.LogWarning($"Location not found for {locationIdentifier} at {dates.First()}");
                        return null;
                    }
                    _logger.LogInformation($"Location found: {location.LocationIdentifier}");

                    // Create a semaphore to limit the number of concurrent tasks to 5.
                    using (var semaphore = new SemaphoreSlim(5))
                    {
                        // Process each date in parallel, but only allow 5 tasks concurrently.
                        var tasks = dates.Select(async date =>
                        {
                            await semaphore.WaitAsync();
                            try
                            {
                                using (var innerScope = _serviceProvider.CreateScope())
                                {
                                    var eventLogRepository = innerScope.ServiceProvider.GetRequiredService<IIndianaEventLogRepository>();
                                    var events = eventLogRepository
                                        .GetEventsByEventCodes(location.LocationIdentifier, date, date.AddDays(1), EventCodes)
                                        .ToList();
                                    _logger.LogInformation($"Found {events.Count} events for location {location.LocationIdentifier} on {date}");

                                    // Perform manual garbage collection after retrieving event logs.
                                    GC.Collect();
                                    GC.WaitForPendingFinalizers();

                                    return events;
                                }
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        }).ToList();

                        // Await all tasks and flatten the results.
                        var eventsForAllDates = (await Task.WhenAll(tasks)).SelectMany(e => e).ToList();
                        return (location, eventsForAllDates);
                    }
                },
                // Process a single signal at a time.
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 }
            );




            _filteringBlock = new TransformBlock<(Location, List<IndianaEvent>)?, (Location, List<IndianaEvent>)>(
                item =>
                {
                    if (item.HasValue)
                    {
                        return item.Value;
                    }
                    // Decide how to handle null values. Here, we return a default value that can be filtered later.
                    return default;
                },
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism }
);

            // Reuse (or slightly adjust) the existing classification block.
            _classifyEventsBlock = new TransformBlock<(Location, List<IndianaEvent>), (Location, Dictionary<string, List<IndianaEvent>>)>(
                tuple =>
                {
                    var (location, events) = tuple;
                    var categorizedEvents = new Dictionary<string, List<IndianaEvent>>
                    {
                        { "planEvents", new List<IndianaEvent>() },
                        { "pedEvents", new List<IndianaEvent>() },
                        { "cycleEvents", new List<IndianaEvent>() },
                        { "splitsEvents", new List<IndianaEvent>() },
                        { "terminationEvents", new List<IndianaEvent>() }
                    };

                    foreach (var ev in events)
                    {
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
                    }

                    return (location, categorizedEvents);
                },
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism }
            );

            _processSignalBlock = new TransformBlock<(Location, Dictionary<string, List<IndianaEvent>>), List<SplitMonitorResult>>(
                async tuple =>
                {
                    var (location, eventGroups) = tuple;
                    var phases = eventGroups["cycleEvents"].Select(c => c.EventParam).Distinct();
                    var analysisPhaseDatas = new List<AnalysisPhaseData>();
                    foreach( var phase in phases)
                    {
                        analysisPhaseDatas.Add(await _analysisPhaseService.GetAnalysisPhaseData(
                            phase,
                            eventGroups["pedEvents"],
                            eventGroups["cycleEvents"],
                            eventGroups["terminationEvents"],
                            1,
                            location
                            ));
                    }
                    var splitMonitorResult = await _splitMonitorService.GetChartData(
                        new SplitMonitorOptions(),
                        eventGroups["planEvents"],
                        eventGroups["cycleEvents"],
                        eventGroups["pedEvents"],
                        eventGroups["splitsEvents"],
                        eventGroups["terminationEvents"],
                        location);

                    return splitMonitorResult.ToList();
                },
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism }
            );
        }

        public async Task<TransitSignalPriorityResult> GetChartDataAsync(TransitSignalPriorityOptions options)
        {
            var results = new List<SplitMonitorResult>();

            // Link the blocks, and add a predicate to filteringBlock's output if needed.
            _loadLocationDataBlock.LinkTo(
                _filteringBlock,
                new DataflowLinkOptions { PropagateCompletion = true }
            );

            // Optionally, filter out any default values in filteringBlock (if default is a valid non-data marker).
            _filteringBlock.LinkTo(
                _classifyEventsBlock,
                new DataflowLinkOptions { PropagateCompletion = true },
                item => item != default
            );
            _classifyEventsBlock.LinkTo(
                _processSignalBlock,
                new DataflowLinkOptions { PropagateCompletion = true }
            );

            // For each location, post one item containing all dates.
            foreach (var location in options.LocationIdentifiers)
            {
                _logger.LogInformation($"Posting location {location} with {options.Dates.ToList().Count} dates");
                bool posted = _loadLocationDataBlock.Post((location, options.Dates.ToList()));
                if (!posted)
                    _logger.LogError($"Failed to post data for location {location}");
            }
            _loadLocationDataBlock.Complete();

            while (await _processSignalBlock.OutputAvailableAsync())
            {
                while (_processSignalBlock.TryReceive(out var result))
                {
                    results.AddRange(result);
                }
            }
            await _processSignalBlock.Completion;

            return new TransitSignalPriorityResult { SplitMonitorResults = results };
        }
    }
}
