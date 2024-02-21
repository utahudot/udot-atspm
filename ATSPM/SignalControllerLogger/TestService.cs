using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Application.Services;
using ATSPM.Data.Models.EventLogModels;
using ATSPM.Data.Models;
using ATSPM.Domain.Workflows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using ATSPM.Domain.Extensions;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data.Enums;
using System.Diagnostics;

namespace ATSPM.LocationControllerLogger
{
    public class TestService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public TestService(IServiceProvider serviceProvider, ILogger<TestService> log)
        {
            _serviceProvider = serviceProvider;
            _logger = log;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("*********************************Starting Service*********************************");
            cancellationToken.Register(() => _logger.LogInformation("StartAsync has been cancelled"));



            _serviceProvider.PrintHostInformation();

            using (var scope = _serviceProvider.CreateScope())
            {
                ////var ftpDevices = scope.ServiceProvider.GetService<IDeviceRepository>().GetActiveDevicesByAllLatestLocations()
                ////    .Where(w => w.Ipaddress.ToString() != "10.10.10.10")
                ////    .Where(w => w.Ipaddress.IsValidIPAddress())
                ////    .Where(w => w.DeviceConfiguration.Protocol == TransportProtocols.Ftp)
                ////    .Take(3);

                ////var httpDevices = scope.ServiceProvider.GetService<IDeviceRepository>().GetActiveDevicesByAllLatestLocations()
                ////    .Where(w => w.Ipaddress.ToString() != "10.10.10.10")
                ////    .Where(w => w.Ipaddress.IsValidIPAddress())
                ////    .Where(w => w.DeviceConfiguration.Protocol == TransportProtocols.Http)
                ////    .Take(3);

                var sw = new Stopwatch();
                sw.Start();

                var sftpDevices = scope.ServiceProvider.GetService<IDeviceRepository>().GetActiveDevicesByAllLatestLocations()
                    .Where(w => w.Ipaddress.ToString() != "10.10.10.10")
                    .Where(w => w.Ipaddress.IsValidIPAddress())
                    .Where(w => w.DeviceConfiguration.Protocol == TransportProtocols.Sftp)
                    //.Where(w => w.DeviceConfiguration.Protocol != TransportProtocols.Http)
                    .OrderBy(o => o.Ipaddress.ToString());
                //.Skip(10)
                //.Take(100);

                //var devices = sftpDevices.Where(w => w.Ipaddress.IsValidIPAddress(true));
                var devices = sftpDevices;


                Console.WriteLine($"devices: {devices.Count()}");

                foreach (var d in devices)
                {
                    Console.WriteLine($"device: {d}");
                }


                int instances = 1;


                var input = new BufferBlock<Device>();

                var downloadStep = new DownloadDeviceData(_serviceProvider, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = instances, CancellationToken = cancellationToken });
                var processEventLogFileWorkflow = new ProcessEventLogFileWorkflow<IndianaEvent>(_serviceProvider, instances);
                var SaveEventsToRepo = new SaveEventsToRepo(_serviceProvider, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1, CancellationToken = cancellationToken });

