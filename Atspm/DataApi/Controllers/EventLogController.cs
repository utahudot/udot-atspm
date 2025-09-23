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
using Utah.Udot.Atspm.Extensions;
using Confluent.Kafka;
using Newtonsoft.Json;
using Utah.Udot.Atspm.Infrastructure.Messaging;
using System.Text;
using Utah.Udot.ATSPM.DataApi.Controllers;

namespace Utah.Udot.Atspm.DataApi.Controllers
{
    /// <summary>
    /// Event log controller
    /// for querying raw device log data
    /// </summary>
    [ApiVersion("1.0")]
    [Authorize(Policy = "CanViewData")]
    public class EventLogController : DataControllerBase
    {
        private readonly IEventLogRepository _repository;
        private readonly ILogger _log;
        private readonly IProducer<string, byte[]> _busProducer;

        /// <inheritdoc/>
        public EventLogController(IEventLogRepository repository, ILogger<EventLogController> log,
        IProducer<string, byte[]> busProducer)
        {
            _repository = repository;
            _log = log;
            _busProducer = busProducer;
        }

        /// <summary>
        /// Returns the possible event log data types
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Call completed successfully</response>
        [HttpGet("[Action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> GetDataTypes()
        {
            var result = typeof(EventLogModelBase).Assembly.GetTypes().Where(w => w.IsSubclassOf(typeof(EventLogModelBase))).Select(s => s.Name).ToList();

            return Ok(result);
        }

        /// <summary>
        /// Get all event logs for location by date
        /// </summary>
        /// <param name="locationIdentifier">Location identifier</param>
        /// <param name="start">Archive date of event to start with</param>
        /// <param name="end">Archive date of event to end with</param>
        /// <returns></returns>
        /// <response code="200">Call completed successfully</response>
        /// <response code="400">Invalid request (date)</response>
        /// <response code="404">Resource not found</response>
        [HttpGet("[Action]/{locationIdentifier}")]
        [Produces("application/json", "application/xml")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetArchivedEvents(string locationIdentifier, DateTime start, DateTime end)
        {
            if (start == DateTime.MinValue || end == DateTime.MinValue || end < start)
                return BadRequest("Invalid date range");

            var result = _repository.GetArchivedEvents(locationIdentifier, start, end);

            if (result.Count == 0)
                return NotFound();

            HttpContext.Response.Headers.Append("X-Total-Count", result.Count.ToString());

            return Ok(result);
        }

        /// <summary>
        /// Get all event logs for location by date and device id
        /// </summary>
        /// <param name="locationIdentifier">Location identifier</param>
        /// <param name="deviceId">Device id events came from</param>
        /// <param name="start">Archive date of event to start with</param>
        /// <param name="end">Archive date of event to end with</param>
        /// <returns></returns>
        /// <response code="200">Call completed successfully</response>
        /// <response code="400">Invalid request (date)</response>
        /// <response code="404">Resource not found</response>
        [HttpGet("[Action]/{locationIdentifier}/{deviceId:int}")]
        [Produces("application/json", "application/xml")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetArchivedEvents(string locationIdentifier, int deviceId, DateTime start, DateTime end)
        {
            if (start == DateTime.MinValue || end == DateTime.MinValue || end < start)
                return BadRequest("Invalid date range");

            if (deviceId == 0)
                return BadRequest("Invalid device id");

            var result = _repository.GetArchivedEvents(locationIdentifier, start, end, deviceId);

            if (result.Count == 0)
                return NotFound();

            HttpContext.Response.Headers.Append("X-Total-Count", result.Count.ToString());

            return Ok(result);
        }

        /// <summary>
        /// Get all event logs for location by date and datatype
        /// </summary>
        /// <param name="locationIdentifier">Location identifier</param>
        /// <param name="dataType">Type that inherits from <see cref="EventLogModelBase"/></param>
        /// <param name="start">Archive date of event to start with</param>
        /// <param name="end">Archive date of event to end with</param>
        /// <returns></returns>
        /// <response code="200">Call completed successfully</response>
        /// <response code="400">Invalid request (date)</response>
        /// <response code="404">Resource not found</response>
        [HttpGet("[Action]/{locationIdentifier}/{dataType}")]
        [Produces("application/json", "application/xml")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetArchivedEvents(string locationIdentifier, string dataType, DateTime start, DateTime end)
        {
            if (start == DateTime.MinValue || end == DateTime.MinValue || end < start)
                return BadRequest("Invalid date range");

            Type type;

            try
            {
                type = Type.GetType($"{typeof(EventLogModelBase).Namespace}.{dataType}, {typeof(EventLogModelBase).Assembly}", true);
            }
            catch (Exception)
            {
                return BadRequest("Invalid data type");
            }

            var result = _repository.GetArchivedEvents(locationIdentifier, start, end, type);

            if (result.Count == 0)
                return NotFound();

            HttpContext.Response.Headers.Append("X-Total-Count", result.Count.ToString());

            return Ok(result);
        }

        /// <summary>
        /// Get all event logs for location by date, device id and datatype
        /// </summary>
        /// <param name="locationIdentifier">Location identifier</param>
        /// <param name="deviceId">Device id events came from</param>
        /// <param name="dataType">Type that inherits from <see cref="EventLogModelBase"/></param>
        /// <param name="start">Archive date of event to start with</param>
        /// <param name="end">Archive date of event to end with</param>
        /// <returns></returns>
        /// <response code="200">Call completed successfully</response>
        /// <response code="400">Invalid request (date)</response>
        /// <response code="404">Resource not found</response>
        [HttpGet("[Action]/{locationIdentifier}/{deviceId:int}/{dataType}")]
        [Produces("application/json", "application/xml")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetArchivedEvents(string locationIdentifier, int deviceId, string dataType, DateTime start, DateTime end)
        {
            if (start == DateTime.MinValue || end == DateTime.MinValue || end < start)
                return BadRequest("Invalid date range");

            if (deviceId == 0)
                return BadRequest("Invalid device id");

            Type type;

            try
            {
                type = Type.GetType($"{typeof(EventLogModelBase).Namespace}.{dataType}, {typeof(EventLogModelBase).Assembly}", true);
            }
            catch (Exception)
            {
                return BadRequest("Invalid data type");
            }

            var result = _repository.GetArchivedEvents(locationIdentifier, start, end, type, deviceId);

            if (result.Count == 0)
                return NotFound();

            HttpContext.Response.Headers.Append("X-Total-Count", result.Count.ToString());

            return Ok(result);
        }

        /// <summary>
        /// Get all days that have event logs for a given location.
        /// </summary>
        /// <param name="locationIdentifier">Location identifier</param>
        /// <param name="dataType">Type that inherits from <see cref="EventLogModelBase"/></param>
        /// <returns>A list of unique days with event logs.</returns>
        /// <response code="200">Call completed successfully</response>
        /// <response code="404">Resource not found</response>
        [AllowAnonymous]
        [HttpGet("[Action]/{locationIdentifier}")]
        [Produces("application/json", "application/xml")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetDaysWithEventLogs(string locationIdentifier, string dataType, DateTime start, DateTime end)
        {

            Type type;

            try
            {
                type = Type.GetType($"{typeof(EventLogModelBase).Namespace}.{dataType}, {typeof(EventLogModelBase).Assembly}", true);
            }
            catch (Exception)
            {
                return BadRequest("Invalid data type");
            }

            var result = _repository.GetDaysWithEventLogs(locationIdentifier, type, start, end);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<EventBatchEnvelope> batches)
        {
            if (batches == null || batches.Count == 0)
                return BadRequest("No batches provided.");

            _log.LogInformation("Publishing {BatchCount} batches", batches.Count);

            var produceTasks = batches.Select(batch =>
            {
                // Serialize the envelope
                var envelopeJson = JsonConvert.SerializeObject(batch);
                var envelopeBytes = Encoding.UTF8.GetBytes(envelopeJson);

                // Key by LocationIdentifier, topic by EventType
                var message = new Message<string, byte[]>
                {
                    Key = batch.LocationIdentifier,
                    Value = envelopeBytes
                };

                return _busProducer.ProduceAsync("location-events", message);
            });

            await Task.WhenAll(produceTasks);

            return Ok(new { publishedBatches = batches.Count });
        }
    }
}
