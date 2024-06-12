#region license
// Copyright 2024 Utah Departement of Transportation
// for DataApi - %Namespace%/OldEventLogController.cs
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
using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Data.Models;
using Google.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace ATSPM.DataApi.Controllers
{
    /// <summary>
    /// Location controller event log data
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class OldEventLogController : ControllerBase
    {
        private readonly IControllerEventLogRepository _repository;
        private readonly ILogger _log;

        /// <inheritdoc/>
        public OldEventLogController(IControllerEventLogRepository repository, ILogger<OldEventLogController> log)
        {
            _repository = repository;
            _log = log;
        }

        /// <summary>
        /// Get Location events between dates
        /// </summary>
        /// <param name="locationIdentifier">Location identifier</param>
        /// <param name="start">Date/time of first event. Example: <c>2023-02-09T08:15:30.0</c></param>
        /// <param name="end">Date/time of last event. Example: <c>2023-02-09T11:59:59.5</c></param>
        /// <returns>List of ControllerEventLogs</returns>
        /// <response code="200">Call completed successfully</response>
        /// <response code="400">Invalid request (start/end range)</response>
        /// <response code="404">Resource not found</response>
        //[ApiVersion("2.0")]
        [HttpGet("{locationIdentifier}")]
        [Produces("application/json", "application/xml", "text/csv")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<ControllerEventLog>> GetLocationEventsBetweenDates(string locationIdentifier, DateTime start, DateTime end)
        {
            _log.LogDebug("Location: {Location} start: {start} end: {end}", locationIdentifier, start, end);

            if (start == DateTime.MinValue || end == DateTime.MinValue)
                return BadRequest("Invalid datetime range on start/end");
            
            var result = _repository.GetLocationEventsBetweenDates(locationIdentifier, start, end);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        /// <summary>
        /// Get Location events between dates with event code
        /// </summary>
        /// <param name="locationIdentifier">Location identifier</param>
        /// <param name="start">date/time of first event</param>
        /// <param name="end">date/time of last event</param>
        /// <returns>List of ControllerEventLogs</returns>
        /// <response code="200">Call completed successfully</response>
        /// <response code="400">Invalid request (start/end range or event code)</response>
        /// <response code="404">Resource not found</response>
        //[ApiVersion("1.0")]
        [HttpGet("[Action]/{locationIdentifier}")]
        [Produces("application/json", "application/xml", "text/csv")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<ControllerEventLog>> GetLocationEventsByEventCode(string locationIdentifier, DateTime start, DateTime end, [FromQuery] IEnumerable<int> eventCode)
        {
            //var eventCode = new List<int>();
            
            _log.LogDebug("Location: {Location} event: {event} start: {start} end: {end}", locationIdentifier, eventCode, start, end);

            Console.WriteLine($"events: {eventCode.Count()}");

            if (start == DateTime.MinValue || end == DateTime.MinValue)
                return BadRequest("Invalid datetime range on start/end");

            var result = _repository.GetLocationEventsByEventCode(locationIdentifier, start, end, eventCode.First());

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        /// <summary>
        /// Get Location events between dates with list of event codes and parameters
        /// </summary>
        /// <param name="locationIdentifier"></param>
        /// <param name="start">date/time of first event</param>
        /// <param name="end">date/time of last event</param>
        /// <param name="body">List of event codes and parameters</param>
        /// <returns>List of ControllerEventLogs</returns>
        /// <response code="200">Call completed successfully</response>
        /// <response code="400">Invalid request (start/end range)</response>
        /// <response code="404">Resource not found</response>
        [HttpPost("{locationIdentifier}")]
        [Produces("application/json", "application/xml", "text/csv")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<ControllerEventLog>> GetRecordsByParameterAndEvent(string locationIdentifier, DateTime start, DateTime end, [FromBody] TestInput body)
        {
            _log.LogDebug("Location: {Location} start: {start} end: {end}", locationIdentifier, start, end);

            if (start == DateTime.MinValue || end == DateTime.MinValue)
                return BadRequest("Invalid datetime range on start/end");

            var result = _repository.GetRecordsByParameterAndEvent(locationIdentifier, start, end, body.Params, body.Codes);

            if (result == null)
                return NotFound();

            return Ok(result);
        }
    }

    public class TestInput
    {
        public List<int> Codes { get; set; }
        public List<int> Params { get; set; }
    }
}
