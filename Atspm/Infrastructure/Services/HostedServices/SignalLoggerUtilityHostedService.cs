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
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Infrastructure.Services.HostedServices
{
    public class LocationLoggerUtilityHostedService : IHostedService
    {
        private readonly ILogger _log;
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<DeviceEventLoggingConfiguration> _options;

        public LocationLoggerUtilityHostedService(ILogger<LocationLoggerUtilityHostedService> log, IServiceProvider serviceProvider, IOptions<DeviceEventLoggingConfiguration> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _log.LogInformation("*********************************Starting Service*********************************");

            _serviceProvider.PrintHostInformation();


            //foreach (var d in _options.Value.IncludedLocations)
            //{
            //    _log.LogInformation("Including Event Logs for locations: {location}", d);
            //}

            //foreach (var d in _options.Value.ExcludedLocations)
            //{
            //    _log.LogInformation("Excluding Event Logs for locations: {location}", d);
            //}

            //foreach (var d in _options.Value.IncludedAreas)
            //{
            //    _log.LogInformation("Including Event Logs for area: {area}", d);
            //}

            //foreach (var d in _options.Value.IncludedJurisdictions)
            //{
            //    _log.LogInformation("Including Event Logs for jurisdiction: {jurisdiction}", d);
            //}

            //foreach (var d in _options.Value.IncludedRegions)
            //{
            //    _log.LogInformation("Including Event Logs for region: {region}", d);
            //}

            //foreach (var d in _options.Value.IncludedLocationTypes)
            //{
            //    _log.LogInformation("Including Event Logs for location type: {locationtype}", d);
            //}

            //_log.LogInformation("Filtering event logs by device type: {devicetype}", _options.Value.DeviceType);

            //_log.LogInformation("Filtering event logs by transport protocol: {protocol}", _options.Value.TransportProtocol);

            //Console.WriteLine("*********************************Starting Service*********************************");





            var sw = new Stopwatch();
            sw.Start();

            using (var scope = _serviceProvider.CreateScope())
            {
                var query = scope.ServiceProvider.GetService<IDeviceRepository>().GetActiveDevicesByAllLatestLocations().AsQueryable();

                if (_options.Value.IncludedLocations?.Count() > 0)
                {
                    foreach (var d in _options.Value.IncludedLocations)
                    {
                        _log.LogInformation("Including Event Logs for locations: {location}", d);
                    }

                    query = query.Where(i => _options.Value.IncludedLocations.Any(d => i.Location.LocationIdentifier == d));
                }

                if (_options.Value.ExcludedLocations?.Count() > 0)
                {
                    foreach (var d in _options.Value.ExcludedLocations)
                    {
                        _log.LogInformation("Excluding Event Logs for locations: {location}", d);
                    }

                    query = query.Where(i => !_options.Value.ExcludedLocations.Any(d => i.Location.LocationIdentifier == d));
                }

                if (_options.Value.IncludedAreas?.Count() > 0)
                {
                    foreach (var d in _options.Value.IncludedAreas)
                    {
                        _log.LogInformation("Including Event Logs for area: {area}", d);
                    }

                    query = query.Where(i => _options.Value.IncludedAreas.Any(d => i.Location.Areas.Any(a => a.Name == d)));
                }

                if (_options.Value.IncludedJurisdictions?.Count() > 0)
                {
                    foreach (var d in _options.Value.IncludedJurisdictions)
                    {
                        _log.LogInformation("Including Event Logs for jurisdiction: {jurisdiction}", d);
                    }

                    query = query.Where(i => _options.Value.IncludedJurisdictions.Any(d => i.Location.Jurisdiction.Name == d));
                }

                if (_options.Value.IncludedRegions?.Count() > 0)
                {
                    foreach (var d in _options.Value.IncludedRegions)
                    {
                        _log.LogInformation("Including Event Logs for region: {region}", d);
                    }

                    query = query.Where(i => _options.Value.IncludedRegions.Any(d => i.Location.Region.Description == d));
                }

                if (_options.Value.IncludedLocationTypes?.Count() > 0)
                {
                    foreach (var d in _options.Value.IncludedLocationTypes)
                    {
                        _log.LogInformation("Including Event Logs for location type: {locationtype}", d);
                    }

                    query = query.Where(i => _options.Value.IncludedLocationTypes.Any(d => i.Location.LocationType.Name == d));
                }

                if (_options.Value.DeviceType != DeviceTypes.Unknown)
                {
                    _log.LogInformation("Filtering event logs by device type: {devicetype}", _options.Value.DeviceType);

                    query = query.Where(i => i.DeviceType == _options.Value.DeviceType);
                }

                if (_options.Value.TransportProtocol != TransportProtocols.Unknown)
                {
                    _log.LogInformation("Filtering event logs by transport protocol: {protocol}", _options.Value.TransportProtocol);

                    query = query.Where(i => i.DeviceConfiguration.Protocol == _options.Value.TransportProtocol);
                }

                var devices = query.ToList();

                Console.WriteLine($"devices: {devices.Count()}");

                foreach (var d in devices)
                {
                    Console.WriteLine($"device: {d}");
                }











                //var input = new BufferBlock<Device>();

                //var downloadStep = new DownloadDeviceData(_serviceProvider, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = _options.Value.MaxDegreeOfParallelism, CancellationToken = cancellationToken });
                //var processEventLogFileWorkflow = new ProcessEventLogFileWorkflow<IndianaEvent>(_serviceProvider, _options.Value.SaveToDatabaseBatchSize, _options.Value.MaxDegreeOfParallelism);

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
}