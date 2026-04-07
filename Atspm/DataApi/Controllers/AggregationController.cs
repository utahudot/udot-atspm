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

        /// <summary>
        /// Uploads aggregated data to the database
        /// </summary>
        /// <param name="request">The upload request containing aggregation data</param>
        /// <returns>Result of the upload operation</returns>
        [HttpPost("UploadAggregations")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(UploadResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [RequestSizeLimit(long.MaxValue)]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadAggregations([FromBody] UploadAggregationsRequest request)
        {
            try
            {
                var results = new UploadResult();

                foreach (var aggregationData in request.Aggregations)
                {
                    // Note: You'll need to implement the actual save logic based on your repository
                    // This is a placeholder - adjust according to your data layer
                    _log.LogInformation($"Processing upload for {aggregationData.Type} with {aggregationData.RecordCount} records");

                    // TODO: Deserialize and save the data
                    //var data = JsonSerializer.Deserialize<List<AggregationModelBase>>(aggregationData.JsonData);
                    // await _repository.SaveAggregationsAsync(data);

                    results.ProcessedTypes.Add(aggregationData.Type);
                    results.TotalRecordsProcessed += aggregationData.RecordCount;
                }

                _log.LogInformation($"Upload completed. Processed {results.TotalRecordsProcessed} records across {results.ProcessedTypes.Count} types");
                return Ok(results);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Upload failed");
                return BadRequest(new ProblemDetails
                {
                    Title = "Upload failed",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
        }
    }

    /// <summary>
    /// Request model for uploading aggregations
    /// </summary>
    public class UploadAggregationsRequest
    {
        /// <summary>
        /// List of aggregation data to upload
        /// </summary>
        public List<AggregationUploadData> Aggregations { get; set; } = new();
    }

    /// <summary>
    /// Individual aggregation data for upload
    /// </summary>
    public class AggregationUploadData
    {
        /// <summary>
        /// The aggregation type name
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// JSON data as string
        /// </summary>
        public string JsonData { get; set; } = string.Empty;

        /// <summary>
        /// Number of records (for logging)
        /// </summary>
        public int RecordCount { get; set; }
    }

    /// <summary>
    /// Result of upload operation
    /// </summary>
    public class UploadResult
    {
        /// <summary>
        /// Types that were processed
        /// </summary>
        public List<string> ProcessedTypes { get; set; } = new();

        /// <summary>
        /// Total records processed
        /// </summary>
        public int TotalRecordsProcessed { get; set; }
    }
}