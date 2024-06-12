#region license
// Copyright 2024 Utah Departement of Transportation
// for DataApi - ATSPM.DataApi.Controllers/EventLogController.cs
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
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATSPM.DataApi.Controllers
{
    /// <summary>
    /// Event log controller
    /// for querying raw device log data
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    [Authorize(Policy = "CanViewData")]
    public class EventLogController : ControllerBase
    {
        private readonly IEventLogRepository _repository;
        private readonly ILogger _log;

        /// <inheritdoc/>
        public EventLogController(IEventLogRepository repository, ILogger<EventLogController> log)
        {
            _repository = repository;
            _log = log;
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
        public IActionResult GetArchivedEvents(string locationIdentifier, DateOnly start, DateOnly end)
        {
            if (start == DateOnly.MinValue || end == DateOnly.MinValue || end < start)
                return BadRequest("Invalid date range");

            var result = _repository.GetArchivedEvents(locationIdentifier, start, end);

            if (result.Count == 0)
                return NotFound();

            HttpContext.Response.Headers.Add("X-Total-Count", result.Count.ToString());

            return Ok(result);
        }

        /// <summary>
        /// Get all event logs for location by date and device id
        /// </summary>
        /// <param name="locationIdentifier">Location identifier</param>
        /// <param name="deviceId">Deivce id events came from</param>
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
        public IActionResult GetArchivedEvents(string locationIdentifier, int deviceId, DateOnly start, DateOnly end)
        {
            if (start == DateOnly.MinValue || end == DateOnly.MinValue || end < start)
                return BadRequest("Invalid date range");

            if (deviceId == 0)
                return BadRequest("Invalid device id");

            var result = _repository.GetArchivedEvents(locationIdentifier, start, end, deviceId);

            if (result.Count == 0)
                return NotFound();

            HttpContext.Response.Headers.Add("X-Total-Count", result.Count.ToString());

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
        public IActionResult GetArchivedEvents(string locationIdentifier, string dataType, DateOnly start, DateOnly end)
        {
            if (start == DateOnly.MinValue || end == DateOnly.MinValue || end < start)
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

            HttpContext.Response.Headers.Add("X-Total-Count", result.Count.ToString());

            return Ok(result);
        }

        /// <summary>
        /// Get all event logs for location by date, device id and datatype
        /// </summary>
        /// <param name="locationIdentifier">Location identifier</param>
        /// <param name="deviceId">Deivce id events came from</param>
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
        public IActionResult GetArchivedEvents(string locationIdentifier, int deviceId, string dataType, DateOnly start, DateOnly end)
        {
            if (start == DateOnly.MinValue || end == DateOnly.MinValue || end < start)
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

            HttpContext.Response.Headers.Add("X-Total-Count", result.Count.ToString());

            return Ok(result);
        }
    }
}
