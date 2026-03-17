#region license
// Copyright 2026 Utah Departement of Transportation
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
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.ATSPM.DataApi.Controllers;
using Utah.Udot.ATSPM.DataApi.Services;

namespace Utah.Udot.Atspm.DataApi.Controllers
{
    /// <summary>
    /// Aggregation controller
    /// for querying raw aggregation data
    /// </summary>
    /// <inheritdoc/>
    [ApiVersion("1.0")]
    [Authorize(Policy = "CanViewData")]
    public class AggregationController(
        IAggregationRepository repository, 
        ILocationRepository locations, 
        ILogger<AggregationController> log,
        AggregationImporterService aggregationImporterService)
        : DataControllerBase<CompressedAggregationBase, AggregationModelBase>(repository, locations, log)
    {
        private readonly IAggregationRepository _repository = repository;
        private readonly AggregationImporterService _aggregationImporterService = aggregationImporterService;

        /// <summary>
        /// Upload pedestrian aggregation data from a CSV file.
        /// Expected CSV columns: phaseNumber, pedBeginWalkCount, pedCallsRegisteredCount, 
        /// pedCycles, pedDelay, pedRequests, imputedPedCallsRegistered, maxPedDelay, minPedDelay, 
        /// uniquePedDetections, start, end
        /// </summary>
        /// <param name="locationIdentifier">The location identifier for the aggregation data</param>
        /// <param name="file">CSV file containing pedestrian aggregation data</param>
        /// <returns>Import result with status and record count</returns>
        /// <response code="200">Successfully imported pedestrian aggregation data</response>
        /// <response code="400">Invalid request or CSV format error</response>
        /// <response code="404">Location not found</response>
        /// <response code="500">Server error during import</response>
        [HttpPost("[action]/{locationIdentifier}")]
        [Authorize(Policy = "CanEditData")]
        [Consumes("text/csv", "multipart/form-data")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadPedestrianAggregationFromCsvAsync(string locationIdentifier, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "No file uploaded" });

                if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { message = "File must be a CSV file" });

                using var stream = file.OpenReadStream();
                var result = await _aggregationImporterService.ImportPedestrianAggregationFromCsvAsync(locationIdentifier, stream);

                if (!result.Success)
                {
                    return result.Message.Contains("not found") 
                        ? NotFound(new { message = result.Message })
                        : BadRequest(new { message = result.Message, errors = result.Errors });
                }

                return Ok(new 
                { 
                    message = result.Message,
                    recordsImported = result.RecordsImported,
                    errors = result.Errors.Count > 0 ? result.Errors : null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error processing pedestrian CSV upload: {ex.Message}" });
            }
        }

        /// <summary>
        /// Upload cycle aggregation data from a CSV file.
        /// Expected CSV columns: greenTime, phaseBeginCount, phaseNumber, redTime, 
        /// totalGreenToGreenCycles, totalRedToRedCycles, yellowTime, approachId, start, end
        /// </summary>
        /// <param name="locationIdentifier">The location identifier for the aggregation data</param>
        /// <param name="file">CSV file containing cycle aggregation data</param>
        /// <returns>Import result with status and record count</returns>
        /// <response code="200">Successfully imported cycle aggregation data</response>
        /// <response code="400">Invalid request or CSV format error</response>
        /// <response code="404">Location not found</response>
        /// <response code="500">Server error during import</response>
        [HttpPost("[action]/{locationIdentifier}")]
        [Authorize(Policy = "CanEditData")]
        [Consumes("text/csv", "multipart/form-data")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadCycleAggregationFromCsvAsync(string locationIdentifier, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "No file uploaded" });

                if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { message = "File must be a CSV file" });

                using var stream = file.OpenReadStream();
                var result = await _aggregationImporterService.ImportCycleAggregationFromCsvAsync(locationIdentifier, stream);

                if (!result.Success)
                {
                    return result.Message.Contains("not found") 
                        ? NotFound(new { message = result.Message })
                        : BadRequest(new { message = result.Message, errors = result.Errors });
                }

                return Ok(new 
                { 
                    message = result.Message,
                    recordsImported = result.RecordsImported,
                    errors = result.Errors.Count > 0 ? result.Errors : null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error processing cycle CSV upload: {ex.Message}" });
            }
        }
    }
}
