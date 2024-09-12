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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Analysis.WorkflowFilters;
using Utah.Udot.Atspm.Analysis.Workflows;
using Utah.Udot.Atspm.Analysis.WorkflowSteps;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.NetStandardToolkit.Workflows;

namespace Utah.Udot.Atspm.Infrastructure.Services.HostedServices
{
    public class LocationLoggerUtilityHostedService : IHostedService
    {
        private readonly ILogger _log;
        private readonly IServiceScopeFactory _services;
        private readonly IOptions<DeviceEventLoggingConfiguration> _options;

        public LocationLoggerUtilityHostedService(ILogger<LocationLoggerUtilityHostedService> log, IServiceScopeFactory serviceProvider, IOptions<DeviceEventLoggingConfiguration> options) =>
            (_log, _services, _options) = (log, serviceProvider, options);

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _log.LogInformation("*********************************Starting Service*********************************");


            var sw = new Stopwatch();
            sw.Start();

            Console.WriteLine($"path1: {_options.Value.Path}");

            using (var scope = _services.CreateAsyncScope())
            {
                var workflow = new DeviceEventLogWorkflow(_services);

                workflow.Initialized += (s,a) => Console.WriteLine($"{s.GetType().Name} - initialized");

                var repo = scope.ServiceProvider.GetService<IDeviceRepository>();

                await foreach(var d in repo.GetDevicesForLogging(_options.Value.DeviceEventLoggingQueryOptions))
                {
                    Console.WriteLine($"device: {d}");

                    await workflow.Input.SendAsync(d);
                }

                workflow.Input.Complete();


                await workflow.Output.Completion;







                //var input = new BufferBlock<Device>();

                //var downloadStep = new DownloadDeviceData(_services, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = _options.Value.MaxDegreeOfParallelism, CancellationToken = cancellationToken });
                //var processEventLogFileWorkflow = new ProcessEventLogFileWorkflow<IndianaEvent>(_services, _options.Value.SaveToDatabaseBatchSize, _options.Value.MaxDegreeOfParallelism);

                //var actionResult = new ActionBlock<Tuple<Device, FileInfo>>(t =>
                //{
                //    //Console.WriteLine($"{t.Item1} - {t.Item2.FullName}");
                //}, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = _options.Value.MaxDegreeOfParallelism, CancellationToken = cancellationToken });

                //input.LinkTo(downloadStep, new DataflowLinkOptions() { PropagateCompletion = true });

                //await Task.Delay(TimeSpan.FromSeconds(1));

                //downloadStep.LinkTo(actionResult, new DataflowLinkOptions() { PropagateCompletion = true });

                //foreach (var d in devices)
                //{
                //    input.Post(d);
                //}

                //input.Complete();

                //try
                //{
                //    await actionResult.Completion.ContinueWith(t => Console.WriteLine($"!!!Task actionResult is complete!!! {t.Status}"), cancellationToken);
                //}
                //catch (Exception e)
                //{
                //    Console.WriteLine($"{actionResult.Completion.Status}---------------{e}");
                //}

                sw.Stop();

                Console.WriteLine($"*********************************************complete - {sw.Elapsed}");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _log.LogInformation("*********************************Stopping Service*********************************");
            cancellationToken.Register(() => _log.LogInformation("StopAsync has been cancelled"));

            return Task.CompletedTask;
        }
    }

    public class DeviceEventLogWorkflow : WorkflowBase<Device, CompressedEventLogs<EventLogModelBase>>
    {
        private readonly ExecutionDataflowBlockOptions _stepOptions = new ExecutionDataflowBlockOptions();
        private readonly IServiceScopeFactory _services;
        private readonly int _batchSize;

        /// <inheritdoc/>
        public DeviceEventLogWorkflow(IServiceScopeFactory services, int batchSize = 50000)
        {
            _services = services;
            _batchSize = batchSize;
        }