                var actionResult = new ActionBlock<CompressedEventLogBase>(t =>
                {
                    Console.WriteLine($"{t.LocationIdentifier} - {t.ArchiveDate} - {t.DeviceId} - {t.Data.Count()}");

                    //var repo = scope.ServiceProvider.GetService<IEventLogRepository>();

                    //var i = await repo.LookupAsync(t);
                    //Console.WriteLine($"======================={i.LocationIdentifier} - {i.ArchiveDate} - {i.DeviceId} - {i.Data.Count()}=======================");

                    //foreach (var i in repo.GetList())
                    //{
                    //    Console.WriteLine($"{i.LocationIdentifier} - {i.ArchiveDate} - {i.DeviceId} - {i.Data.Count()}");
                    //}
                }, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = instances, CancellationToken = cancellationToken });

                input.LinkTo(downloadStep, new DataflowLinkOptions() { PropagateCompletion = true });

                await Task.Delay(TimeSpan.FromSeconds(1));

                downloadStep.LinkTo(processEventLogFileWorkflow.Input, new DataflowLinkOptions() { PropagateCompletion = true });
                processEventLogFileWorkflow.Output.LinkTo(SaveEventsToRepo, new DataflowLinkOptions() { PropagateCompletion = true });
                SaveEventsToRepo.LinkTo(actionResult, new DataflowLinkOptions() { PropagateCompletion = true });

                foreach (var d in devices)
                {
                    input.Post(d);
                }

                input.Complete();

                try
                {
                    await actionResult.Completion.ContinueWith(t => Console.WriteLine($"!!!Task actionResult is complete!!! {t.Status}"));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{actionResult.Completion.Status}---------------{e}");
                }


                sw.Stop();

                Console.WriteLine($"*********************************************complete - {sw.Elapsed}");

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("*********************************Stopping Service*********************************");
            cancellationToken.Register(() => _logger.LogInformation("StopAsync has been cancelled"));

            return Task.CompletedTask;
        }
    }

    public class DownloadDeviceData : TransformManyProcessStepBaseAsync<Device, Tuple<Device, FileInfo>>
    {
        private readonly IServiceProvider _serviceProvider;

        /// <inheritdoc/>
        public DownloadDeviceData(IServiceProvider serviceProvider, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            _serviceProvider = serviceProvider;
        }

        protected override IAsyncEnumerable<Tuple<Device, FileInfo>> Process(Device input, CancellationToken cancelToken = default)
        {
            using (var scope = _serviceProvider.CreateAsyncScope())
            {
                var downloader = scope.ServiceProvider.GetServices<IDeviceDownloader>().First(c => c.CanExecute(input));

                return downloader.Execute(input, cancelToken);
            }
        }
    }

    public class DecodeDeviceData<T> : TransformManyProcessStepBaseAsync<Tuple<Device, FileInfo>, Tuple<Device, T>> where T : EventLogModelBase
    {
        private readonly IServiceProvider _serviceProvider;

        /// <inheritdoc/>
        public DecodeDeviceData(IServiceProvider serviceProvider, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            _serviceProvider = serviceProvider;
        }

        protected override IAsyncEnumerable<Tuple<Device, T>> Process(Tuple<Device, FileInfo> input, CancellationToken cancelToken = default)
        {
            try
            {
                using (var scope = _serviceProvider.CreateAsyncScope())
                {
                    var decoder = scope.ServiceProvider.GetServices<ILocationControllerDecoder<T>>().First(c => c.CanExecute(input));

                    //Console.WriteLine($"device: {input.DeviceConfiguration.Protocol} - downloader: {downloader.GetType().Name}");

                    return decoder.Execute(input, cancelToken);
                }
            }
            catch (Exception)
            {
            }

            return default;
        }
    }

    public class ArchiveDeviceData<T> : TransformManyProcessStepBaseAsync<Tuple<Device, T>[], CompressedEventLogs<T>> where T : EventLogModelBase
    {
        /// <inheritdoc/>
        public ArchiveDeviceData(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override async IAsyncEnumerable<CompressedEventLogs<T>> Process(Tuple<Device, T>[] input, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            var result = input.GroupBy(g => (g.Item2.LocationIdentifier, g.Item2.Timestamp.Date, g.Item1.Id))
                .Select(s => new CompressedEventLogs<T>()
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

    public class SaveEventsToRepo : TransformManyProcessStepBaseAsync<CompressedEventLogBase, CompressedEventLogBase>
    {
        private readonly IServiceProvider _serviceProvider;

        /// <inheritdoc/>
        public SaveEventsToRepo(IServiceProvider serviceProvider, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async IAsyncEnumerable<CompressedEventLogBase> Process(CompressedEventLogBase input, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            using (var scope = _serviceProvider.CreateAsyncScope())
            {
                var repo = scope.ServiceProvider.GetService<IEventLogRepository>();

                var searchLog = await repo.LookupAsync(input);

                if (searchLog != null)
                {
                    var eventLogs = new HashSet<EventLogModelBase>(Enumerable.Union(searchLog.Data, input.Data));

                    searchLog.Data = eventLogs.ToList();

                    await repo.UpdateAsync(searchLog);
                }
                else
                {
                    await repo.AddAsync(input);
                }

                yield return input;
            }
        }
    }

    //public class ArchiveDeviceData<T> : TransformManyProcessStepBaseAsync<EventLogModelBase[], CompressedEventLogs<T>> where T : EventLogModelBase
    //{
    //    private readonly IServiceProvider _serviceProvider;

    //    /// <inheritdoc/>
    //    public ArchiveDeviceData(IServiceProvider serviceProvider, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
    //    {
    //        _serviceProvider = serviceProvider;
    //    }

    //    protected override IAsyncEnumerable<CompressedEventLogs<T>> Process(EventLogModelBase[] input, CancellationToken cancelToken = default)
    //    {
    //        using (var scope = _serviceProvider.CreateAsyncScope())
    //        {
    //            / t.GroupBy(g => (g.Timestamp.Date, g.LocationIdentifier))
    //                //.Select(s => new CompressedEventLogs<>() { SignalIdentifier = s.Key.LocationIdentifier, ArchiveDate = s.Key.Date, LogData = t.ToList() });

    //            var repo = scope.ServiceProvider.GetService<IEventLogRepository>();

    //            var searchLog = await repo.LookupAsync(t);

    //            if (searchLog != null)
    //            {
    //                var eventLogs = new HashSet<EventLogModelBase>(Enumerable.Union(searchLog.LogData, archive.LogData));

    //                searchLog.Data = eventLogs.ToList();

    //                await EventLogArchive.UpdateAsync(searchLog);

    //                result.Add(searchLog);
    //            }
    //            else
    //            {
    //                await EventLogArchive.AddAsync(archive);
    //                result.Add(archive);
    //            }
    //        }
    //    }
    //}

    public class ProcessEventLogFileWorkflow<T> : WorkflowBase<Tuple<Device, FileInfo>, CompressedEventLogs<T>> where T : EventLogModelBase
    {
        private readonly DataflowBlockOptions _filterOptions = new DataflowBlockOptions();
        private readonly ExecutionDataflowBlockOptions _stepOptions = new ExecutionDataflowBlockOptions();

        private readonly IServiceProvider _serviceProvider;

        public ProcessEventLogFileWorkflow(IServiceProvider serviceProvider, int maxDegreeOfParallelism = 1, CancellationToken cancellationToken = default)
        {
            _serviceProvider = serviceProvider;

            _filterOptions.CancellationToken = cancellationToken;
            _stepOptions.CancellationToken = cancellationToken;
            _stepOptions.MaxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        public DecodeDeviceData<T> DecodeDeviceDataStep { get; private set; }
        public BatchBlock<Tuple<Device, T>> BatchLogsStep { get; private set; }
        public ArchiveDeviceData<T> ArchiveDeviceDataStep { get; private set; }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            DecodeDeviceDataStep = new(_serviceProvider, _stepOptions);
            BatchLogsStep = new(50000);
            ArchiveDeviceDataStep = new(_stepOptions);
        }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(DecodeDeviceDataStep);
            Steps.Add(BatchLogsStep);
            Steps.Add(ArchiveDeviceDataStep);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(DecodeDeviceDataStep, new DataflowLinkOptions() { PropagateCompletion = true });
            DecodeDeviceDataStep.LinkTo(BatchLogsStep, new DataflowLinkOptions() { PropagateCompletion = true });
            BatchLogsStep.LinkTo(ArchiveDeviceDataStep, new DataflowLinkOptions() { PropagateCompletion = true });
            ArchiveDeviceDataStep.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }
}
