#region license
// Copyright 2024 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.PreemptServiceRequest/PreemptServiceRequestService.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// You may not use this file except in compliance with the License.
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
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
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
        private readonly SplitMonitorService _splitMonitorService;
        private readonly ILogger<TransitSignalPriorityService> _logger;

        public TransitSignalPriorityService(
            IServiceProvider serviceProvider,
            SplitMonitorService splitMonitorService,
            ILogger<TransitSignalPriorityService> logger)
        {
            _serviceProvider = serviceProvider;
            _splitMonitorService = splitMonitorService;
            _logger = logger;
        }

        public TransformBlock<(string, DateTime), (Location, DateTime)?> LocationBlock =>
            new(async locationIdentifier =>
            {
                var location = new Location();
                using (var scope = _serviceProvider.CreateScope())
                {
                    var locationRepository = scope.ServiceProvider.GetRequiredService<ILocationRepository>();
                    location = locationRepository.GetLatestVersionOfLocation(locationIdentifier.Item1, locationIdentifier.Item2);

                    if (location == null)
                    {
                        _logger.LogWarning($"Location not found for {locationIdentifier.Item1} at {locationIdentifier.Item2}");
                        return null;
                    }

                    _logger.LogInformation($"Location found: {location.LocationIdentifier}");
                }
                return (location, locationIdentifier.Item2);
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism });

        //private readonly TransformBlock<(Location, DateTime)?, (Location, DateTime)> _filterValidLocationsBlock =
        //    new(tuple => tuple!.Value, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism });

        public TransformBlock<(Location, DateTime)?, (Location, List<IndianaEvent>)?> EventBlock =>
            new(tuple =>
            {
                if (tuple == null)
                    return null;
                var (location, date) = tuple!.Value;
                using var scope = _serviceProvider.CreateScope();
                var eventLogRepository = scope.ServiceProvider.GetRequiredService<IIndianaEventLogRepository>();
                var events = eventLogRepository.GetEventsByEventCodes(location.LocationIdentifier, date, date.AddDays(1), EventCodes).ToList();

                _logger.LogInformation($"Found {events.Count} events for location {location.LocationIdentifier} on {date}");

                return (location, events);
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism });

        public TransformBlock<(Location, List<IndianaEvent>)?, (Location, Dictionary<string, List<IndianaEvent>>)?> ClassifyEventsBlock =>
    new(tuple =>
    {
        if(tuple == null)
            return null;
        var (location, events) = tuple!.Value;
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
    }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism });


        public TransformBlock<(Location, Dictionary<string, List<IndianaEvent>>)?,List<SplitMonitorResult>?> ProcessSignalBlock =>
            new(async tuple =>
            {
                if (tuple == null)
                    return null;
                var (location, eventGroups) = tuple!.Value;

                var splitMonitorResult = await _splitMonitorService.GetChartData(
                    new SplitMonitorOptions(),
                    eventGroups["planEvents"],
                    eventGroups["cycleEvents"],
                    eventGroups["pedEvents"],
                    eventGroups["splitsEvents"],
                    eventGroups["terminationEvents"],
                    location);
                return (splitMonitorResult.ToList());
                // Store results if needed
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism });

        public async Task<TransitSignalPriorityResult> GetChartDataAsync(TransitSignalPriorityOptions options)
        {
            var results = new List<SplitMonitorResult>();

            // Link dataflow blocks
            LocationBlock.LinkTo(EventBlock, new DataflowLinkOptions { PropagateCompletion = true });
            //_filterValidLocationsBlock.LinkTo(EventBlock, new DataflowLinkOptions { PropagateCompletion = true });
            EventBlock.LinkTo(ClassifyEventsBlock, new DataflowLinkOptions { PropagateCompletion = true });
            ClassifyEventsBlock.LinkTo(ProcessSignalBlock, new DataflowLinkOptions { PropagateCompletion = true });

            // Post locations to pipeline
            foreach (var location in options.LocationIdentifiers)
            {
                foreach (var date in options.Dates)
                {
                    _logger.LogInformation($"Posting location {location} for date {date}");
                    bool posted = LocationBlock.Post((location, date));
                    if (!posted)
                        _logger.LogError($"Failed to post {location} for {date}");
                }
            }
            LocationBlock.Complete();
            while (await ProcessSignalBlock.OutputAvailableAsync())
            {
                while (ProcessSignalBlock.TryReceive(out var result))
                {
                    results.AddRange(result); // Store the processed results
                }
            }
            await ProcessSignalBlock.Completion; 

            return new TransitSignalPriorityResult { SplitMonitorResults = results };
        }
    }
}
