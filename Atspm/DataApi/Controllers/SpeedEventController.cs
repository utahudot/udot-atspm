#region license
// Copyright 2025 Utah Departement of Transportation
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utah.Udot.ATSPM.DataApi.Controllers;
using Utah.Udot.Atspm.Extensions;
using Utah.Udot.Atspm.Services;
using Utah.Udot.Atspm.Infrastructure.Messaging;

namespace Utah.Udot.Atspm.DataApi.Controllers
{
    /// <summary>
    /// Event log controller
    /// for querying raw device log data
    /// </summary>
    [ApiVersion("1.0")]
    //[Authorize(Policy = "CanViewData")]
    public class SpeedEventController : DataControllerBase
    {
        private readonly ILogger<SpeedEventController> _log;
        private readonly IEventBusPublisher<RawSpeedPacket> _publisher;

        // Single constructor taking both dependencies
        public SpeedEventController(
            ILogger<SpeedEventController> log,
            IEventBusPublisher<RawSpeedPacket> publisher)
        {
            _log = log;
            _publisher = publisher;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<RawSpeedPacket> batch)
        {
            if (batch == null || batch.Count == 0)
                return BadRequest("No events provided.");

            _log.LogInformation("Publishing {Count} events to Kafka", batch.Count);
            await Task.WhenAll(batch.Select(evt =>
                _publisher.PublishAsync(evt, HttpContext.RequestAborted)));

            return Ok(new { published = batch.Count });
        }
    }
}
