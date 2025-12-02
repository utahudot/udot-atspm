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
    public class AggregationController(IAggregationRepository repository, ILogger<AggregationController> log) : DataControllerBase
    {
        private readonly IAggregationRepository _repository = repository;
        private readonly ILogger _log = log;

        /// <summary>
        /// Retrieves the available aggregated data types defined in the system.
        /// </summary>
        /// <returns>
        /// A list of aggregation type names.
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
                var result = typeof(AggregationModelBase)
                    .ListDerivedTypes()
                    .ToList()
                    .AsReadOnly();

                return Ok(result);
            }
            catch (Exception ex)
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
        /// Retrieves archived aggregation records for a specific location within a given date range.
        /// </summary>
        /// <param name="locationIdentifier">
        /// Unique identifier of the location whose aggregations are requested.
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
        /// <returns>
        /// A stream of <see cref="CompressedDataBase"/> objects representing archived aggregations.
        /// </returns>
        /// <remarks>
        /// ### Examples:
        /// - GET /api/aggregations/location123?start=2024-01-01&amp;end=2024-12-31
        /// ### Streaming and Cancellation
        /// - Results are streamed using <c>IAsyncEnumerable</c>.
        /// - Clients can cancel by aborting the HTTP request (e.g., disposing <c>HttpClient</c> in .NET or calling <c>AbortController.abort()</c> in JavaScript).
        /// - Cancellation immediately stops enumeration and closes the response.
        /// </remarks>
        /// <response code="200">Aggregations successfully retrieved (may be empty).</response>
        /// <response code="400">Invalid date range or malformed request.</response>
        /// <response code="404">Location not found (if repository distinguishes this).</response>
        [HttpGet("[action]/{locationIdentifier}")]
        [Produces("application/json", "application/xml")]
        [ProducesResponseType(typeof(IEnumerable<CompressedDataBase>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public ActionResult<IAsyncEnumerable<CompressedDataBase>> GetArchivedAggregations(
            [FromRoute] string locationIdentifier,
            [FromQuery] DateTime start,
            [FromQuery] DateTime end,
            CancellationToken cancellationToken)
        {
            if (end < start)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid date range",
                    Detail = "End must be greater than or equal to start",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var results = _repository.GetArchivedAggregations(locationIdentifier, start, end).WithCancellation(cancellationToken);

            return Ok(results);
        }

        /// <summary>
        /// Retrieves archived aggregation records for a specific location and aggregation data type within a given date range.
        /// </summary>
        /// <param name="locationIdentifier">
        /// Unique identifier of the location whose aggregations are requested.
        /// </param>
        /// <param name="dataType">
        /// The name of the aggregation data type to retrieve (e.g., DailyAggregation, MonthlyAggregation).
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
        /// <returns>
        /// A stream of <see cref="CompressedDataBase"/> objects representing archived aggregations of the specified type.
        /// </returns>
        /// <remarks>
        /// ### Examples
        /// - GET /api/aggregations/location123/DailyAggregation?start=2024-01-01&amp;end=2024-12-31
        ///
        /// ### Streaming and Cancellation
        /// - Results are streamed using <c>IAsyncEnumerable</c>.
        /// - Clients can cancel by aborting the HTTP request (e.g., disposing <c>HttpClient</c> in .NET or calling <c>AbortController.abort()</c> in JavaScript).
        /// - Cancellation immediately stops enumeration and closes the response.
        /// </remarks>
        /// <response code="200">Aggregations successfully retrieved (may be empty).</response>
        /// <response code="400">Invalid date range or data type.</response>
        /// <response code="404">Location not found (if repository distinguishes this).</response>
        [HttpGet("[action]/{locationIdentifier}/{dataType}")]
        [Produces("application/json", "application/xml")]
        [ProducesResponseType(typeof(IEnumerable<CompressedDataBase>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public ActionResult<IAsyncEnumerable<CompressedDataBase>> GetArchivedAggregations(
            [FromRoute] string locationIdentifier,
            [FromRoute] string dataType,
            [FromQuery] DateTime start,
            [FromQuery] DateTime end,
            CancellationToken cancellationToken)
        {
            if (end < start)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid date range",
                    Detail = "End must be greater than or equal to start",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            Type type;
            try
            {
                type = Type.GetType(
                    $"{typeof(AggregationModelBase).Namespace}.{dataType}, {typeof(AggregationModelBase).Assembly}",
                    throwOnError: true);
            }
            catch (Exception)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid data type",
                    Detail = $"The specified data type '{dataType}' is not recognized.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var results = _repository.GetArchivedAggregations(locationIdentifier, start, end, type).WithCancellation(cancellationToken);

            return Ok(results);
        }
    }
}