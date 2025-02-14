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

        public TransitSignalPriorityService(IServiceProvider serviceProvider, PlanService planService)
        {
            _serviceProvider = serviceProvider;
            _planService = planService;
        }

        public async Task<TransitSignalPriorityResult> GetChartDataAsync(TransitSignalPriorityOptions options)
        {
            var result = new TransitSignalPriorityResult { Events = new List<IndianaEvent>() };
            var eventCollection = new ConcurrentBag<IndianaEvent>();

            var dateLocationBlock = new TransformManyBlock<DateTime, (Location, DateTime)>(async date =>
            {
                var locations = new List<Location>();

                using (var scope = _serviceProvider.CreateScope())
                {
                    var locationRepository = scope.ServiceProvider.GetRequiredService<ILocationRepository>();
                    locations = locationRepository.GetVersionOfLocations(options.LocationIdentifiers.ToList(), date).ToList();
                }

                return locations.Select(location => (location, date));
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism });

            var eventBlock = new TransformManyBlock<(Location, DateTime), IndianaEvent>(tuple =>
            {
                var (location, date) = tuple;
                var startTime = date.Date;
                var endTime = date.Date.AddDays(1).AddTicks(-1);
                var events = new List<IndianaEvent>();

                using (var scope = _serviceProvider.CreateScope())
                {
                    var eventLogRepository = scope.ServiceProvider.GetRequiredService<IIndianaEventLogRepository>();
                    events = eventLogRepository.GetEventsByEventCodes(location.LocationIdentifier, startTime, endTime, EventCodes).ToList();
                }

                // Manually trigger garbage collection after processing each batch
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                return events;

            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism });

            var outputBlock = new ActionBlock<IndianaEvent>(eventData =>
            {
                eventCollection.Add(eventData);

            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism });

            // Link blocks together
            dateLocationBlock.LinkTo(eventBlock, new DataflowLinkOptions { PropagateCompletion = true });
            eventBlock.LinkTo(outputBlock, new DataflowLinkOptions { PropagateCompletion = true });

            // Post all dates to the pipeline for concurrent execution
            foreach (var date in options.Dates)
                dateLocationBlock.Post(date);

            dateLocationBlock.Complete();
            await outputBlock.Completion;

            result.Events = eventCollection.ToList();

            // Final GC cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            return result;
        }


    }
}







