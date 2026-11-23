#region license
// Copyright 2026 Utah Departement of Transportation
// for DataApi - Utah.Udot.ATSPM.DataApi.Controllers/LocationDataControllerBase.cs
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
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Swashbuckle.AspNetCore.Annotations;
using Utah.Udot.Atspm.Common;
using Utah.Udot.Atspm.Data.Interfaces;
using Utah.Udot.Atspm.Infrastructure.Attributes;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.NetStandardToolkit.Common;

namespace Utah.Udot.ATSPM.DataApi.Controllers
{
    /// <summary>
    /// Provides a base implementation for API controllers that handle data operations.
    /// </summary>
    /// <typeparam name="T">
    /// The entity type returned by the controller. Must inherit from <see cref="StartEndRange"/> and implement <see cref="ILocationLayer"/>.
    /// </typeparam>
    /// <remarks>
    /// This abstract base class defines common dependencies and validation logic used by
    /// derived controllers. It ensures consistent input validation for location identifiers
    /// and date ranges across multiple endpoints.
    /// </remarks>
    [ApiController]
    [AuthorizePermission(AtspmAuthorization.Permissions.DataView)]
    [Route("api/v{version:apiVersion}/[controller]")]
    public abstract class LocationDataControllerBase<T>(ILocationDataRepository<T> repository, ILocationRepository locations, ILogger log)
        : ControllerBase where T : StartEndRange, ILocationLayer
    {
        private readonly ILocationDataRepository<T> _repository = repository;

        /// <summary>
        /// Repository used to validate and interact with location data.
        /// </summary>
        protected readonly ILocationRepository _locations = locations;

        /// <summary>
        /// Logger instance used for diagnostic and error logging within derived controllers.
        /// </summary>
        protected readonly ILogger _log = log;

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
        /// - Each line is a JSON object representing a <typeparamref name="T"/> record.
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
        [SwaggerResponse(StatusCodes.Status200OK, "Data successfully streamed", typeof(string), contentTypes: new[] { "application/x-ndjson" })]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> StreamData(
            [FromRoute] string locationIdentifier,
            [FromQuery, BindRequired] DateTime start,
            [FromQuery, BindRequired] DateTime end,
            CancellationToken cancellationToken)
        {
            var acceptError = ValidateNdjsonAcceptHeader();
            if (acceptError != null) return acceptError;

            var error = await ValidateInputs(locationIdentifier, start, end);
            if (error != null) return error;

            return await StreamNdjsonAsync(_repository.GetData(locationIdentifier, start, end), cancellationToken);
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
        /// - Response is a JSON array of <typeparamref name="T"/> objects.
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<T>>> GetData(
            [FromRoute] string locationIdentifier,
            [FromQuery, BindRequired] DateTime start,
            [FromQuery, BindRequired] DateTime end,
            CancellationToken cancellationToken)
        {
            var error = await ValidateInputs(locationIdentifier, start, end);
            if (error != null) return error;

            var list = await _repository.GetData(locationIdentifier, start, end).ToListAsync(cancellationToken);

            return Ok(list);
        }

        /// <summary>
        /// Retrieves the distinct days that contain data for a specific location
        /// within the given date range.
        /// </summary>
        /// <remarks>
        /// This endpoint allows anonymous access and returns a JSON array of <see cref="DateOnly"/> values.
        /// Each value represents a day for which data exists. The request must specify a valid
        /// location identifier and date range. If the inputs are invalid or the location
        /// cannot be resolved, an appropriate error response is returned.
        /// </remarks>
        /// <param name="locationIdentifier">
        /// The unique identifier of the location whose data is requested.
        /// </param>
        /// <param name="start">
        /// The inclusive start date/time of the range. Must be less than or equal to <paramref name="end"/>.
        /// </param>
        /// <param name="end">
        /// The inclusive end date/time of the range. Must be greater than or equal to <paramref name="start"/>.
        /// </param>
        /// <response code="200">Data successfully retrieved (may be empty).</response>
        /// <response code="400">Invalid date range or malformed request.</response>
        /// <response code="404">Location not found.</response>
        [AllowAnonymous]
        [HttpGet("[action]/{locationIdentifier}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<DateOnly>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<DateOnly>>> GetDaysWithData(
            [FromRoute] string locationIdentifier,
            [FromQuery, BindRequired] DateTime start,
            [FromQuery, BindRequired] DateTime end)
        {
            var error = await ValidateInputs(locationIdentifier, start, end);
            if (error != null) return error;

            var result = _repository.GetDaysWithData(locationIdentifier, start, end);

            return Ok(result);
        }

        /// <summary>
        /// Writes an asynchronous sequence of items to the HTTP response stream in NDJSON format.
        /// </summary>
        /// <param name="items">The asynchronous sequence of items to serialize and write.</param>
        /// <param name="cancellationToken">Token used to cancel streaming.</param>
        /// <returns>An <see cref="EmptyResult"/> once streaming completes.</returns>
        protected async Task<IActionResult> StreamNdjsonAsync(IAsyncEnumerable<T> items, CancellationToken cancellationToken)
        {
            var serializerSettings = HttpContext.RequestServices
                .GetService<IOptions<MvcNewtonsoftJsonOptions>>()?.Value.SerializerSettings;

            Response.ContentType = "application/x-ndjson";

            await foreach (var item in items.WithCancellation(cancellationToken))
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(item, serializerSettings);
                await Response.WriteAsync(json + "\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }

            return new EmptyResult();
        }

        private static readonly MediaTypeHeaderValue NdjsonMediaType = MediaTypeHeaderValue.Parse("application/x-ndjson");

        /// <summary>
        /// Validates that the request <c>Accept</c> header permits <c>application/x-ndjson</c> responses.
        /// </summary>
        /// <returns>
        /// Returns a <c>406 Not Acceptable</c> result if the client explicitly rejects <c>application/x-ndjson</c>,
        /// or <c>null</c> if acceptable.
        /// </returns>
        protected ActionResult? ValidateNdjsonAcceptHeader()
        {
            var acceptHeaders = Request.GetTypedHeaders().Accept;
            if (acceptHeaders != null && acceptHeaders.Count > 0 && !acceptHeaders.Any(h => NdjsonMediaType.IsSubsetOf(h)))
            {
                return StatusCode(StatusCodes.Status406NotAcceptable);
            }

            return null;
        }

        /// <summary>
        /// Creates an RFC 7807 <see cref="ProblemDetails"/> response serialized as <c>application/problem+json</c>
        /// regardless of the request's <c>Accept</c> header.
        /// </summary>
        /// <param name="statusCode">HTTP status code for the error response.</param>
        /// <param name="title">Short summary of the problem.</param>
        /// <param name="detail">Detailed explanation of the problem.</param>
        /// <returns>An <see cref="ActionResult"/> producing <c>application/problem+json</c>.</returns>
        protected static ActionResult CreateProblemResult(int statusCode, string title, string detail)
        {
            return new JsonResult(new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail
            })
            {
                StatusCode = statusCode,
                ContentType = "application/problem+json"
            };
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
        /// Returns a <c>400</c> <see cref="ProblemDetails"/> result if the date range is invalid,
        /// a <c>404</c> <see cref="ProblemDetails"/> result if the location does not exist,
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
                return CreateProblemResult(StatusCodes.Status400BadRequest, "Invalid date range", "End must be > start");

            if (!await _locations.LocationExists(locationIdentifier))
                return CreateProblemResult(StatusCodes.Status404NotFound, "Location not found", $"No location '{locationIdentifier}' exists.");

            return null;
        }
    }
}
