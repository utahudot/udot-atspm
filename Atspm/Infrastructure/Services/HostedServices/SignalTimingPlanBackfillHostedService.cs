#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Services.HostedServices/SignalTimingPlanBackfillHostedService.cs
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
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Infrastructure.Extensions;
using Utah.Udot.ATSPM.Infrastructure.Workflows;
using Utah.Udot.NetStandardToolkit.Workflows;

namespace Utah.Udot.Atspm.Infrastructure.Services.HostedServices
{
    /// <summary>
    /// Hosted service for backfilling <see cref="SignalTimingPlan"/> rows from stored Indiana event logs.
    /// </summary>
    public class SignalTimingPlanBackfillHostedService(
        ILogger<SignalTimingPlanBackfillHostedService> log,
        IServiceScopeFactory serviceProvider,
        IOptions<SignalTimingPlanBackfillConfiguration> options) : HostedServiceBase(log, serviceProvider)
    {
        private readonly ILogger<SignalTimingPlanBackfillHostedService> _log = log;
        private readonly IOptions<SignalTimingPlanBackfillConfiguration> _options = options;

        /// <inheritdoc/>
        public override async Task Process(IServiceScope scope, Stopwatch stopwatch = default, CancellationToken cancellationToken = default)
        {
            var config = _options.Value;

            if (config.End <= config.Start)
            {
                throw new InvalidOperationException($"{nameof(config.End)} must be after {nameof(config.Start)}.");
            }

            var locationRepository = scope.ServiceProvider.GetRequiredService<ILocationRepository>();
            var eventLogRepository = scope.ServiceProvider.GetRequiredService<IIndianaEventLogRepository>();
            var scopeFactory = scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>();

            var queryStart = config.Start.AddHours(-config.SignalTimingPlanOffsetHours);
            var queryEnd = config.End.AddHours(config.SignalTimingPlanOffsetHours);
            var workflow = new SignalTimingPlansWorkflow(
                scopeFactory,
                config.ProcessingBatchSize,
                config.ParallelProcesses,
                cancellationToken);
            await workflow.WhenInitialized();

            var planRowsProcessed = 0;
            var result = new ActionBlock<SignalTimingPlan>(
                _ => planRowsProcessed++,
                new ExecutionDataflowBlockOptions { CancellationToken = cancellationToken });

            workflow.Output.LinkTo(result, new DataflowLinkOptions { PropagateCompletion = true });

            var locationsProcessed = 0;
            var planEventsQueued = 0;

            await foreach (var location in locationRepository
                .GetEventsForAggregation(config.End, config.EventAggregationQueryOptions)
                .WithCancellation(cancellationToken))
            {
                locationsProcessed++;

                var planEvents = eventLogRepository
                    .GetEventsBetweenDates(location.LocationIdentifier, queryStart, queryEnd)
                    .FromSpecification(new IndianaPlanDataSpecification())
                    .OrderBy(o => o.Timestamp)
                    .ToList();

                foreach (var planEvent in planEvents)
                {
                    await workflow.Input.SendAsync(planEvent, cancellationToken);
                }

                planEventsQueued += planEvents.Count;

                _log.LogInformation(
                    "Queued {PlanEventCount} signal timing plan events for {LocationIdentifier}",
                    planEvents.Count,
                    location.LocationIdentifier);
            }

            workflow.Input.Complete();

            await workflow.Output.Completion;
            await result.Completion;

            _log.LogInformation(
                "Backfilled signal timing plans from {PlanEventCount} plan events for {LocationCount} locations. Processed {PlanRowCount} plan rows.",
                planEventsQueued,
                locationsProcessed,
                planRowsProcessed);
        }
    }
}
