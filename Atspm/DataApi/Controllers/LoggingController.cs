#region license
// Copyright 2024 Utah Departement of Transportation
// for DataApi - Utah.Udot.Atspm.DataApi.Controllers/LoggingController.cs
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
using Microsoft.Extensions.Options;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Infrastructure.Configuration;
using Utah.Udot.Atspm.Infrastructure.Services.HostedServices;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.ValueObjects;
using Utah.Udot.ATSPM.Infrastructure.Workflows;

namespace Utah.Udot.Atspm.DataApi.Controllers
{
    /// <summary>
    /// logging controller
    /// for querying raw device log data
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
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
        public async Task<List<DeviceEventDownload>> SyncNewLocationEventsAsync(string deviceIds)
        {
            var host = Host.CreateDefaultBuilder().ConfigureAppConfiguration((h, c) =>
            {
                c.AddUserSecrets<Program>(optional: true);
            })
                .ApplyVolumeConfiguration()
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
            var devices = _repository.GetList().Where(device => device.LoggingEnabled == true && deviceIdList.Contains(device.Id)).ToList();
            DateTime end = DateTime.Now.AddHours(1); // Set end time to one hour in the future to ensure we capture all events up to now
            DateTime start = end.Date.AddDays(-1);

            List<DeviceEventDownload> devicesEventDownload = new List<DeviceEventDownload>();

            if (devices == null || !devices.Any())
                return devicesEventDownload;

            var semaphore = new SemaphoreSlim(3); // allow 3 concurrent workflows
            var tasks = new List<Task<DeviceEventDownload>>();

            foreach (var device in devices)
            {
                await semaphore.WaitAsync();

                var task = Task.Run(async () =>
                {
                    try
                    {
                        using (var scope = host.Services.CreateScope())
                        {
                            var eventLogRepository = scope.ServiceProvider.GetService<IEventLogRepository>();

                            var eventsPreWorkflow = eventLogRepository
                                .GetArchivedEvents(device.Location.LocationIdentifier, start, end, device.Id)
                                .SelectMany(s => s.Data)
                                .ToList();

                            var workflow = new DeviceEventLogWorkflow(scope.ServiceProvider.GetService<IServiceScopeFactory>(), 50000, 1);

                            await Task.Delay(TimeSpan.FromSeconds(2));

                            await workflow.Input.SendAsync(device);
                            workflow.Input.Complete();
                            await Task.WhenAll(workflow.Steps.Select(s => s.Completion));

                            var eventsPostWorkflow = eventLogRepository
                                .GetArchivedEvents(device.Location.LocationIdentifier, start, end, device.Id)
                                .SelectMany(s => s.Data)
                                .ToList();

                            return new DeviceEventDownload
                            {
                                DeviceId = device.Id,
                                Ipaddress = device.Ipaddress,
                                DeviceType = device.DeviceType,
                                BeforeWorkflowEventCount = eventsPreWorkflow.Count,
                                AfterWorkflowEventCount = eventsPostWorkflow.Count,
                                ChangeInEventCount = eventsPostWorkflow.Count - eventsPreWorkflow.Count
                            };
                        }
                    }
                    catch (Exception)
                    {
                        return new DeviceEventDownload
                        {
                            DeviceId = device.Id,
                            Ipaddress = device.Ipaddress,
                            DeviceType = device.DeviceType,
                        };
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                tasks.Add(task);
            }

            var results = await Task.WhenAll(tasks);
            devicesEventDownload.AddRange(results);

            //devicesEventDownload.AddRange(results);
            return devicesEventDownload;
        }



        [HttpPost("deviceEventLoggin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeviceEventLogging([FromServices] IServiceScopeFactory serviceScopeFactory, [FromServices] ILogger<DeviceEventLogHostedService> logger, DeviceEventLoggingConfiguration deviceIds)
        {
            var options = Options.Create(deviceIds);
            var eventloggingservice = new DeviceEventLogHostedService(logger, serviceScopeFactory, options);
            var ts = new CancellationTokenSource();
            await eventloggingservice.StartAsync(ts.Token);
            await eventloggingservice.StopAsync(ts.Token);
            return Ok("hello world");
        }
    }
}
