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
        public async void SyncNewLocationEventsAsync(int locationId)
        {
            var host = Host.CreateDefaultBuilder()
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

            var locationDevices = _repository.GetActiveDevicesByLocation(locationId);

            if (locationDevices == null || !locationDevices.Any())
                return;

            var workflowTasks = new List<Task>();

            using (var scope = host.Services.CreateScope())
            {
                foreach (var device in locationDevices)
                {
                    var workflow = new DeviceEventLogWorkflow(scope.ServiceProvider.GetService<IServiceScopeFactory>(), 50000, 1);
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    // Start the workflow
                    var workflowTask = Task.Run(async () =>
                    {
                        await workflow.Input.SendAsync(device);
                        workflow.Input.Complete();
                        await Task.WhenAll(workflow.Steps.Select(s => s.Completion));
                    });

                    workflowTasks.Add(workflowTask);
                }
            }
            // Wait for all workflows to complete
            await Task.WhenAll(workflowTasks);
        }

    }
}
