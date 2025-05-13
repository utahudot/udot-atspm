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

namespace Utah.Udot.Atspm.DataApi.Controllers
{
    /// <summary>
    /// Event log controller
    /// for querying raw device log data
    /// </summary>
    [ApiVersion("1.0")]
    [Authorize(Policy = "CanViewData")]
    public class SpeedEventController : DataControllerBase
    {
        private readonly ILogger _log;
        private readonly IEventBusPublisher<SpeedEvent> _publisher;

        /// <inheritdoc/>
        public SpeedEventController( ILogger<EventLogController> log)
        {
            _log = log;
        }


        public SpeedEventController(IEventBusPublisher<SpeedEvent> publisher)
            => _publisher = publisher;

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<SpeedEvent> batch)
        {
            if (batch == null || batch.Count == 0)
                return BadRequest("No events provided.");

            // publish in parallel
            await Task.WhenAll(batch.Select(evt => _publisher.PublishAsync(evt, HttpContext.RequestAborted)));
            return Ok(new { published = batch.Count });
        }
    }
}
