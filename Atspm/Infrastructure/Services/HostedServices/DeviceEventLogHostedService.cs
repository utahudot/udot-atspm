#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Services.HostedServices/DeviceEventLogHostedService.cs
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
using Utah.Udot.Atspm.Infrastructure.Extensions;
using Utah.Udot.ATSPM.Infrastructure.Workflows;
using Utah.Udot.NetStandardToolkit.Workflows;

namespace Utah.Udot.Atspm.Infrastructure.Services.HostedServices
{
    /// <summary>
    /// Hosted service for running the <see cref="DeviceEventLogWorkflow"/>
    /// </summary>
    /// <remarks>
    /// Hosted service for running the <see cref="DeviceEventLogWorkflow"/>
    /// </remarks>
    /// <param name="log"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="options"></param>
    public class DeviceEventLogHostedService(ILogger<DeviceEventLogHostedService> log, IServiceScopeFactory serviceProvider, IOptions<DeviceEventLoggingConfiguration> options) : HostedServiceBase(log, serviceProvider)
    {
        private readonly IOptions<DeviceEventLoggingConfiguration> _options = options;

        /// <inheritdoc/>
        public override async Task Process(IServiceScope scope, Stopwatch stopwatch, CancellationToken cancellationToken = default)
        {
            var repo = scope.ServiceProvider.GetService<IDeviceRepository>();
            var scopeFactory = scope.ServiceProvider.GetService<IServiceScopeFactory>();

            var devices = repo.GetDevicesForLogging(_options.Value.DeviceEventLoggingQueryOptions);
            int targetInstances = _options.Value.WorkflowBatchSize;
            int devicesPerWorkflow;

            if (_options.Value.DevicesBatchSize > 0)
            {
                devicesPerWorkflow = _options.Value.DevicesBatchSize.Value;
            }
            else
            {
                var totalDevices = await devices.CountAsync(cancellationToken);
                devicesPerWorkflow = (int)Math.Ceiling((double)totalDevices / targetInstances);
            }

            Func<DeviceEventLogWorkflow> workflowFactory = () => new DeviceEventLogWorkflow(scopeFactory, _options.Value.ProcessingBatchSize, _options.Value.ParallelProcesses, cancellationToken);

            await workflowFactory.BatchRunAsync(devices, devicesPerWorkflow, targetInstances, cancellationToken);

            Console.WriteLine($"-------------------hello!");
        }
    }

    public static class WorkflowExtensions
    {
        public static async Task BatchRunAsync<TInput, TOutput>(this Func<WorkflowBase<TInput, TOutput>> factory, IAsyncEnumerable<TInput> source, int batchSize, int parallelInstances, CancellationToken cancellationToken)
        {
            Func<WorkflowBase<TInput, TOutput>> factory2 = factory;
            BatchBlock<TInput> batcher = new BatchBlock<TInput>(batchSize, new GroupingDataflowBlockOptions
            {
                BoundedCapacity = batchSize * (parallelInstances + 2),
                CancellationToken = cancellationToken
            });
            ActionBlock<TInput[]> manager = new ActionBlock<TInput[]>(async delegate (TInput[] chunk)
            {
                using WorkflowBase<TInput, TOutput> workflow = factory2();
                int attempts = 0;
                while (workflow.Input == null && attempts < 150)
                {
                    await Task.Delay(50, cancellationToken);
                    attempts++;
                }

                attempts = 0;
                while (!workflow.IsInitialized && attempts < 150)
                {
                    await Task.Delay(50, cancellationToken);
                    attempts++;
                }

                foreach (TInput item in chunk)
                {
                    await workflow.Input.SendAsync(item, cancellationToken);
                }

                workflow.Input.Complete();

                //while (await workflow.Output.OutputAvailableAsync(cancellationToken))
                //{
                //    workflow.Output.TryReceive(out _);
                //}

                //await Task.WhenAll(workflow.Steps.Select((IDataflowBlock s) => s.Completion)).ContinueWith(t => Console.WriteLine($"-------------------boo!"));
                await workflow.Output.Completion;
                Console.WriteLine($"-------------------did stuff!");
            }, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = parallelInstances,
                CancellationToken = cancellationToken
            });


            batcher.LinkTo(manager, new DataflowLinkOptions
            {
                PropagateCompletion = true
            });

            await foreach (TInput item2 in source.WithCancellation(cancellationToken))
            {
                await batcher.SendAsync(item2, cancellationToken);
            }

            batcher.TriggerBatch();
            batcher.Complete();

            await manager.Completion;
            Console.WriteLine($"-------------------manager!");
        }
    }
}