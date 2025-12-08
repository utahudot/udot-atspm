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
using Utah.Udot.Atspm.Infrastructure.Extensions;
using Utah.Udot.ATSPM.Infrastructure.Workflows;

namespace Utah.Udot.Atspm.Infrastructure.Services.HostedServices
{
    public class EventAggregationHostedService(ILogger<EventAggregationHostedService> log, IServiceScopeFactory serviceProvider, IOptions<EventLogAggregateConfiguration> options) : HostedServiceBase(log, serviceProvider)
    {
        private readonly IOptions<EventLogAggregateConfiguration> _options = options;

        /// <inheritdoc/>
        public override async Task Process(IServiceScope scope, Stopwatch stopwatch = null, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"{_options.Value}");
            Console.WriteLine($"{_options.Value.EventAggregationQueryOptions}");

            foreach (var date in _options.Value.Dates)
            {
                var tl = date.CreateTimeline<StartEndRange>(TimeSpan.FromMinutes(15));

                var workflow = new AggregationWorkflow(scope.ServiceProvider.GetService<IServiceScopeFactory>(), tl, _options.Value.ParallelProcesses, cancellationToken);

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

                var result = new ActionBlock<CompressedAggregationBase>(a => Console.WriteLine($"output: {a}"));
                workflow.Output.LinkTo(result, new DataflowLinkOptions() { PropagateCompletion = true });

                var locations = scope.ServiceProvider.GetService<ILocationRepository>();
                var eventLogs = scope.ServiceProvider.GetService<IEventLogRepository>();

                await foreach (var l in locations.GetEventsForAggregation(date, _options.Value.EventAggregationQueryOptions))
                {
                    var events = await eventLogs.GetData(l.LocationIdentifier, tl.Start, tl.End).ToListAsync(cancellationToken);

                    await workflow.Input.SendAsync(Tuple.Create<Location, IEnumerable<CompressedEventLogBase>>(l, events));
                }

                workflow.Input.Complete();

                await Task.WhenAll(workflow.Steps.Select(s => s.Completion));
                await workflow.Output.Completion;
                await result.Completion;
            }
        }
    }
}