#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Services.HostedServices/EventAggregationHostedService.cs
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Analysis.WorkflowFilters;
using Utah.Udot.Atspm.Analysis.Workflows;
using Utah.Udot.Atspm.Analysis.WorkflowSteps;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.ATSPM.Infrastructure.Workflows;

namespace Utah.Udot.Atspm.Infrastructure.Services.HostedServices
{
    public class EventAggregationHostedService(ILogger<EventAggregationHostedService> log, IServiceScopeFactory serviceProvider, IOptions<EventLogAggregateConfiguration> options) : HostedServiceBase(log, serviceProvider)
    {
        private readonly IOptions<EventLogAggregateConfiguration> _options = options;

        /// <inheritdoc/>
        public override async Task Process(IServiceScope scope, Stopwatch stopwatch = null, CancellationToken cancellationToken = default)
        {
            var aggDate = _options.Value.Dates.FirstOrDefault();
            var tl = aggDate.CreateTimeline<StartEndRange>(TimeSpan.FromMinutes(15));

            var locations = scope.ServiceProvider.GetService<ILocationRepository>();
            var eventLogs = scope.ServiceProvider.GetService<IEventLogRepository>();

            var loc = locations.GetLatestVersionOfLocation("7115", aggDate);
            var events = eventLogs.GetArchivedEvents("7115", aggDate.Date, aggDate.AddDays(1));

            var blockOptions = new ExecutionDataflowBlockOptions()
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1
            };

            var unBox = new UnboxArchivedEvents(blockOptions);
            var unBoxed = await unBox.ExecuteAsync(Tuple.Create<Location, IEnumerable<CompressedEventLogBase>>(loc, events));

            var filterEvents = new FilterEventsByType<IndianaEvent>(blockOptions);
            var indianaEvents = await filterEvents.ExecuteAsync(unBoxed);

            var filterPedEvents = new FilterPedDataProcessStep();
            filterPedEvents.Post(indianaEvents);
            filterPedEvents.Complete();

            var filteredPedEvents = filterPedEvents.Receive();

            var aggregatePedPhases = new AggregatePedestrianPhasesStep(tl, blockOptions);
            var phasePedAggregations = await aggregatePedPhases.ExecuteAsync(filteredPedEvents);

            Console.WriteLine($"phasePedAggregation: {phasePedAggregations.Count()}");

            foreach (var p in phasePedAggregations)
            {
                Console.WriteLine($"{p}");
            }

        }
    }

    public static class TempDateExtensions
    {
        public static Timeline<T> CreateTimeline<T>(this DateTime date, TimeSpan span) where T : IStartEndRange, new()
        {
            var startSlot = date.Date;
            var endSlot = date.Date.AddDays(1).AddTicks(-1);

            return new Timeline<T>(startSlot, endSlot, span);
        }
    }
}