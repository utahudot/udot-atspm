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
using Utah.Udot.Atspm.Infrastructure.Services.HostedServices;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.ATSPM.DataApi.Controllers;

namespace Utah.Udot.Atspm.DataApi.Controllers
{
    /// <summary>
    /// Event log controller
    /// for querying raw device log data
    /// </summary>
    [ApiVersion("1.0")]
    [Authorize(Policy = "CanViewData")]
    public class EventLogController(IEventLogRepository repository, ILocationRepository locations, ILogger<AggregationController> log) : DataControllerBase(locations, log)
    {
        private readonly IEventLogRepository _repository = repository;

        /// <summary>
        /// Retrieves the available EventLogModelBase derived types defined in the system.
        /// </summary>
        /// <returns>
        /// A list of event type names.
        /// </returns>
        /// <response code="200">
        /// Call completed successfully and returns the list of aggregation types.
        /// </response>
        /// <response code="500">
        /// An unexpected error occurred while retrieving aggregation types.
        /// </response>
        [HttpGet("[Action]")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public ActionResult<IReadOnlyList<string>> GetDataTypes()
        {
            try
            {
                var result = typeof(EventLogModelBase)
                    .ListDerivedTypes()
                    .ToList()
                    .AsReadOnly();

                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Unexpected error",
                    Detail = "An error occurred while retrieving aggregation types.",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Streams archived aggregation records for a specific location within a given date range.
        /// </summary>
        /// <param name="locationIdentifier">
        /// Unique identifier of the location whose aggregations are requested.
        /// </param>
        /// <param name="start">
        /// Inclusive start date of the archive range. Must be less than or equal to <paramref name="end"/>.</param>
        /// <param name="end">
        /// Inclusive end date of the archive range. Must be greater than or equal to <paramref name="start"/>.</param>
        /// <param name="cancellationToken">
        /// Token automatically provided by ASP.NET Core. Consumers can cancel the stream by aborting the HTTP request.
        /// </param>
        /// <remarks>
        /// ### Response Format
        /// - Content type: <c>application/x-ndjson</c>
        /// - Each line is a JSON object representing a <see cref="CompressedAggregationBase"/> record.
        /// - Clients should parse line-by-line rather than expecting a JSON array.
        ///
        /// ### Example Request
        /// - GET /api/v1/Aggregation/ndjson/1014?start=2024-01-01&amp;end=2024-12-31
        ///
        /// ### Example Response (NDJSON)
        /// ```
        /// {"locationId":"1014","start":"2024-01-01T00:00:00Z","end":"2024-01-01T23:59:59Z"}
        /// {"locationId":"1014","start":"2024-01-02T00:00:00Z","end":"2024-01-02T23:59:59Z"}
        /// ```
        ///
        /// ### Streaming and Cancellation
        /// - Results are streamed using <c>IAsyncEnumerable</c>.
        /// - Clients can cancel by aborting the HTTP request (e.g., disposing <c>HttpClient</c> in .NET or calling <c>AbortController.abort()</c> in JavaScript).
        /// - Cancellation immediately stops enumeration and closes the response.
        /// </remarks>
        /// <response code="200">Aggregations successfully streamed (may be empty).</response>
        /// <response code="400">Invalid date range or malformed request.</response>
        /// <response code="404">Location not found.</response>
        [HttpGet("ndjson/{locationIdentifier}")]
        [Produces("application/x-ndjson")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)] // NDJSON represented as text
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> StreamAggregations(
            [FromRoute] string locationIdentifier,
            [FromQuery] DateTime start,
            [FromQuery] DateTime end,
            CancellationToken cancellationToken)
        {
            var error = await ValidateInputs(locationIdentifier, start, end);
            if (error != null) return error;

            Response.ContentType = "application/x-ndjson";

            await foreach (var item in _repository.GetArchivedAggregations(locationIdentifier, start, end).WithCancellation(cancellationToken))
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(item);
                await Response.WriteAsync(json + "\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }

            return new EmptyResult();
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
    }
}