        public DownloadDeviceData DownloadDeviceData { get; private set; }
        public DecodeDeviceData DecodeDeviceData { get; private set; }
        public BatchBlock<Tuple<Device, EventLogModelBase>> BatchEventLogs { get; private set; }
        public ArchiveDeviceData ArchiveDeviceData { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(DownloadDeviceData);
            Steps.Add(DecodeDeviceData);
            Steps.Add(BatchEventLogs);
            Steps.Add(ArchiveDeviceData);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            DownloadDeviceData = new(_services, _stepOptions);
            DecodeDeviceData = new(_services, _stepOptions);
            BatchEventLogs = new(_batchSize);
            ArchiveDeviceData = new(_stepOptions);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(DownloadDeviceData, new DataflowLinkOptions() { PropagateCompletion = true });
            DownloadDeviceData.LinkTo(DecodeDeviceData, new DataflowLinkOptions() { PropagateCompletion = true });
            DecodeDeviceData.LinkTo(BatchEventLogs, new DataflowLinkOptions() { PropagateCompletion = true });
            BatchEventLogs.LinkTo(ArchiveDeviceData, new DataflowLinkOptions() { PropagateCompletion = true });
            ArchiveDeviceData.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }

    public class DownloadDeviceData : TransformManyProcessStepBaseAsync<Device, Tuple<Device, FileInfo>>
    {
        private readonly IServiceScopeFactory _services;

        /// <inheritdoc/>
        public DownloadDeviceData(IServiceScopeFactory services, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            _services = services;
        }

        protected override IAsyncEnumerable<Tuple<Device, FileInfo>> Process(Device input, CancellationToken cancelToken = default)
        {
            using (var scope = _services.CreateAsyncScope())
            {
                var downloader = scope.ServiceProvider.GetServices<IDeviceDownloader>().First(c => c.CanExecute(input));

                return downloader.Execute(input, cancelToken);
            }
        }
    }

    //public class DecodeDeviceData<T> : TransformManyProcessStepBaseAsync<Tuple<Device, FileInfo>, Tuple<Device, T>> where T : EventLogModelBase
    public class DecodeDeviceData : TransformManyProcessStepBaseAsync<Tuple<Device, FileInfo>, Tuple<Device, EventLogModelBase>>
    {
        private readonly IServiceScopeFactory _services;

        /// <inheritdoc/>
        public DecodeDeviceData(IServiceScopeFactory services, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            _services = services;
        }

        protected override IAsyncEnumerable<Tuple<Device, EventLogModelBase>> Process(Tuple<Device, FileInfo> input, CancellationToken cancelToken = default)
        {
            using (var scope = _services.CreateAsyncScope())
            {
                var importer = scope.ServiceProvider.GetService<IEventLogImporter>();

                return importer.Execute(input, cancelToken);
            }
        }
    }

    //public class ArchiveDeviceData<T> : TransformManyProcessStepBaseAsync<Tuple<Device, T>[], CompressedEventLogs<T>> where T : EventLogModelBase
    public class ArchiveDeviceData : TransformManyProcessStepBaseAsync<Tuple<Device, EventLogModelBase>[], CompressedEventLogs<EventLogModelBase>>
    {
        /// <inheritdoc/>
        public ArchiveDeviceData(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override async IAsyncEnumerable<CompressedEventLogs<EventLogModelBase>> Process(Tuple<Device, EventLogModelBase>[] input, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            var result = input.GroupBy(g => (g.Item2.LocationIdentifier, g.Item2.Timestamp.Date, g.Item1.Id))
                .Select(s => new CompressedEventLogs<EventLogModelBase>()
                {
                    LocationIdentifier = s.Key.LocationIdentifier,
                    ArchiveDate = DateOnly.FromDateTime(s.Key.Date),
                    DeviceId = s.Key.Id,
                    Data = s.Select(s => s.Item2).ToList()
                });

            foreach (var r in result)
            {
                yield return r;
            }
        }
    }
}