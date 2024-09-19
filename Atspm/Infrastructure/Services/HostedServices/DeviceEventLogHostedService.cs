#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Services.HostedServices/SignalLoggerUtilityHostedService.cs
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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using System.Windows.Input;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.ATSPM.Infrastructure.WorkflowSteps;
using Utah.Udot.NetStandardToolkit.Workflows;

namespace Utah.Udot.Atspm.Infrastructure.Services.HostedServices
{
    /// <summary>
    /// Hosted service for running the <see cref="DeviceEventLogWorkflow"/>
    /// </summary>
    public class DeviceEventLogHostedService : IHostedService
    {
        private readonly ILogger _log;
        private readonly IServiceScopeFactory _services;
        private readonly IOptions<DeviceEventLoggingConfiguration> _options;

        /// <summary>
        /// Hosted service for running the <see cref="DeviceEventLogWorkflow"/>
        /// </summary>
        /// <param name="log"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="options"></param>
        public DeviceEventLogHostedService(ILogger<DeviceEventLogHostedService> log, IServiceScopeFactory serviceProvider, IOptions<DeviceEventLoggingConfiguration> options) =>
            (_log, _services, _options) = (log, serviceProvider, options);

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var serviceName = this.GetType().Name;
            var logMessages = new HostedServiceLogMessages(_log, this.GetType().Name);

            cancellationToken.Register(() => logMessages.StartingCancelled(serviceName));
            logMessages.StartingService(serviceName);

            var sw = new Stopwatch();
            sw.Start();

            using (var scope = _services.CreateAsyncScope())
            {
                var workflow = new DeviceEventLogWorkflow(_services, _options.Value.BatchSize, _options.Value.ParallelProcesses, cancellationToken);

                var repo = scope.ServiceProvider.GetService<IDeviceRepository>();

                await foreach (var d in repo.GetDevicesForLogging(_options.Value.DeviceEventLoggingQueryOptions))
                {
                    await workflow.Input.SendAsync(d);
                }

                workflow.Input.Complete();

                await Task.WhenAll(workflow.Steps.Select(s => s.Completion));
            }

            sw.Stop();

            logMessages.CompletingService(serviceName, sw.Elapsed);
        }

        /// <inheritdoc/>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            var serviceName = this.GetType().Name;
            var logMessages = new HostedServiceLogMessages(_log, this.GetType().Name);

            cancellationToken.Register(() => logMessages.StoppingCancelled(serviceName));
            logMessages.StoppingService(serviceName);

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Downloads <see cref="EventLogModelBase"/> objects from <see cref="Device"/> and saves them to <see cref="IEventLogRepository"/>
    /// </summary>
    public class DeviceEventLogWorkflow : WorkflowBase<Device, CompressedEventLogBase>
    {
        private readonly IServiceScopeFactory _services;
        private readonly int _batchSize;
        private readonly int _parallelProcesses;
        private readonly CancellationToken _cancellationToken;

        /// <inheritdoc/>
        public DeviceEventLogWorkflow(IServiceScopeFactory services, int batchSize = 50000, int parallelProcesses = 50, CancellationToken cancellationToken = default)
        {
            _services = services;
            _batchSize = batchSize;
            _parallelProcesses = parallelProcesses;
            _cancellationToken = cancellationToken;
        }

        public DownloadDeviceData DownloadDeviceData { get; private set; }
        public DecodeDeviceData DecodeDeviceData { get; private set; }
        public BatchBlock<Tuple<Device, EventLogModelBase>> BatchEventLogs { get; private set; }
        public ArchiveDataEvents ArchiveDeviceData { get; private set; }
        public SaveEventsToRepo SaveEventsToRepo { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(DownloadDeviceData);
            Steps.Add(DecodeDeviceData);
            Steps.Add(BatchEventLogs);
            Steps.Add(ArchiveDeviceData);
            Steps.Add(SaveEventsToRepo);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            DownloadDeviceData = new(_services, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = _parallelProcesses, CancellationToken = _cancellationToken });
            DecodeDeviceData = new(_services, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = _parallelProcesses, CancellationToken = _cancellationToken });
            BatchEventLogs = new(_batchSize);
            ArchiveDeviceData = new(new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = _parallelProcesses, CancellationToken = _cancellationToken });
            SaveEventsToRepo = new(_services, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1, CancellationToken = _cancellationToken });
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(DownloadDeviceData, new DataflowLinkOptions() { PropagateCompletion = true });
            DownloadDeviceData.LinkTo(DecodeDeviceData, new DataflowLinkOptions() { PropagateCompletion = true });
            DecodeDeviceData.LinkTo(BatchEventLogs, new DataflowLinkOptions() { PropagateCompletion = true });
            BatchEventLogs.LinkTo(ArchiveDeviceData, new DataflowLinkOptions() { PropagateCompletion = true });
            ArchiveDeviceData.LinkTo(SaveEventsToRepo, new DataflowLinkOptions() { PropagateCompletion = true });
            SaveEventsToRepo.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }

    

    public class SaveEventsToRepo : TransformManyProcessStepBaseAsync<CompressedEventLogBase, CompressedEventLogBase>
    {
        private readonly IServiceScopeFactory _services;

        /// <inheritdoc/>
        public SaveEventsToRepo(IServiceScopeFactory services, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            _services = services;
        }

        protected override async IAsyncEnumerable<CompressedEventLogBase> Process(CompressedEventLogBase input, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            using (var scope = _services.CreateAsyncScope())
            {
                var repo = scope.ServiceProvider.GetService<IEventLogRepository>();

                //var searchLog = await repo.LookupAsync(input);

                //if (searchLog != null)
                //{
                //    dynamic list = Activator.CreateInstance(typeof(List<>).MakeGenericType(input.DataType));

                //    foreach(var i in Enumerable.Union(searchLog.Data, input.Data).ToHashSet())
                //    {
                //        if (list is IList l)
                //        {
                //            l.Add(i);
                //        }
                //    }

                //    searchLog.Data = list;

                //    await repo.UpdateAsync(searchLog);
                //}
                //else
                //{
                //    await repo.AddAsync(input);
                //}

                //yield return input;

                yield return await repo.Upsert(input);
            }
        }
    }
}