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
using Microsoft.Extensions.Options;
using Utah.Udot.Atspm.Configuration;
using Utah.Udot.Atspm.Infrastructure.Services.HostedServices;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Services;

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
        private readonly IEventLogRepository _repository;
        private readonly ILogger _log;

        /// <inheritdoc/>
        public LoggingController(IEventLogRepository repository, ILogger<EventLogController> log)
        {
            _repository = repository;
            _log = log;
        }

        /// <summary>
        /// This will kick off the workflow to pull events.
        /// </summary>
        [HttpGet("log")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async void SyncNewLocationEventsAsync(string locationId)
        {
            //ILogger<DeviceEventLogHostedService> log, IServiceScopeFactory serviceProvider, IOptions<DeviceEventLoggingConfiguration> options

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<DeviceEventLogHostedService> logger = loggerFactory.CreateLogger<DeviceEventLogHostedService>();

            IServiceScopeFactory serviceScopeFactory = HttpContext.RequestServices.GetRequiredService<IServiceScopeFactory>();

            using (var scope = serviceScopeFactory.CreateScope())
            {
                var deviceRepository = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();
                var deviceDownloader = scope.ServiceProvider.GetRequiredService<IDeviceDownloader>();
                var eventLogImporter = scope.ServiceProvider.GetRequiredService<IEventLogImporter>();
                var eventLogRepository = scope.ServiceProvider.GetRequiredService<IEventLogRepository>();
            }

            var deviceEventLoggingConfig = new DeviceEventLoggingConfiguration
            {
                // Populate with required configuration values
                DeviceEventLoggingQueryOptions = new DeviceEventLoggingQueryOptions
                {
                    IncludedLocations = new List<string> { locationId }

                }
            };
            IOptions<DeviceEventLoggingConfiguration> options = Options.Create(deviceEventLoggingConfig);


            DeviceEventLogHostedService test = new(logger, serviceScopeFactory, options);
            await test.StartAsync(default);
            await test.StopAsync(default);
        }

    }
}
