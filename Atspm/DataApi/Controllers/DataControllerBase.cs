#region license
// Copyright 2025 Utah Departement of Transportation
// for DataApi - Utah.Udot.ATSPM.DataApi.Controllers/DataControllerBase.cs
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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Utah.Udot.Atspm.Extensions;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;

namespace Utah.Udot.ATSPM.DataApi.Controllers
{
    /// <summary>
    /// Provides a base implementation for API controllers that handle data operations.
    /// </summary>
    /// <remarks>
    /// This abstract base class defines common dependencies and validation logic used by
    /// derived controllers. It ensures consistent input validation for location identifiers
    /// and date ranges across multiple endpoints.
    /// </remarks>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public abstract class DataControllerBase<T1, T2>(ICompressedDataRepository<T1> repository, ILocationRepository locations, ILogger log) : ControllerBase where T1 : CompressedDataBase where T2 : class
    {
        private readonly ICompressedDataRepository<T1> _repository = repository;

        /// <summary>
        /// Repository used to validate and interact with location data.
        /// </summary>
        protected readonly ILocationRepository _locations = locations;

        /// <summary>
        /// Logger instance used for diagnostic and error logging within derived controllers.
        /// </summary>
        protected readonly ILogger _log = log;

        /// <summary>
        /// Retrieves the available derived data types defined in the system.
        /// </summary>
        /// <returns>
        /// A list of derived type names.
        /// </returns>
        /// <response code="200">
        /// Call completed successfully and returns the list of data types.
        /// </response>
        /// <response code="500">
        /// An unexpected error occurred while retrieving data types.
        /// </response>
        [HttpGet("[Action]")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public ActionResult<IReadOnlyList<string>> GetDataTypes()
        {
            try
            {
                var result = typeof(T2)
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
                    Detail = "An error occurred while retrieving data types.",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Streams archived data records for a specific location within a given date range.
        /// </summary>
        /// <param name="locationIdentifier">
        /// Unique identifier of the location whose data are requested.
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
        /// - Each line is a JSON object representing a <typeparamref name="T1"/> record.
        /// - Clients should parse line-by-line rather than expecting a JSON array.
        ///
        /// ### Example Request
        /// - GET /StreamData/1014?start=2024-01-01&amp;end=2024-12-31
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
        /// <response code="200">Data successfully streamed (may be empty).</response>
        /// <response code="400">Invalid date range or malformed request.</response>
        /// <response code="404">Location not found.</response>
        [HttpGet("[Action]/{locationIdentifier}")]
        [Produces("application/x-ndjson")]
        //[ProducesResponseType(typeof(string), StatusCodes.Status200OK)] // NDJSON represented as text
        [SwaggerResponse(StatusCodes.Status200OK, "Data successfully streamed", typeof(CompressedDataBase), contentTypes: new[] { "application/x-ndjson" })]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> StreamData(
            [FromRoute] string locationIdentifier,
            [FromQuery] DateTime start,
            [FromQuery] DateTime end,
            CancellationToken cancellationToken)
        {
            var error = await ValidateInputs(locationIdentifier, start, end);
            if (error != null) return error;

            Response.ContentType = "application/x-ndjson";

            await foreach (var item in _repository.GetData(locationIdentifier, start, end).WithCancellation(cancellationToken))
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(item);
                await Response.WriteAsync(json + "\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }

            return new EmptyResult();
        }

        /// <summary>
        /// Retrieves archived data records for a specific location within a given date range.
        /// </summary>
        /// <param name="locationIdentifier">
        /// Unique identifier of the location whose data are requested.
        /// </param>
        /// <param name="start">
        /// Inclusive start date of the archive range. Must be less than or equal to <paramref name="end"/>.</param>
        /// <param name="end">
        /// Inclusive end date of the archive range. Must be greater than or equal to <paramref name="start"/>.</param>
        /// <param name="cancellationToken">
        /// Token automatically provided by ASP.NET Core. Consumers can cancel the request by aborting the HTTP call.
        /// </param>
        /// <remarks>
        /// ### Response Format
        /// - Content type: <c>application/json</c>
        /// - Response is a JSON array of <see cref="CompressedDataBase"/> objects.
        ///
        /// ### Example Request
        /// - GET /GetData/1014?start=2024-01-01&amp;end=2024-12-31
        ///
        /// ### Example Response (JSON Array)
        /// ```json
        /// [
        ///   {"locationId":"1014","start":"2024-01-01T00:00:00Z","end":"2024-01-01T23:59:59Z"},
        ///   {"locationId":"1014","start":"2024-01-02T00:00:00Z","end":"2024-01-02T23:59:59Z"}
        /// ]
        /// ```
        /// </remarks>
        /// <response code="200">Data successfully retrieved (may be empty).</response>
        /// <response code="400">Invalid date range or malformed request.</response>
        /// <response code="404">Location not found.</response>
        [HttpGet("[action]/{locationIdentifier}")]
        [Produces("application/json", "text/csv")]
        [ProducesResponseType(typeof(IEnumerable<CompressedDataBase>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<T1>>> GetData(
            [FromRoute] string locationIdentifier,
            [FromQuery] DateTime start,
            [FromQuery] DateTime end,
            CancellationToken cancellationToken)
        {
            var error = await ValidateInputs(locationIdentifier, start, end);
            if (error != null) return error;

            var list = await _repository.GetData(locationIdentifier, start, end).ToListAsync(cancellationToken);

            return Ok(list);
        }

        /// <summary>
        /// Streams archived data records for a specific location and data type within a given date range.
        /// </summary>
        /// <param name="locationIdentifier">
        /// Unique identifier of the location whose data are requested.</param>
        /// <param name="start">
        /// Inclusive start date of the archive range. Must be less than or equal to <paramref name="end"/>.</param>
        /// <param name="end">
        /// Inclusive end date of the archive range. Must be greater than or equal to <paramref name="start"/>.</param>
        /// <param name="dataType">
        /// Name of the data type. Must map to a valid CLR type.</param>
        /// <param name="cancellationToken">
        /// Token automatically provided by ASP.NET Core. Consumers can cancel the stream by aborting the HTTP request.</param>
        /// <remarks>
        /// ### Response Format
        /// - Content type: <c>application/x-ndjson</c>
        /// - Each line is a JSON object representing a CompressedDataBase record.
        /// - Clients should parse line-by-line rather than expecting a JSON array.
        ///
        /// ### Example Request
        /// - GET /StreamData/1014?start=2024-01-01&amp;end=2024-12-31&amp;dataType=[dataType]
        ///
        /// ### Example Response (NDJSON)
        /// ```
        /// {"locationId":"1014","start":"2024-01-01T00:00:00Z","end":"2024-01-01T23:59:59Z"}
        /// {"locationId":"1014","start":"2024-01-02T00:00:00Z","end":"2024-01-02T23:59:59Z"}
        /// ```
        /// </remarks>
        /// <response code="200">Data successfully streamed (may be empty).</response>
        /// <response code="400">Invalid date range, malformed request, or invalid data type.</response>
        /// <response code="404">Location not found.</response>
        [HttpGet("[action]/{locationIdentifier}/{dataType}")]
        [Produces("application/x-ndjson")]
        //[ProducesResponseType(typeof(string), StatusCodes.Status200OK)] // NDJSON represented as text
        [SwaggerResponse(StatusCodes.Status200OK, "Data successfully streamed", typeof(CompressedDataBase), contentTypes: new[] { "application/x-ndjson" })]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> StreamData(
        [FromRoute] string locationIdentifier,
        [FromRoute] string dataType,
        [FromQuery] DateTime start,
        [FromQuery] DateTime end,
            CancellationToken cancellationToken)
        {
            var error = await ValidateInputs(locationIdentifier, start, end);
            if (error != null) return error;

            var typeError = ValidateDataType(dataType, out var type);
            if (typeError != null) return typeError;

            Response.ContentType = "application/x-ndjson";

            await foreach (var item in _repository.GetData(locationIdentifier, start, end, type).WithCancellation(cancellationToken))
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(item);
                await Response.WriteAsync(json + "\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }

            return new EmptyResult();
        }

        /// <summary>
        /// Retrieves archived data records for a specific location and data type within a given date range.
        /// </summary>
        /// <param name="locationIdentifier">
        /// Unique identifier of the location whose data are requested.</param>
        /// <param name="start">
        /// Inclusive start date of the archive range. Must be less than or equal to <paramref name="end"/>.</param>
        /// <param name="end">
        /// Inclusive end date of the archive range. Must be greater than or equal to <paramref name="start"/>.</param>
        /// <param name="dataType">
        /// Name of the data type. Must map to a valid CLR type.</param>
        /// <param name="cancellationToken">
        /// Token automatically provided by ASP.NET Core. Consumers can cancel the request by aborting the HTTP call.</param>
        /// <remarks>
        /// ### Response Format
        /// - Content type: <c>application/json</c>
        /// - Response is a JSON array of <see cref="CompressedDataBase"/> objects.
        ///
        /// ### Example Request
        /// - GET /GetData/1014?start=2024-01-01&amp;end=2024-12-31&amp;dataType=[dataType]
        ///
        /// ### Example Response (JSON Array)
        /// ```json
        /// [
        ///   {"locationId":"1014","start":"2024-01-01T00:00:00Z","end":"2024-01-01T23:59:59Z"},
        ///   {"locationId":"1014","start":"2024-01-02T00:00:00Z","end":"2024-01-02T23:59:59Z"}
        /// ]
        /// ```
        /// </remarks>
        /// <response code="200">Data successfully retrieved (may be empty).</response>
        /// <response code="400">Invalid date range, malformed request, or invalid data type.</response>
        /// <response code="404">Location not found.</response>
        [HttpGet("[action]/{locationIdentifier}/{dataType}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<CompressedDataBase>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<T1>>> GetData(
            [FromRoute] string locationIdentifier,
            [FromRoute] string dataType,
            [FromQuery] DateTime start,
            [FromQuery] DateTime end,
            CancellationToken cancellationToken)
        {
            var error = await ValidateInputs(locationIdentifier, start, end);
            if (error != null) return error;

            var typeError = ValidateDataType(dataType, out var type);
            if (typeError != null) return typeError;

            var list = await _repository.GetData(locationIdentifier, start, end, type)
                                        .ToListAsync(cancellationToken);
            return Ok(list);
        }

        /// <summary>
        /// Retrieves the distinct days that contain data for a specific location and data type
        /// within the given date range.
        /// </summary>
        /// <remarks>
        /// This endpoint allows anonymous access and returns a JSON array of <see cref="DateOnly"/> values.
        /// Each value represents a day for which data exists. The request must specify a valid
        /// location identifier, data type, and date range. If the inputs are invalid or the location/data type
        /// cannot be resolved, an appropriate error response is returned.
        /// </remarks>
        /// <param name="locationIdentifier">
        /// The unique identifier of the location whose data is requested.
        /// </param>
        /// <param name="dataType">
        /// The name of the event log data type. Must map to a valid derived type of <c>EventLogModelBase</c>.
        /// </param>
        /// <param name="start">
        /// The inclusive start date/time of the range. Must be less than or equal to <paramref name="end"/>.
        /// </param>
        /// <param name="end">
        /// The inclusive end date/time of the range. Must be greater than or equal to <paramref name="start"/>.
        /// </param>
        /// <response code="200">Data successfully retrieved (may be empty).</response>
        /// <response code="400">Invalid date range, malformed request, or invalid data type.</response>
        /// <response code="404">Location not found.</response>
        [AllowAnonymous]
        [HttpGet("[action]/{locationIdentifier}/{dataType}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<DateOnly>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<DateOnly>>> GetDaysWithData(
            [FromRoute] string locationIdentifier,
            [FromRoute] string dataType,
            [FromQuery] DateTime start,
            [FromQuery] DateTime end)
        {
            var error = await ValidateInputs(locationIdentifier, start, end);
            if (error != null) return error;

            var typeError = ValidateDataType(dataType, out var type);
            if (typeError != null) return typeError;

            var result = _repository.GetDaysWithData(locationIdentifier, type, start, end);

            return Ok(result);
        }

        /// <summary>
        /// Validates common input parameters for data queries.
        /// </summary>
        /// <param name="locationIdentifier">
        /// Unique identifier of the location whose data is being requested.
        /// </param>
        /// <param name="start">
        /// Inclusive start date of the query range. Must be less than or equal to <paramref name="end"/>.
        /// </param>
        /// <param name="end">
        /// Inclusive end date of the query range. Must be greater than or equal to <paramref name="start"/>.
        /// </param>
        /// <returns>
        /// Returns <see cref="BadRequestObjectResult"/> if the date range is invalid,
        /// <see cref="NotFoundObjectResult"/> if the location does not exist,
        /// or <c>null</c> if validation succeeds.
        /// </returns>
        /// <remarks>
        /// This method is intended to be called at the beginning of controller actions
        /// to enforce uniform validation rules. Derived controllers can override this
        /// method to extend or customize validation behavior.
        /// </remarks>
        protected virtual async Task<ActionResult?> ValidateInputs(string locationIdentifier, DateTime start, DateTime end)
        {
            if (end <= start)
                return BadRequest(new ProblemDetails { Title = "Invalid date range", Detail = "End must be > start" });

            if (!await _locations.LocationExists(locationIdentifier))
                return NotFound(new ProblemDetails { Title = "Location not found", Detail = $"No location '{locationIdentifier}' exists." });

            return null; // success
        }

        /// <summary>
        /// Validates that the requested data type maps to a known CLR type.
        /// </summary>
        /// <param name="dataType">
        /// The name of the data type requested.
        /// </param>
        /// <param name="type">
        /// When validation succeeds, contains the resolved CLR <see cref="Type"/>.
        /// </param>
        /// <returns>
        /// Returns <see cref="BadRequestObjectResult"/> if the data type is invalid,
        /// or <c>null</c> if validation succeeds.
        /// </returns>
        protected ActionResult? ValidateDataType(string dataType, out Type? type)
        {
            if (!typeof(T2).ToDictionary().TryGetValue(dataType, out type))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid data type",
                    Detail = $"The specified data type '{dataType}' is not recognized.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            return null; // success
        }
    }
}
