#region license
// Copyright 2026 Utah Departement of Transportation
// for DataApi - Utah.Udot.ATSPM.DataApi.Services/AggregationImporterService.cs
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

using CsvHelper;
using System.Globalization;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;

namespace Utah.Udot.ATSPM.DataApi.Services
{
    /// <summary>
    /// Service for importing aggregation data from CSV files.
    /// </summary>
    public class AggregationImporterService
    {
        private readonly IPhasePedAggregationRepository _pedRepository;
        private readonly IPhaseCycleAggregationRepository _cycleRepository;
        private readonly ILocationRepository _locationRepository;

        public AggregationImporterService(
            IPhasePedAggregationRepository pedRepository,
            IPhaseCycleAggregationRepository cycleRepository,
            ILocationRepository locationRepository)
        {
            _pedRepository = pedRepository;
            _cycleRepository = cycleRepository;
            _locationRepository = locationRepository;
        }

        /// <summary>
        /// Imports pedestrian aggregation data from a CSV stream.
        /// </summary>
        /// <param name="locationIdentifier">The location identifier for the data</param>
        /// <param name="csvStream">Stream containing CSV data</param>
        /// <returns>Result of the import operation</returns>
        public async Task<AggregationImportResult> ImportPedestrianAggregationFromCsvAsync(string locationIdentifier, Stream csvStream)
        {
            var result = new AggregationImportResult();

            try
            {
                // Validate location exists
                var location = _locationRepository.GetLatestVersionOfLocation(locationIdentifier);
                if (location == null)
                {
                    result.Success = false;
                    result.Message = $"Location '{locationIdentifier}' not found";
                    return result;
                }

                // Parse CSV
                using var reader = new StreamReader(csvStream);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                var records = new List<PhasePedAggregationCsvRecord>();
                await csv.ReadAsync();
                csv.ReadHeader();

                while (await csv.ReadAsync())
                {
                    try
                    {
                        var record = csv.GetRecord<PhasePedAggregationCsvRecord>();
                        if (record != null)
                        {
                            records.Add(record);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Row {csv.Parser.Row}: {ex.Message}");
                    }
                }

                if (records.Count == 0)
                {
                    result.Success = false;
                    result.Message = "No valid records found in CSV";
                    return result;
                }

                // Convert to aggregation models
                var aggregations = new List<PhasePedAggregation>();
                foreach (var record in records)
                {
                    var agg = new PhasePedAggregation
                    {
                        LocationIdentifier = locationIdentifier,
                        PhaseNumber = record.phaseNumber,
                        Start = record.start,
                        End = record.end,
                        PedBeginWalkCount = record.pedBeginWalkCount,
                        PedCallsRegisteredCount = record.pedCallsRegisteredCount,
                        PedCycles = record.pedCycles,
                        PedDelay = record.pedDelay,
                        PedRequests = record.pedRequests,
                        ImputedPedCallsRegistered = record.imputedPedCallsRegistered,
                        MaxPedDelay = record.maxPedDelay,
                        MinPedDelay = record.minPedDelay,
                        UniquePedDetections = record.uniquePedDetections
                    };
                    aggregations.Add(agg);
                }

                // Insert into database using compressed aggregations
                var compressedAgg = new CompressedAggregations<PhasePedAggregation>
                {
                    LocationIdentifier = locationIdentifier,
                    Start = aggregations.Min(a => a.Start),
                    End = aggregations.Max(a => a.End),
                    Data = aggregations
                };

                await _pedRepository.AddAsync(compressedAgg);
                result.Success = true;
                result.Message = $"Successfully imported {records.Count} pedestrian aggregation records";
                result.RecordsImported = records.Count;

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors = new List<string> { $"Error importing pedestrian CSV: {ex.Message}" };
                result.Message = $"Error importing pedestrian CSV: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// Imports cycle aggregation data from a CSV stream.
        /// </summary>
        /// <param name="locationIdentifier">The location identifier for the data</param>
        /// <param name="csvStream">Stream containing CSV data</param>
        /// <returns>Result of the import operation</returns>
        public async Task<AggregationImportResult> ImportCycleAggregationFromCsvAsync(string locationIdentifier, Stream csvStream)
        {
            var result = new AggregationImportResult();

            try
            {
                // Validate location exists
                var location = _locationRepository.GetLatestVersionOfLocation(locationIdentifier);
                if (location == null)
                {
                    result.Success = false;
                    result.Message = $"Location '{locationIdentifier}' not found";
                    return result;
                }

                // Parse CSV
                using var reader = new StreamReader(csvStream);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                var records = new List<PhaseCycleAggregationCsvRecord>();
                await csv.ReadAsync();
                csv.ReadHeader();

                while (await csv.ReadAsync())
                {
                    try
                    {
                        var record = csv.GetRecord<PhaseCycleAggregationCsvRecord>();
                        if (record != null)
                        {
                            records.Add(record);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Row {csv.Parser.Row}: {ex.Message}");
                    }
                }

                if (records.Count == 0)
                {
                    result.Success = false;
                    result.Message = "No valid records found in CSV";
                    return result;
                }

                // Convert to aggregation models
                var aggregations = new List<PhaseCycleAggregation>();
                foreach (var record in records)
                {
                    var agg = new PhaseCycleAggregation
                    {
                        LocationIdentifier = locationIdentifier,
                        ApproachId = record.approachId,
                        PhaseNumber = record.phaseNumber,
                        Start = record.start,
                        End = record.end,
                        GreenTime = record.greenTime,
                        PhaseBeginCount = record.phaseBeginCount,
                        RedTime = record.redTime,
                        TotalGreenToGreenCycles = record.totalGreenToGreenCycles,
                        TotalRedToRedCycles = record.totalRedToRedCycles,
                        YellowTime = record.yellowTime
                    };
                    aggregations.Add(agg);
                }

                // Insert into database using compressed aggregations
                var compressedAgg = new CompressedAggregations<PhaseCycleAggregation>
                {
                    LocationIdentifier = locationIdentifier,
                    Start = aggregations.Min(a => a.Start),
                    End = aggregations.Max(a => a.End),
                    Data = aggregations
                };

                await _cycleRepository.AddAsync(compressedAgg);
                result.Success = true;
                result.Message = $"Successfully imported {records.Count} cycle aggregation records";
                result.RecordsImported = records.Count;

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors = new List<string> { $"Error importing cycle CSV: {ex.Message}" };
                result.Message = $"Error importing cycle CSV: {ex.Message}";
                return result;
            }
        }
    }

    /// <summary>
    /// Represents a pedestrian aggregation record from CSV.
    /// </summary>
    public class PhasePedAggregationCsvRecord
    {
        public int phaseNumber { get; set; }
        public int pedBeginWalkCount { get; set; }
        public int pedCallsRegisteredCount { get; set; }
        public int pedCycles { get; set; }
        public double pedDelay { get; set; }
        public int pedRequests { get; set; }
        public int imputedPedCallsRegistered { get; set; }
        public double maxPedDelay { get; set; }
        public double minPedDelay { get; set; }
        public int uniquePedDetections { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
    }

    /// <summary>
    /// Represents a cycle aggregation record from CSV.
    /// </summary>
    public class PhaseCycleAggregationCsvRecord
    {
        public int greenTime { get; set; }
        public int phaseBeginCount { get; set; }
        public int phaseNumber { get; set; }
        public int redTime { get; set; }
        public int totalGreenToGreenCycles { get; set; }
        public int totalRedToRedCycles { get; set; }
        public int yellowTime { get; set; }
        public int approachId { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
    }

    /// <summary>
    /// Result of an aggregation import operation.
    /// </summary>
    public class AggregationImportResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int RecordsImported { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}