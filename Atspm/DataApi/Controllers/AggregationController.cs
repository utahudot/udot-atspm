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
using System.Text.Json;
using System.Text.Json.Nodes;
using Utah.Udot.Atspm.Extensions;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.ATSPM.DataApi.Controllers;
using Utah.Udot.NetStandardToolkit.Common;

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
                    string flattenedJson = FlattenDataFields(aggregationData.JsonData);

                    // 1. Get the concrete C# type
                    Type concreteType = GetTypeFromDataType(aggregationData.Type);

                    // 🚨 Safety check
                    if (concreteType == null || concreteType == typeof(AggregationModelBase))
                        throw new Exception($"Invalid concrete type for {aggregationData.Type}");

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    // 2. Deserialize into List<ConcreteType>
                    var listType = typeof(List<>).MakeGenericType(concreteType);
                    var obj = JsonSerializer.Deserialize(flattenedJson, listType, options);

                    // 3. Cast to base type
                    var typedList = ((System.Collections.IEnumerable)obj)
                        .Cast<AggregationModelBase>()
                        .ToList();

                    // 🚨 Extra safety: ensure all items are actually the expected type
                    if (typedList.Any(x => x.GetType() != concreteType))
                        throw new Exception("Type mismatch detected in deserialized data.");

                    // 4. Compress (this assumes you fixed Compress to use group.First().GetType())
                    var compressedAggregations = await Compress(typedList).ToListAsync();

                    // 5. Upsert each one using correct generic type
                    foreach (var compressed in compressedAggregations)
                    {
                        var actualType = compressed.GetType();

                        var method = typeof(IAggregationRepositoryExtensions)
                            .GetMethod(nameof(IAggregationRepositoryExtensions.Upsert))
                            .MakeGenericMethod(actualType);

                        await (Task)method.Invoke(null, new object[] { _repository, compressed });
                    }

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

        private static string FlattenDataFields(string jsonArray)
        {
            var array = JsonNode.Parse(jsonArray).AsArray();

            foreach (var itemNode in array)
            {
                if (itemNode is JsonObject obj)
                {
                    // Remove top-level start/end if not needed
                    obj.Remove("start");
                    obj.Remove("end");

                    // First, flatten "data.*" fields
                    var keysToMove = new List<string>();
                    foreach (var kv in obj)
                        if (kv.Key.StartsWith("data."))
                            keysToMove.Add(kv.Key);

                    foreach (var key in keysToMove)
                    {
                        var value = obj[key]?.DeepClone();
                        string newKey = key.Substring(5); // remove "data."
                        obj.Add(newKey, value);
                        obj.Remove(key);
                    }

                    // Now capitalize **all keys** in the object
                    var keys = new List<string>(obj.Select(i => i.Key));
                    foreach (var key in keys)
                    {
                        string capitalizedKey = char.ToUpperInvariant(key[0]) + key.Substring(1);
                        if (capitalizedKey != key)
                        {
                            var value = obj[key]?.DeepClone();
                            if (key.Contains("ocationIdentifier"))
                            {
                                value = value.ToString();
                            }
                            obj.Remove(key);
                            obj.Add(capitalizedKey, value);
                        }
                    }
                }
            }

            return array.ToJsonString();
        }

        private static readonly Dictionary<string, Type> TypeMap = new()
        {
            { "DetectorEventCountAggregation", typeof(Utah.Udot.Atspm.Data.Models.DetectorEventCountAggregation) },
            { "ApproachPcdAggregation", typeof(Utah.Udot.Atspm.Data.Models.ApproachPcdAggregation) },
            { "ApproachSpeedAggregation", typeof(Utah.Udot.Atspm.Data.Models.ApproachSpeedAggregation) },
            { "ApproachSplitFailAggregation", typeof(Utah.Udot.Atspm.Data.Models.ApproachSplitFailAggregation) },
            { "PhaseCycleAggregation", typeof(Utah.Udot.Atspm.Data.Models.PhaseCycleAggregation) },
            { "PhaseLeftTurnGapAggregation", typeof(Utah.Udot.Atspm.Data.Models.PhaseLeftTurnGapAggregation) },
            { "PhasePedAggregation", typeof(Utah.Udot.Atspm.Data.Models.PhasePedAggregation) },
            { "PhaseSplitMonitorAggregation", typeof(Utah.Udot.Atspm.Data.Models.PhaseSplitMonitorAggregation) },
            { "PhaseTerminationAggregation", typeof(Utah.Udot.Atspm.Data.Models.PhaseTerminationAggregation) },
            { "PreemptionAggregation", typeof(Utah.Udot.Atspm.Data.Models.PreemptionAggregation) },
            { "PriorityAggregation", typeof(Utah.Udot.Atspm.Data.Models.PriorityAggregation) },
            { "SignalEventCountAggregation", typeof(Utah.Udot.Atspm.Data.Models.SignalEventCountAggregation) },
            { "SignalPlanAggregation", typeof(Utah.Udot.Atspm.Data.Models.SignalPlanAggregation) },
            { "ApproachYellowRedActivationAggregation", typeof(Utah.Udot.Atspm.Data.Models.ApproachYellowRedActivationAggregation) },
        };

        private static Type GetTypeFromDataType(string dataType)
        {
            if (string.IsNullOrEmpty(dataType)) return null;

            string className;
            TypeMap.TryGetValue(dataType, out var type);
            return type;
        }

        private async IAsyncEnumerable<CompressedAggregationBase> Compress(
       IEnumerable<AggregationModelBase> input)
        {
            var grouped = input.GroupBy(g => new GroupKey(
                g.LocationIdentifier,
                g.Start.Year,
                g.Start.Month,
                g.Start.Day,
                g.GetType()
            ));

            foreach (var group in grouped)
            {
                // Build timeline
                var tl = new Timeline<StartEndRange>(
                    group.Min(m => m.Start),
                    group.Max(m => m.Start),
                    TimeSpan.FromDays(1)
                );

                // Create compressed object
                var compressed = CreateCompressedAggregation(group.Key.DataType);

                compressed.LocationIdentifier = group.Key.LocationIdentifier;
                compressed.Start = tl.Start;
                compressed.End = tl.End;
                compressed.DataType = group.Key.DataType;
                compressed.Data = input;

                yield return compressed;
            }

            await Task.CompletedTask; // keeps async signature happy
        }

        private static CompressedAggregationBase CreateCompressedAggregation(Type type)
        {
            var compType = typeof(CompressedAggregations<>).MakeGenericType(type);
            return (CompressedAggregationBase)Activator.CreateInstance(compType)!;
        }

        private record GroupKey(string LocationIdentifier, int Year, int Month, int Day, Type DataType);

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