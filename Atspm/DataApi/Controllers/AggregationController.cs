#region license
// Copyright 2025 Utah Departement of Transportation
// for DataApi - Utah.Udot.Atspm.DataApi.Controllers/AggregationController.cs
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
using Utah.Udot.Atspm.Infrastructure.Services.HostedServices;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.ATSPM.DataApi.Controllers;

namespace Utah.Udot.Atspm.DataApi.Controllers
{
    /// <summary>
    /// Aggregation controller
    /// for querying raw aggregation data
    /// </summary>
    /// <inheritdoc/>
    [ApiVersion("1.0")]
    [Authorize(Policy = "CanViewData")]
    public class AggregationController(IAggregationRepository repository, ILocationRepository locations, ILogger<AggregationController> log) 
        : DataControllerBase<CompressedAggregationBase, AggregationModelBase>(repository, locations, log)
    {
        private readonly IAggregationRepository _repository = repository;

        //protected override IAsyncEnumerable<CompressedAggregationBase> GetData(string locationIdentifier, DateTime start, DateTime end, Type? type = null)
        //{
        //    return type == null
        //        ? _repository.GetData(locationIdentifier, start, end)
        //        : _repository.GetData(locationIdentifier, start, end, type);
        //}

        ///// <summary>
        ///// Retrieves the available AggregationModelBase derived types defined in the system.
        ///// </summary>
        ///// <returns>
        ///// A list of aggregation type names.
        ///// </returns>
        ///// <response code="200">
        ///// Call completed successfully and returns the list of aggregation types.
        ///// </response>
        ///// <response code="500">
        ///// An unexpected error occurred while retrieving aggregation types.
        ///// </response>
        //[HttpGet("[Action]")]
        //[Produces("application/json")]
        //[ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        //public ActionResult<IReadOnlyList<string>> GetDataTypes()
        //{
        //    try
        //    {
        //        var result = typeof(AggregationModelBase)
        //            .ListDerivedTypes()
        //            .ToList()
        //            .AsReadOnly();

        //        return Ok(result);
        //    }
        //    catch (Exception)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        //        {
        //            Title = "Unexpected error",
        //            Detail = "An error occurred while retrieving aggregation types.",
        //            Status = StatusCodes.Status500InternalServerError
        //        });
        //    }
        //}

        ///// <summary>
        ///// Streams archived aggregation records for a specific location within a given date range.
        ///// </summary>
        ///// <param name="locationIdentifier">
        ///// Unique identifier of the location whose aggregations are requested.
        ///// </param>
        ///// <param name="start">
        ///// Inclusive start date of the archive range. Must be less than or equal to <paramref name="end"/>.</param>
        ///// <param name="end">
        ///// Inclusive end date of the archive range. Must be greater than or equal to <paramref name="start"/>.</param>
        ///// <param name="cancellationToken">
        ///// Token automatically provided by ASP.NET Core. Consumers can cancel the stream by aborting the HTTP request.
        ///// </param>
        ///// <remarks>
        ///// ### Response Format
        ///// - Content type: <c>application/x-ndjson</c>
        ///// - Each line is a JSON object representing a <see cref="CompressedAggregationBase"/> record.
        ///// - Clients should parse line-by-line rather than expecting a JSON array.
        /////
        ///// ### Example Request
        ///// - GET /api/v1/Aggregation/ndjson/1014?start=2024-01-01&amp;end=2024-12-31
        /////
        ///// ### Example Response (NDJSON)
        ///// ```
        ///// {"locationId":"1014","start":"2024-01-01T00:00:00Z","end":"2024-01-01T23:59:59Z"}
        ///// {"locationId":"1014","start":"2024-01-02T00:00:00Z","end":"2024-01-02T23:59:59Z"}
        ///// ```
        /////
        ///// ### Streaming and Cancellation
        ///// - Results are streamed using <c>IAsyncEnumerable</c>.
        ///// - Clients can cancel by aborting the HTTP request (e.g., disposing <c>HttpClient</c> in .NET or calling <c>AbortController.abort()</c> in JavaScript).
        ///// - Cancellation immediately stops enumeration and closes the response.
        ///// </remarks>
        ///// <response code="200">Aggregations successfully streamed (may be empty).</response>
        ///// <response code="400">Invalid date range or malformed request.</response>
        ///// <response code="404">Location not found.</response>
        //[HttpGet("ndjson/{locationIdentifier}")]
        //[Produces("application/x-ndjson")]
        //[ProducesResponseType(typeof(string), StatusCodes.Status200OK)] // NDJSON represented as text
        //[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        //public async Task<IActionResult> StreamAggregations(
        //    [FromRoute] string locationIdentifier,
        //    [FromQuery] DateTime start,
        //    [FromQuery] DateTime end,
        //    CancellationToken cancellationToken)
        //{
        //    var error = await ValidateInputs(locationIdentifier, start, end);
        //    if (error != null) return error;

        //    Response.ContentType = "application/x-ndjson";

        //    await foreach (var item in _repository.GetData(locationIdentifier, start, end).WithCancellation(cancellationToken))
        //    {
        //        var json = Newtonsoft.Json.JsonConvert.SerializeObject(item);
        //        await Response.WriteAsync(json + "\n", cancellationToken);
        //        await Response.Body.FlushAsync(cancellationToken);
        //    }

        //    return new EmptyResult();
        //}

        ///// <summary>
        ///// Retrieves archived aggregation records for a specific location within a given date range.
        ///// </summary>
        ///// <param name="locationIdentifier">
        ///// Unique identifier of the location whose aggregations are requested.
        ///// </param>
        ///// <param name="start">
        ///// Inclusive start date of the archive range. Must be less than or equal to <paramref name="end"/>.</param>
        ///// <param name="end">
        ///// Inclusive end date of the archive range. Must be greater than or equal to <paramref name="start"/>.</param>
        ///// <param name="cancellationToken">
        ///// Token automatically provided by ASP.NET Core. Consumers can cancel the request by aborting the HTTP call.
        ///// </param>
        ///// <remarks>
        ///// ### Response Format
        ///// - Content type: <c>application/json</c>
        ///// - Response is a JSON array of <see cref="CompressedAggregationBase"/> objects.
        /////
        ///// ### Example Request
        ///// - GET /api/v1/Aggregation/GetData/1014?start=2024-01-01&amp;end=2024-12-31
        /////
        ///// ### Example Response (JSON Array)
        ///// ```json
        ///// [
        /////   {"locationId":"1014","start":"2024-01-01T00:00:00Z","end":"2024-01-01T23:59:59Z"},
        /////   {"locationId":"1014","start":"2024-01-02T00:00:00Z","end":"2024-01-02T23:59:59Z"}
        ///// ]
        ///// ```
        ///// </remarks>
        ///// <response code="200">Aggregations successfully retrieved (may be empty).</response>
        ///// <response code="400">Invalid date range or malformed request.</response>
        ///// <response code="404">Location not found.</response>
        //[HttpGet("[action]/{locationIdentifier}")]
        //[Produces("application/json", "application/xml")]
        //[ProducesResponseType(typeof(IEnumerable<CompressedAggregationBase>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        //public async Task<ActionResult<IEnumerable<CompressedAggregationBase>>> GetData(
        //    [FromRoute] string locationIdentifier,
        //    [FromQuery] DateTime start,
        //    [FromQuery] DateTime end,
        //    CancellationToken cancellationToken)
        //{
        //    var error = await ValidateInputs(locationIdentifier, start, end);
        //    if (error != null) return error;

        //    var list = await _repository.GetData(locationIdentifier, start, end)
        //                                .ToListAsync(cancellationToken);

        //    return Ok(list);
        //}

        ///// <summary>
        ///// Streams archived aggregation records for a specific location and data type within a given date range.
        ///// </summary>
        ///// <param name="locationIdentifier">
        ///// Unique identifier of the location whose aggregations are requested.</param>
        ///// <param name="start">
        ///// Inclusive start date of the archive range. Must be less than or equal to <paramref name="end"/>.</param>
        ///// <param name="end">
        ///// Inclusive end date of the archive range. Must be greater than or equal to <paramref name="start"/>.</param>
        ///// <param name="dataType">
        ///// Name of the aggregation data type (e.g., "TrafficVolumeAggregation"). Must map to a valid CLR type.</param>
        ///// <param name="cancellationToken">
        ///// Token automatically provided by ASP.NET Core. Consumers can cancel the stream by aborting the HTTP request.</param>
        ///// <remarks>
        ///// ### Response Format
        ///// - Content type: <c>application/x-ndjson</c>
        ///// - Each line is a JSON object representing a <see cref="CompressedAggregationBase"/> record.
        ///// - Clients should parse line-by-line rather than expecting a JSON array.
        /////
        ///// ### Example Request
        ///// - GET /api/v1/Aggregation/ndjson/1014?start=2024-01-01&amp;end=2024-12-31&amp;dataType=TrafficVolumeAggregation
        /////
        ///// ### Example Response (NDJSON)
        ///// ```
        ///// {"locationId":"1014","start":"2024-01-01T00:00:00Z","end":"2024-01-01T23:59:59Z"}
        ///// {"locationId":"1014","start":"2024-01-02T00:00:00Z","end":"2024-01-02T23:59:59Z"}
        ///// ```
        ///// </remarks>
        ///// <response code="200">Aggregations successfully streamed (may be empty).</response>
        ///// <response code="400">Invalid date range, malformed request, or invalid data type.</response>
        ///// <response code="404">Location not found.</response>
        //[HttpGet("ndjson/{locationIdentifier}/{dataType}")]
        //[Produces("application/x-ndjson")]
        //[ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        //public async Task<IActionResult> StreamAggregations(
        //[FromRoute] string locationIdentifier,
        //[FromRoute] string dataType,
        //[FromQuery] DateTime start,
        //[FromQuery] DateTime end,
        //    CancellationToken cancellationToken)
        //{
        //    var error = await ValidateInputs(locationIdentifier, start, end);
        //    if (error != null) return error;

        //    if (!typeof(AggregationModelBase).ToDictionary().TryGetValue(dataType, out var type))
        //    {
        //        return BadRequest(new ProblemDetails
        //        {
        //            Title = "Invalid data type",
        //            Detail = $"The specified data type '{dataType}' is not recognized.",
        //            Status = StatusCodes.Status400BadRequest
        //        });
        //    }

        //    Response.ContentType = "application/x-ndjson";

        //    await foreach (var item in _repository.GetData(locationIdentifier, start, end, type).WithCancellation(cancellationToken))
        //    {
        //        var json = Newtonsoft.Json.JsonConvert.SerializeObject(item);
        //        await Response.WriteAsync(json + "\n", cancellationToken);
        //        await Response.Body.FlushAsync(cancellationToken);
        //    }

        //    return new EmptyResult();
        //}

        ///// <summary>
        ///// Retrieves archived aggregation records for a specific location and data type within a given date range.
        ///// </summary>
        ///// <param name="locationIdentifier">
        ///// Unique identifier of the location whose aggregations are requested.</param>
        ///// <param name="start">
        ///// Inclusive start date of the archive range. Must be less than or equal to <paramref name="end"/>.</param>
        ///// <param name="end">
        ///// Inclusive end date of the archive range. Must be greater than or equal to <paramref name="start"/>.</param>
        ///// <param name="dataType">
        ///// Name of the aggregation data type (derived from <see cref="CompressedAggregationBase"/>). Must map to a valid CLR type.</param>
        ///// <param name="cancellationToken">
        ///// Token automatically provided by ASP.NET Core. Consumers can cancel the request by aborting the HTTP call.</param>
        ///// <remarks>
        ///// ### Response Format
        ///// - Content type: <c>application/json</c>
        ///// - Response is a JSON array of <see cref="CompressedAggregationBase"/> objects.
        /////
        ///// ### Example Request
        ///// - GET /api/v1/Aggregation/GetData/1014?start=2024-01-01&amp;end=2024-12-31&amp;dataType=TrafficVolumeAggregation
        /////
        ///// ### Example Response (JSON Array)
        ///// ```json
        ///// [
        /////   {"locationId":"1014","start":"2024-01-01T00:00:00Z","end":"2024-01-01T23:59:59Z"},
        /////   {"locationId":"1014","start":"2024-01-02T00:00:00Z","end":"2024-01-02T23:59:59Z"}
        ///// ]
        ///// ```
        ///// </remarks>
        ///// <response code="200">Aggregations successfully retrieved (may be empty).</response>
        ///// <response code="400">Invalid date range, malformed request, or invalid data type.</response>
        ///// <response code="404">Location not found.</response>
        //[HttpGet("[action]/{locationIdentifier}/{dataType}")]
        //[Produces("application/json", "application/xml")]
        //[ProducesResponseType(typeof(IEnumerable<CompressedAggregationBase>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        //public async Task<ActionResult<IEnumerable<CompressedAggregationBase>>> GetData(
        //    [FromRoute] string locationIdentifier,
        //    [FromRoute] string dataType,
        //    [FromQuery] DateTime start,
        //    [FromQuery] DateTime end,
        //    CancellationToken cancellationToken)
        //{
        //    var error = await ValidateInputs(locationIdentifier, start, end);
        //    if (error != null) return error;

        //    if (!typeof(AggregationModelBase).ListDerivedTypes().Contains(dataType))
        //    {
        //        return BadRequest(new ProblemDetails { Title = "Invalid data type", Detail = $"The specified data type '{dataType}' is not recognized." });
        //    }

        //    if (!typeof(AggregationModelBase).ToDictionary().TryGetValue(dataType, out var type))
        //    {
        //        return BadRequest(new ProblemDetails
        //        {
        //            Title = "Invalid data type",
        //            Detail = $"The specified data type '{dataType}' is not recognized.",
        //            Status = StatusCodes.Status400BadRequest
        //        });
        //    }

        //    var list = await _repository.GetData(locationIdentifier, start, end, type)
        //                                .ToListAsync(cancellationToken);
        //    return Ok(list);
        //}
    }
}