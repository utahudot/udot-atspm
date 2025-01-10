#region license
// Copyright 2024 Utah Departement of Transportation
// for DataApi - Utah.Udot.Atspm.DataApi.Controllers/EventLogController.cs
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

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.ValueObjects;
using Utah.Udot.ATSPM.Infrastructure.Workflows;

namespace Utah.Udot.Atspm.DataApi.Controllers
{
    /// <summary>
    /// Event log controller
    /// for querying raw device log data
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class LoggingController : ControllerBase
    {
        private readonly IDeviceRepository _repository;
        private readonly ILogger _log;

        /// <inheritdoc/>
        public LoggingController(IDeviceRepository deviceRepository, ILogger<EventLogController> log)
        {
            _repository = deviceRepository;
            _log = log;
        }

        /// <summary>
        /// This will kick off the workflow to pull events.
        /// </summary>
        [HttpGet("log")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<List<DeviceEventDownload>> SyncNewLocationEventsAsync(string locationIdentifier, string deviceIds)
        {
            var host = Host.CreateDefaultBuilder().ConfigureAppConfiguration((h, c) =>
            {
                c.AddUserSecrets<Program>(optional: true);
            })
                .ConfigureServices((h, s) =>
                {
                    s.AddAtspmDbContext(h);
                    s.AddAtspmEFConfigRepositories();
                    s.AddAtspmEFEventLogRepositories();
                    s.AddAtspmEFAggregationRepositories();
                    s.AddDownloaderClients();
                    s.AddDeviceDownloaders(h);
                    s.AddEventLogDecoders();
                    s.AddEventLogImporters(h);
                }).Build();

            var deviceIdList = deviceIds.Split(',').Select(int.Parse).ToList();
            var devices = _repository.GetList().Where(device => deviceIdList.Contains(device.Id) && (device.Location.LocationIdentifier == locationIdentifier)).ToList();
            var today = DateTime.Today;
            var start = DateOnly.FromDateTime(today);
            var end = DateOnly.FromDateTime(today);
            List<DeviceEventDownload> devicesEventDownload = new List<DeviceEventDownload>();

            if (devices == null || !devices.Any())
                return devicesEventDownload;

            using (var scope = host.Services.CreateScope())
            {
                var eventLogRepository = scope.ServiceProvider.GetService<IEventLogRepository>();
                var workflow = new DeviceEventLogWorkflow(scope.ServiceProvider.GetService<IServiceScopeFactory>(), 50000, 1);
                await Task.Delay(TimeSpan.FromSeconds(2));

                foreach (var device in devices)
                {
                    var eventsPreWorkflow = eventLogRepository.GetArchivedEvents(locationIdentifier, start, end, device.Id).SelectMany(s => s.Data).ToList();
                    // Start the workflow
                    await Task.Run(async () =>
                    {
                        await workflow.Input.SendAsync(device);
                        workflow.Input.Complete();
                        await Task.WhenAll(workflow.Steps.Select(s => s.Completion));
                    });
                    var eventsPostWorkflow = eventLogRepository.GetArchivedEvents(locationIdentifier, start, end, device.Id).SelectMany(s => s.Data).ToList();
                    var integer = 4;
                    var deviceDownload = new DeviceEventDownload
                    {
                        DeviceId = device.Id,
                        Ipaddress = device.Ipaddress,
                        DeviceType = device.DeviceType,
                        BeforeWorkflowEventCount = eventsPreWorkflow.Count,
                        AfterWorkflowEventCount = eventsPostWorkflow.Count,
                        ChangeInEventCount = eventsPostWorkflow.Count - eventsPreWorkflow.Count
                    };
                    devicesEventDownload.Add(deviceDownload);
                }
            }
            return devicesEventDownload;
        }

    }
}
