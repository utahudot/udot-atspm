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
using Swashbuckle.AspNetCore.Annotations;
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
    public class EventLogController(IEventLogRepository repository, ILocationRepository locations, IDeviceRepository devices, ILogger<EventLogController> log)
        : DataControllerBase<CompressedEventLogBase, EventLogModelBase>(repository, locations, log)
    {
        private readonly IEventLogRepository _repository = repository;
        private readonly IDeviceRepository _devices = devices;

        /// <summary>
        /// Streams archived data records for a specific location, device, and data type within a given date range.
        /// </summary>
        /// <param name="locationIdentifier">
        /// Unique identifier of the location whose data are requested.
        /// </param>
        /// <param name="deviceId">
        /// Unique identifier of the device whose data are requested.
        /// </param>
        /// <param name="start">
        /// Inclusive start date of the archive range. Must be less than or equal to <paramref name="end"/>.
        /// </param>
        /// <param name="end">
        /// Inclusive end date of the archive range. Must be greater than or equal to <paramref name="start"/>.
        /// </param>
        /// <param name="cancellationToken">
        /// Token automatically provided by ASP.NET Core. Consumers can cancel the stream by aborting the HTTP request.
        /// </param>
        /// <remarks>
        /// ### Response Format
        /// - Content type: <c>application/x-ndjson</c>
        /// - Each line is a JSON object representing a <see cref="CompressedEventLogBase"/> record.
        /// - Clients should parse line-by-line rather than expecting a JSON array.
        ///
        /// ### Example Request
        /// - GET /StreamData/1014?deviceId=42&amp;start=2024-01-01&amp;end=2024-12-31&amp;dataType=[dataType]
        /// </remarks>
        /// <response code="200">Data successfully streamed (may be empty).</response>
        /// <response code="400">Invalid date range, malformed request, or invalid data type.</response>
        /// <response code="404">Location not found.</response>
        [HttpGet("[Action]/{locationIdentifier}/{deviceId:int}")]
        [Produces("application/x-ndjson")]
        //[ProducesResponseType(typeof(string), StatusCodes.Status200OK)] // NDJSON represented as text
        [SwaggerResponse(StatusCodes.Status200OK, "Data successfully streamed", typeof(CompressedDataBase), contentTypes: new[] { "application/x-ndjson" })]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> StreamData(
            [FromRoute] string locationIdentifier,
            [FromRoute] int deviceId,
            [FromQuery] DateTime start,
            [FromQuery] DateTime end,
            CancellationToken cancellationToken)
        {
            var error = await ValidateInputs(locationIdentifier, start, end);
            if (error != null) return error;

            if (!await _devices.DeviceExists(deviceId))
                return NotFound(new ProblemDetails { Title = "Device not found", Detail = $"No device id '{deviceId}' exists." });

            Response.ContentType = "application/x-ndjson";

            await foreach (var item in _repository.GetData(locationIdentifier, start, end, deviceId)
                                                  .WithCancellation(cancellationToken))
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(item);
                await Response.WriteAsync(json + "\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }

            return new EmptyResult();
        }

        /// <summary>
        /// Retrieves archived data records for a specific location, device, and data type within a given date range.
        /// </summary>
        /// <param name="locationIdentifier">
        /// Unique identifier of the location whose data are requested.
        /// </param>
        /// <param name="deviceId">
        /// Unique identifier of the device whose data are requested.
        /// </param>
        /// <param name="start">
        /// Inclusive start date of the archive range. Must be less than or equal to <paramref name="end"/>.
        /// </param>
        /// <param name="end">
        /// Inclusive end date of the archive range. Must be greater than or equal to <paramref name="start"/>.
        /// </param>
        /// <param name="cancellationToken">
        /// Token automatically provided by ASP.NET Core. Consumers can cancel the request by aborting the HTTP call.
        /// </param>
        /// <remarks>
        /// ### Response Format
        /// - Content type: <c>application/json</c>
        /// - Response is a JSON array of <see cref="CompressedEventLogBase"/> objects.
        /// </remarks>
        /// <response code="200">Data successfully retrieved (may be empty).</response>
        /// <response code="400">Invalid date range, malformed request, or invalid data type.</response>
        /// <response code="404">Location not found.</response>
        [HttpGet("[Action]/{locationIdentifier}/{deviceId:int}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<CompressedEventLogBase>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<CompressedEventLogBase>>> GetData(
            [FromRoute] string locationIdentifier,
            [FromRoute] int deviceId,
            [FromQuery] DateTime start,
            [FromQuery] DateTime end,
            CancellationToken cancellationToken)
        {
            var error = await ValidateInputs(locationIdentifier, start, end);
            if (error != null) return error;

            if (!await _devices.DeviceExists(deviceId))
                return NotFound(new ProblemDetails { Title = "Device not found", Detail = $"No device id '{deviceId}' exists." });

            var list = await _repository.GetData(locationIdentifier, start, end, deviceId)
                                        .ToListAsync(cancellationToken);
            return Ok(list);
        }

        /// <summary>
        /// Streams archived data records for a specific location, device, and data type within a given date range.
        /// </summary>
        /// <param name="locationIdentifier">
        /// Unique identifier of the location whose data are requested.
        /// </param>
        /// <param name="deviceId">
        /// Unique identifier of the device whose data are requested.
        /// </param>
        /// <param name="start">
        /// Inclusive start date of the archive range. Must be less than or equal to <paramref name="end"/>.
        /// </param>
        /// <param name="end">
        /// Inclusive end date of the archive range. Must be greater than or equal to <paramref name="start"/>.
        /// </param>
        /// <param name="dataType">
        /// Name of the data type. Must map to a valid CLR type.
        /// </param>
        /// <param name="cancellationToken">
        /// Token automatically provided by ASP.NET Core. Consumers can cancel the stream by aborting the HTTP request.
        /// </param>
        /// <remarks>
        /// ### Response Format
        /// - Content type: <c>application/x-ndjson</c>
        /// - Each line is a JSON object representing a <see cref="CompressedEventLogBase"/> record.
        /// - Clients should parse line-by-line rather than expecting a JSON array.
        ///
        /// ### Example Request
        /// - GET /StreamData/1014?deviceId=42&amp;start=2024-01-01&amp;end=2024-12-31&amp;dataType=[dataType]
        /// </remarks>
        /// <response code="200">Data successfully streamed (may be empty).</response>
        /// <response code="400">Invalid date range, malformed request, or invalid data type.</response>
        /// <response code="404">Location not found.</response>
        [HttpGet("[action]/{locationIdentifier}/{dataType}/{deviceId:int}")]
        [Produces("application/x-ndjson")]
        //[ProducesResponseType(typeof(string), StatusCodes.Status200OK)] // NDJSON represented as text
        [SwaggerResponse(StatusCodes.Status200OK, "Data successfully streamed", typeof(CompressedDataBase), contentTypes: new[] { "application/x-ndjson" })]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> StreamData(
            [FromRoute] string locationIdentifier,
            [FromRoute] string dataType,
            [FromRoute] int deviceId,
            [FromQuery] DateTime start,
            [FromQuery] DateTime end,
            CancellationToken cancellationToken)
        {
            var error = await ValidateInputs(locationIdentifier, start, end);
            if (error != null) return error;

            if (!await _devices.DeviceExists(deviceId))
                return NotFound(new ProblemDetails { Title = "Device not found", Detail = $"No device id '{deviceId}' exists." });

            var typeError = ValidateDataType(dataType, out var type);
            if (typeError != null) return typeError;

            Response.ContentType = "application/x-ndjson";

            await foreach (var item in _repository.GetData(locationIdentifier, start, end, type, deviceId)
                                                  .WithCancellation(cancellationToken))
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(item);
                await Response.WriteAsync(json + "\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }

            return new EmptyResult();
        }

        /// <summary>
        /// Retrieves archived data records for a specific location, device, and data type within a given date range.
        /// </summary>
        /// <param name="locationIdentifier">
        /// Unique identifier of the location whose data are requested.
        /// </param>
        /// <param name="deviceId">
        /// Unique identifier of the device whose data are requested.
        /// </param>
        /// <param name="start">
        /// Inclusive start date of the archive range. Must be less than or equal to <paramref name="end"/>.
        /// </param>
        /// <param name="end">
        /// Inclusive end date of the archive range. Must be greater than or equal to <paramref name="start"/>.
        /// </param>
        /// <param name="dataType">
        /// Name of the data type. Must map to a valid CLR type.
        /// </param>
        /// <param name="cancellationToken">
        /// Token automatically provided by ASP.NET Core. Consumers can cancel the request by aborting the HTTP call.
        /// </param>
        /// <remarks>
        /// ### Response Format
        /// - Content type: <c>application/json</c>
        /// - Response is a JSON array of <see cref="CompressedEventLogBase"/> objects.
        /// </remarks>
        /// <response code="200">Data successfully retrieved (may be empty).</response>
        /// <response code="400">Invalid date range, malformed request, or invalid data type.</response>
        /// <response code="404">Location not found.</response>
        [HttpGet("[action]/{locationIdentifier}/{dataType}/{deviceId:int}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<CompressedEventLogBase>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<CompressedEventLogBase>>> GetData(
            [FromRoute] string locationIdentifier,
            [FromRoute] string dataType,
            [FromRoute] int deviceId,
            [FromQuery] DateTime start,
            [FromQuery] DateTime end,
            CancellationToken cancellationToken)
        {
            var error = await ValidateInputs(locationIdentifier, start, end);
            if (error != null) return error;

            if (!await _devices.DeviceExists(deviceId))
                return NotFound(new ProblemDetails { Title = "Device not found", Detail = $"No device id '{deviceId}' exists." });

            var typeError = ValidateDataType(dataType, out var type);
            if (typeError != null) return typeError;

            var list = await _repository.GetData(locationIdentifier, start, end, type, deviceId)
                                        .ToListAsync(cancellationToken);
            return Ok(list);
        }
    }
}
