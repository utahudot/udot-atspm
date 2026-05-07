#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Services.HostedServices/EventLogTransferHostedService.cs
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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Data;

namespace Utah.Udot.Atspm.Infrastructure.Services.HostedServices
{
    /// <summary>
    /// Hosted service for transferring event logs between repositories.
    /// </summary>
    /// <remarks>
    /// Hosted service for transferring event logs between repositories.
    /// </remarks>
    /// <param name="log"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="options"></param>
    public class EventLogTransferHostedService(ILogger<EventAggregationHostedService> log, IServiceScopeFactory serviceProvider, IOptions<EventLogTransferOptions> options) : HostedServiceBase(log, serviceProvider)
    {
        private readonly IOptions<EventLogTransferOptions> _options = options;

        /// <inheritdoc/>
        public override async Task Process(IServiceScope scope, Stopwatch stopwatch, CancellationToken cancellationToken = default)
        {
            Console.WriteLine(_options.Value);

            var temp = scope.ServiceProvider.GetKeyedService<EventLogContext>(nameof(EventLogTransferOptions.SourceRepository));

            var total = await temp.CompressedEvents
                .AsNoTracking()
                .Where(w => w.Start >= _options.Value.StartDate && w.End <= _options.Value.EndDate)
                .CountAsync(cancellationToken);

            Console.WriteLine($"result count: {total}");

            var batchSize = 1000;

            for (int i = 0; i < Math.Ceiling((double)total / batchSize); i++)
            {
                var read = new TransformBlock<int, IEnumerable<CompressedEventLogBase>>(async i =>
                {
                    using (var s = scope.ServiceProvider.GetService<IServiceScopeFactory>().CreateAsyncScope())
                    {
                        using (var sourceContext = s.ServiceProvider.GetKeyedService<EventLogContext>(nameof(EventLogTransferOptions.SourceRepository)))
                        {
                            Console.WriteLine($"Retrieving Batch {i + 1} | {sourceContext.GetHashCode()}");

                            var result = await sourceContext.CompressedEvents
                                .AsNoTracking()
                                .Where(w => w.Start >= _options.Value.StartDate && w.End <= _options.Value.EndDate)
                                .OrderBy(o => o.LocationIdentifier).ThenBy(o => o.DeviceId).ThenBy(o => o.DataType).ThenBy(o => o.Start)
                                .Skip(i * batchSize).Take(batchSize)
                                .ToListAsync(cancellationToken);

                            return result;
                        }
                    }
                });

                var write = new ActionBlock<IEnumerable<CompressedEventLogBase>>(async b =>
                {
                    using (var s = scope.ServiceProvider.GetService<IServiceScopeFactory>().CreateAsyncScope())
                    {
                        using (var destContext = s.ServiceProvider.GetKeyedService<EventLogContext>(nameof(EventLogTransferOptions.DestinationRepository)))
                        {
                            Console.WriteLine($"Writing Batch {i + 1} | {destContext.GetHashCode()}");

                            foreach (var i in b)
                            {
                                await destContext.CompressedEvents.AddAsync(i);
                            }

                            Console.WriteLine($"Saving Batch {i + 1}");
                            await destContext.SaveChangesAsync(cancellationToken);
                        }
                    }
                });

                read.LinkTo(write, new DataflowLinkOptions() { PropagateCompletion = true });

                await read.SendAsync(i, cancellationToken);

                read.Complete();

                await write.Completion;

                Console.WriteLine($"batch complete: {i + 1}");
            }



            //// 1. Define the blocks ONCE outside the loop
            //var dataflowOptions = new ExecutionDataflowBlockOptions
            //{
            //    // This allows multiple batches to be read/written at the same time
            //    MaxDegreeOfParallelism = 4,
            //    EnsureOrdered = true
            //};

            //var readBlock = new TransformBlock<int, IEnumerable<CompressedEventLogBase>>(async batchIndex =>
            //{
            //    using var s = scope.ServiceProvider.GetService<IServiceScopeFactory>().CreateAsyncScope();
            //    using var sourceContext = s.ServiceProvider.GetKeyedService<EventLogContext>(nameof(EventLogTransferOptions.SourceRepository));

            //    return await sourceContext.CompressedEvents
            //        .AsNoTracking()
            //        .Where(w => w.Start >= _options.Value.StartDate && w.End <= _options.Value.EndDate)
            //        .OrderBy(o => o.LocationIdentifier).ThenBy(o => o.DeviceId) // ... etc
            //        .Skip(batchIndex * batchSize)
            //        .Take(batchSize)
            //        .ToListAsync(cancellationToken);
            //}, dataflowOptions);

            //var writeBlock = new ActionBlock<IEnumerable<CompressedEventLogBase>>(async batch =>
            //{
            //    using var s = scope.ServiceProvider.GetService<IServiceScopeFactory>().CreateAsyncScope();
            //    using var destContext = s.ServiceProvider.GetKeyedService<EventLogContext>(nameof(EventLogTransferOptions.DestinationRepository));

            //    await destContext.CompressedEvents.AddRangeAsync(batch);
            //    await destContext.SaveChangesAsync(cancellationToken);
            //}, dataflowOptions);

            //// 2. Link them
            //readBlock.LinkTo(writeBlock, new DataflowLinkOptions { PropagateCompletion = true });

            //// 3. Push all work into the pipeline without awaiting inside the loop
            //int totalBatches = (int)Math.Ceiling((double)total / batchSize);
            //for (int i = 0; i < totalBatches; i++)
            //{
            //    await readBlock.SendAsync(i, cancellationToken);
            //}

            //// 4. Signal completion and await the WHOLE thing once
            //readBlock.Complete();
            //await writeBlock.Completion;
        }
    }
}