#region license
// Copyright 2026 Utah Departement of Transportation
// for DatabaseInstaller - DatabaseInstaller.Services/TransferV4ConfigCommandHostedService.cs
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

using DatabaseInstaller.Commands;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.ConfigurationModels;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;

namespace DatabaseInstaller.Services
{
    /// <summary>
    /// Migrates configuration data from ATSPM v4 database (MOE) to v5.
    /// 
    /// Field Mapping Documentation:
    /// =================================
    /// 
    /// LOCATION MAPPING (v4 Signals -> v5 Locations)
    /// v4 Field                          v5 Field                   Notes
    /// ==============================    ====================       =====================================
    /// SignalID                          (new record, auto-id)      
    /// SignalNumber (string)             LocationIdentifier         Primary identifier for the location
    /// SignalName                        PrimaryName                Primary name of the location
    /// SignalName (append region/area)   SecondaryName              Secondary name for clarity
    /// Latitude                          Latitude                   Coordinate mapping
    /// Longitude                         Longitude                  Coordinate mapping
    /// NULL (default true)               ChartEnabled               Defaults to false initially
    /// NULL (default)                    VersionAction              Set to New
    /// Comments                          Note                       Migration comment field
    /// NULL (default NOW)                Start                      Set to current date/time
    /// NULL (default false)              PedsAre1to1                Defaults to false; check v4 if available
    /// NULL (create/lookup)              LocationTypeId             Lookup or default to standard type (ID 1)
    /// NULL (optional)                   JurisdictionId             Lookup by jurisdiction if available in v4
    /// NULL (optional)                   RegionId                   Lookup by region if available in v4
    /// 
    /// APPROACH MAPPING (v4 SignalApproaches -> v5 Approaches)
    /// v4 Field                          v5 Field                   Notes
    /// ==============================    ====================       =====================================
    /// ApproachID                        (new record, auto-id)      
    /// SignalID (FK)                     LocationId (FK)            Foreign key to migrated Location
    /// ApproachDescription               Description                Approach direction/name
    /// ApproachSpeed/MPH                 Mph                        Speed limit
    /// ProtectedPhase                    ProtectedPhaseNumber       Phase number 1-8
    /// ProtectedPhaseOverlap             IsProtectedPhaseOverlap    Boolean flag (always false for v4)
    /// PermissivePhase                   PermissivePhaseNumber      Phase number 1-8 or null
    /// PermissivePhaseOverlap            IsPermissivePhaseOverlap   Boolean flag (always false for v4)
    /// PedestrianPhase                   PedestrianPhaseNumber      Phase number or null
    /// PedestrianPhaseOverlap            IsPedestrianPhaseOverlap   Boolean flag (always false for v4)
    /// PedestrianDetectors (comma-list)  PedestrianDetectors        Comma-separated detector IDs
    /// TransitSignalPriorityPhase        TransitSignalPriorityNumber Phase number or null
    /// ApproachDirection (string)        DirectionTypeId (enum)     Maps N/S/E/W to DirectionTypes enum (NB/SB/EB/WB)
    /// 
    /// DETECTOR MAPPING (v4 SignalDetectors -> v5 Detectors)
    /// v4 Field                          v5 Field                   Notes
    /// ==============================    ====================       =====================================
    /// DetectorID                        (new record, auto-id)      
    /// ApproachID (FK)                   ApproachId (FK)            Foreign key to migrated Approach
    /// DetectorNumber                    DectectorIdentifier        Detector channel/name identifier
    /// DetectorChannel                   DetectorChannel            Physical channel number
    /// DistanceFromStopBar               DistanceFromStopBar        Distance in feet (nullable)
    /// SpeedFilter                       MinSpeedFilter             Minimum speed filter (nullable)
    /// DateAdded                         DateAdded                  When detector was added
    /// DateDisabled                      DateDisabled               When detector was disabled (nullable)
    /// Lane                              LaneNumber                 Lane number (nullable)
    /// MovementType                      MovementType (enum)        Through/Left/Right/UTurn etc.
    /// LaneType                          LaneType (enum)            General/Shared/Bike/Turn lane etc.
    /// DetectionHardwareType             DetectionHardware (enum)   Inductive Loop/MMR/Radar etc.
    /// DecisionPoint                     DecisionPoint              Decision point distance (nullable)
    /// MovementDelay                     MovementDelay              Movement delay in ms (nullable)
    /// Latency                           LatencyCorrection          Latency correction factor (default 0.0)
    /// 
    /// DETECTOR COMMENT MAPPING (v4 DetectorComments -> v5 DetectorComments)
    /// v4 Field                          v5 Field                   Notes
    /// ==============================    ====================       =====================================
    /// CommentID                         (new record, auto-id)      
    /// DetectorID (FK)                   DetectorId (FK)            Foreign key to migrated Detector
    /// CommentDate                       TimeStamp                  When comment was created
    /// CommentText                       Comment                    The comment text
    /// 
    /// </summary>
    public class TransferV4ConfigCommandHostedService : IHostedService
    {
        private readonly ILogger<TransferV4ConfigCommandHostedService> _logger;
        private readonly ConfigContext _configContext;
        private readonly ILocationRepository _locationRepository;
        private readonly IApproachRepository _approachRepository;
        private readonly IDetectorRepository _detectorRepository;
        private readonly IDetectorCommentRepository _detectorCommentRepository;
        private readonly ILocationTypeRepository _locationTypeRepository;
        private readonly IRegionsRepository _regionsRepository;
        private readonly IJurisdictionRepository _jurisdictionRepository;
        private readonly TransferV4ConfigCommandConfiguration _config;

        // Batch size for processing
        private const int BatchSize = 50;

        // Dictionary to map v4 IDs to v5 IDs during migration
        private readonly Dictionary<string, int> _signalToLocationMap = new();
        private readonly Dictionary<int, int> _approachMap = new();
        private readonly Dictionary<int, int> _detectorMap = new();

        // Cache for jurisdiction and region IDs with their descriptions
        private Dictionary<int, int> _v4ToV5JurisdictionMap;
        private Dictionary<int, int> _v4ToV5RegionMap;

        public TransferV4ConfigCommandHostedService(
            ILogger<TransferV4ConfigCommandHostedService> logger,
            ConfigContext configContext,
            ILocationRepository locationRepository,
            IApproachRepository approachRepository,
            IDetectorRepository detectorRepository,
            IDetectorCommentRepository detectorCommentRepository,
            ILocationTypeRepository locationTypeRepository,
            IRegionsRepository regionsRepository,
            IJurisdictionRepository jurisdictionRepository,
            IOptions<TransferV4ConfigCommandConfiguration> config)
        {
            _logger = logger;
            _configContext = configContext;
            _locationRepository = locationRepository;
            _approachRepository = approachRepository;
            _detectorRepository = detectorRepository;
            _detectorCommentRepository = detectorCommentRepository;
            _locationTypeRepository = locationTypeRepository;
            _regionsRepository = regionsRepository;
            _jurisdictionRepository = jurisdictionRepository;
            _locationTypeRepository = locationTypeRepository;
            _config = config.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_config.Source))
            {
                _logger.LogError("Source connection string is required for v4 migration");
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("Starting v4 to v5 configuration migration...");

                // Step 0: Sync jurisdictions and regions from v4 to v5
                await SyncJurisdictionsAndRegionsAsync(_config.Source, cancellationToken);

                // Parse location filter
                var locationFilter = ParseLocationFilter(_config.Locations);

                // Step 1: Load v4 data
                var v4Signals = await LoadV4SignalsAsync(_config.Source, locationFilter, cancellationToken);
                _logger.LogInformation("Loaded {Count} signals from v4 database", v4Signals.Count);

                if (v4Signals.Count == 0)
                {
                    _logger.LogWarning("No signals found matching the filter criteria");
                    return;
                }

                // Step 2: Load v4 related data
                var signalIds = v4Signals.Select(s => s.SignalID).ToList();
                var v4Approaches = await LoadV4ApproachesAsync(_config.Source, signalIds, cancellationToken);
                _logger.LogInformation("Loaded {Count} approaches from v4 database", v4Approaches.Count);

                var v4Detectors = await LoadV4DetectorsAsync(_config.Source, v4Approaches.Select(a => a.ApproachID).ToList(), cancellationToken);
                _logger.LogInformation("Loaded {Count} detectors from v4 database", v4Detectors.Count);

                var v4DetectorComments = await LoadV4DetectorCommentsAsync(_config.Source, v4Detectors.Select(d => d.ID).ToList(), cancellationToken);
                _logger.LogInformation("Loaded {Count} detector comments from v4 database", v4DetectorComments.Count);

                // Step 3: Migrate data
                await MigrateLocationsAsync(v4Signals, cancellationToken);
                _logger.LogInformation("Migrated {Count} locations", _signalToLocationMap.Count);

                await MigrateApproachesAsync(v4Approaches, cancellationToken);
                _logger.LogInformation("Migrated {Count} approaches", _approachMap.Count);

                await MigrateDetectorsAsync(v4Detectors, cancellationToken);
                _logger.LogInformation("Migrated {Count} detectors", _detectorMap.Count);

                await MigrateDetectorCommentsAsync(v4DetectorComments, cancellationToken);
                _logger.LogInformation("Migrated {Count} detector comments", v4DetectorComments.Count);

                stopwatch.Stop();
                _logger.LogInformation("V4 to V5 migration completed successfully in {Elapsed}", stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error during v4 to v5 migration after {Elapsed}", stopwatch.Elapsed);
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private List<string> ParseLocationFilter(string locationsStr)
        {
            if (string.IsNullOrWhiteSpace(locationsStr))
                return new List<string>();

            return locationsStr.Split(',')
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToList();
        }

        private async Task<List<V4Signal>> LoadV4SignalsAsync(string connectionString, List<string> locationFilter, CancellationToken cancellationToken)
        {
            var signals = new List<V4Signal>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    var query = @"
                        SELECT 
                            SignalID, 
                            PrimaryName, 
                            SecondaryName, 
                            Latitude, 
                            Longitude, 
                            Note,
                            RegionID,
                            JurisdictionId,
                            Pedsare1to1
                        FROM Signals
                        WHERE 1=1";

                    // Add location filter if provided
                    if (locationFilter.Count > 0)
                    {
                        var placeholders = string.Join(",", Enumerable.Range(0, locationFilter.Count).Select(i => $"@loc{i}"));
                        query += $" AND SignalID IN ({placeholders})";
                    }

                    using (var command = new SqlCommand(query, connection))
                    {
                        // Bind location parameters
                        for (int i = 0; i < locationFilter.Count; i++)
                        {
                            command.Parameters.AddWithValue($"@loc{i}", locationFilter[i]);
                        }

                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                // Parse coordinates from varchar(30) strings
                                var latStr = reader.IsDBNull(3) ? "0" : reader.GetString(3);
                                var lonStr = reader.IsDBNull(4) ? "0" : reader.GetString(4);
                                double.TryParse(latStr, out var latitude);
                                double.TryParse(lonStr, out var longitude);

                                signals.Add(new V4Signal
                                {
                                    SignalID = reader.GetString(0),
                                    PrimaryName = reader.IsDBNull(1) ? null : reader.GetString(1),
                                    SecondaryName = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    Latitude = latitude,
                                    Longitude = longitude,
                                    Note = reader.IsDBNull(5) ? null : reader.GetString(5),
                                    RegionID = reader.IsDBNull(6) ? null : (int?)reader.GetInt32(6),
                                    JurisdictionId = reader.IsDBNull(7) ? null : (int?)reader.GetInt32(7),
                                    Pedsare1to1 = reader.IsDBNull(8) ? false : reader.GetBoolean(8)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading v4 signals from database");
                throw;
            }

            return signals;
        }

        private async Task<List<V4Approach>> LoadV4ApproachesAsync(string connectionString, List<string> signalIds, CancellationToken cancellationToken)
        {
            var approaches = new List<V4Approach>();

            if (signalIds.Count == 0)
                return approaches;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    var query = @"
                        SELECT 
                            ApproachID, 
                            SignalID, 
                            DirectionTypeID, 
                            Description, 
                            ProtectedPhaseNumber, 
                            PermissivePhaseNumber, 
                            PedestrianPhaseNumber,
                            MPH,
                            IsProtectedPhaseOverlap,
                            IsPermissivePhaseOverlap,
                            IsPedestrianPhaseOverlap,
                            PedestrianDetectors
                        FROM Approaches
                        WHERE SignalID IN (";

                    // Add placeholders for each signal ID
                    var placeholders = string.Join(",", Enumerable.Range(0, signalIds.Count).Select(i => $"@signal{i}"));
                    query += placeholders + ")";

                    using (var command = new SqlCommand(query, connection))
                    {
                        // Bind signal ID parameters
                        for (int i = 0; i < signalIds.Count; i++)
                        {
                            command.Parameters.AddWithValue($"@signal{i}", signalIds[i]);
                        }

                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                var signalId = reader.IsDBNull(1) ? null : reader.GetString(1);

                                approaches.Add(new V4Approach
                                {
                                    ApproachID = reader.GetInt32(0),
                                    SignalID = signalId,
                                    DirectionTypeID = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                                    Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                                    ProtectedPhaseNumber = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                                    PermissivePhaseNumber = reader.IsDBNull(5) ? null : (int?)reader.GetInt32(5),
                                    PedestrianPhaseNumber = reader.IsDBNull(6) ? null : (int?)reader.GetInt32(6),
                                    Mph = reader.IsDBNull(7) ? null : (int?)reader.GetInt32(7),
                                    IsProtectedPhaseOverlap = reader.IsDBNull(8) ? false : reader.GetBoolean(8),
                                    IsPermissivePhaseOverlap = reader.IsDBNull(9) ? false : reader.GetBoolean(9),
                                    IsPedestrianPhaseOverlap = reader.IsDBNull(10) ? false : reader.GetBoolean(10),
                                    PedestrianDetectors = reader.IsDBNull(11) ? null : reader.GetString(11)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading v4 approaches from database");
                throw;
            }

            return approaches;
        }

        private async Task<List<V4Detector>> LoadV4DetectorsAsync(string connectionString, List<int> approachIds, CancellationToken cancellationToken)
        {
            var detectors = new List<V4Detector>();

            if (approachIds.Count == 0)
                return detectors;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    var query = @"
                        SELECT 
                            ID, 
                            ApproachID, 
                            DetectorID, 
                            DetChannel, 
                            DistanceFromStopBar, 
                            MinSpeedFilter, 
                            DateAdded, 
                            DateDisabled,
                            LaneNumber,
                            MovementTypeID,
                            LaneTypeID,
                            DetectionHardwareID,
                            DecisionPoint,
                            MovementDelay,
                            LatencyCorrection
                        FROM Detectors
                        WHERE ApproachID IN (" + string.Join(",", Enumerable.Range(0, approachIds.Count).Select(i => $"@aid{i}")) + ")";

                    using (var command = new SqlCommand(query, connection))
                    {
                        for (int i = 0; i < approachIds.Count; i++)
                        {
                            command.Parameters.AddWithValue($"@aid{i}", approachIds[i]);
                        }

                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                detectors.Add(new V4Detector
                                {
                                    ID = reader.GetInt32(0),
                                    ApproachID = reader.GetInt32(1),
                                    DetectorID = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    DetChannel = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                                    DistanceFromStopBar = reader.IsDBNull(4) ? null : (int?)reader.GetInt32(4),
                                    MinSpeedFilter = reader.IsDBNull(5) ? null : (int?)reader.GetInt32(5),
                                    DateAdded = reader.IsDBNull(6) ? DateTime.Now : reader.GetDateTime(6),
                                    DateDisabled = reader.IsDBNull(7) ? null : (DateTime?)reader.GetDateTime(7),
                                    LaneNumber = reader.IsDBNull(8) ? null : (int?)reader.GetInt32(8),
                                    MovementTypeID = reader.IsDBNull(9) ? null : (int?)reader.GetInt32(9),
                                    LaneTypeID = reader.IsDBNull(10) ? null : (int?)reader.GetInt32(10),
                                    DetectionHardwareID = reader.IsDBNull(11) ? 0 : reader.GetInt32(11),
                                    DecisionPoint = reader.IsDBNull(12) ? null : (int?)reader.GetInt32(12),
                                    MovementDelay = reader.IsDBNull(13) ? null : (int?)reader.GetInt32(13),
                                    LatencyCorrection = reader.IsDBNull(14) ? 0.0 : reader.GetDouble(14)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading v4 detectors from database");
                throw;
            }

            return detectors;
        }

        private async Task<List<V4DetectorComment>> LoadV4DetectorCommentsAsync(string connectionString, List<int> detectorIds, CancellationToken cancellationToken)
        {
            var comments = new List<V4DetectorComment>();

            if (detectorIds.Count == 0)
                return comments;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    var query = @"
                        SELECT 
                            CommentID, 
                            ID, 
                            TimeStamp, 
                            CommentText
                        FROM DetectorComments
                        WHERE ID IN (" + string.Join(",", Enumerable.Range(0, detectorIds.Count).Select(i => $"@did{i}")) + ")";

                    using (var command = new SqlCommand(query, connection))
                    {
                        for (int i = 0; i < detectorIds.Count; i++)
                        {
                            command.Parameters.AddWithValue($"@did{i}", detectorIds[i]);
                        }

                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                comments.Add(new V4DetectorComment
                                {
                                    CommentID = reader.GetInt32(0),
                                    ID = reader.GetInt32(1),
                                    TimeStamp = reader.GetDateTime(2),
                                    CommentText = reader.IsDBNull(3) ? null : reader.GetString(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading v4 detector comments from database");
                throw;
            }

            return comments;
        }

        private async Task SyncJurisdictionsAndRegionsAsync(string connectionString, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Syncing jurisdictions and regions from v4 to v5...");

                // Load v4 jurisdictions and regions
                var v4Jurisdictions = await LoadV4JurisdictionsAsync(connectionString, cancellationToken);
                var v4Regions = await LoadV4RegionsAsync(connectionString, cancellationToken);

                // Initialize maps
                _v4ToV5JurisdictionMap = new Dictionary<int, int>();
                _v4ToV5RegionMap = new Dictionary<int, int>();

                // Sync jurisdictions
                await SyncJurisdictionsToV5Async(v4Jurisdictions, cancellationToken);

                // Sync regions
                await SyncRegionsToV5Async(v4Regions, cancellationToken);

                _logger.LogInformation("Jurisdiction and region sync completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing jurisdictions and regions");
                _v4ToV5JurisdictionMap = new Dictionary<int, int>();
                _v4ToV5RegionMap = new Dictionary<int, int>();
            }
        }

        private async Task<List<(int Id, string Name, string Mpo, string CountyParish, string OtherPartners)>> LoadV4JurisdictionsAsync(string connectionString, CancellationToken cancellationToken)
        {
            var jurisdictions = new List<(int, string, string, string, string)>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    var query = @"
                        SELECT 
                            Id, 
                            JurisdictionName, 
                            MPO, 
                            CountyParish, 
                            OtherPartners
                        FROM Jurisdictions
                        WHERE 1=1";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                jurisdictions.Add((
                                    reader.GetInt32(0),
                                    reader.IsDBNull(1) ? null : reader.GetString(1),
                                    reader.IsDBNull(2) ? null : reader.GetString(2),
                                    reader.IsDBNull(3) ? null : reader.GetString(3),
                                    reader.IsDBNull(4) ? null : reader.GetString(4)
                                ));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading v4 jurisdictions");
            }

            return jurisdictions;
        }

        private async Task<List<(int Id, string Description)>> LoadV4RegionsAsync(string connectionString, CancellationToken cancellationToken)
        {
            var regions = new List<(int, string)>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    var query = @"
                        SELECT 
                            ID, 
                            Description
                        FROM Region
                        WHERE 1=1";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                regions.Add((
                                    reader.GetInt32(0),
                                    reader.IsDBNull(1) ? null : reader.GetString(1)
                                ));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading v4 regions");
            }

            return regions;
        }

        private async Task SyncJurisdictionsToV5Async(List<(int Id, string Name, string Mpo, string CountyParish, string OtherPartners)> v4Jurisdictions, CancellationToken cancellationToken)
        {
            foreach (var v4Jurisdiction in v4Jurisdictions)
            {
                try
                {
                    var existingJurisdictionId = _configContext.Jurisdictions
                        .AsNoTracking()
                        .Where(j => j.Name == v4Jurisdiction.Name)
                        .Select(j => (int?)j.Id)
                        .FirstOrDefault();

                    if (existingJurisdictionId.HasValue)
                    {
                        _logger.LogDebug("Jurisdiction {Name} already exists (ID: {V5Id})", v4Jurisdiction.Name, existingJurisdictionId.Value);
                        _v4ToV5JurisdictionMap[v4Jurisdiction.Id] = existingJurisdictionId.Value;
                    }
                    else
                    {
                        var newJurisdiction = new Jurisdiction
                        {
                            Name = v4Jurisdiction.Name ?? $"Jurisdiction_{v4Jurisdiction.Id}",
                            Mpo = v4Jurisdiction.Mpo,
                            CountyParish = v4Jurisdiction.CountyParish,
                            OtherPartners = v4Jurisdiction.OtherPartners
                        };

                        await _jurisdictionRepository.AddAsync(newJurisdiction);

                        _logger.LogInformation("Created new jurisdiction: {Name} (V5 ID: {V5Id})", newJurisdiction.Name, newJurisdiction.Id);
                        _v4ToV5JurisdictionMap[v4Jurisdiction.Id] = newJurisdiction.Id;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error syncing jurisdiction {Id}", v4Jurisdiction.Id);
                }
            }

            _logger.LogInformation("Synced {Count} jurisdictions", _v4ToV5JurisdictionMap.Count);
        }

        private async Task SyncRegionsToV5Async(List<(int Id, string Description)> v4Regions, CancellationToken cancellationToken)
        {
            foreach (var v4Region in v4Regions)
            {
                try
                {
                    var existingRegionId = _configContext.Regions
                        .AsNoTracking()
                        .Where(r => r.Description == v4Region.Description)
                        .Select(r => (int?)r.Id)
                        .FirstOrDefault();

                    if (existingRegionId.HasValue)
                    {
                        _logger.LogDebug("Region {Description} already exists (ID: {V5Id})", v4Region.Description, existingRegionId.Value);
                        _v4ToV5RegionMap[v4Region.Id] = existingRegionId.Value;
                    }
                    else
                    {
                        var newRegion = new Region
                        {
                            Description = v4Region.Description ?? $"Region_{v4Region.Id}"
                        };

                        await _regionsRepository.AddAsync(newRegion);

                        _logger.LogInformation("Created new region: {Description} (V5 ID: {V5Id})", newRegion.Description, newRegion.Id);
                        _v4ToV5RegionMap[v4Region.Id] = newRegion.Id;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error syncing region {Id}", v4Region.Id);
                }
            }

            _logger.LogInformation("Synced {Count} regions", _v4ToV5RegionMap.Count);
        }

        private async Task MigrateLocationsAsync(List<V4Signal> v4Signals, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting location migration...");
            var locationsToAdd = new List<Location>();

            var existingLocationMap = _configContext.Locations
                .AsNoTracking()
                .Where(l => l.LocationIdentifier != null)
                .GroupBy(l => l.LocationIdentifier)
                .Select(g => new
                {
                    LocationIdentifier = g.Key,
                    Id = g.OrderByDescending(l => l.Start).Select(l => l.Id).FirstOrDefault()
                })
                .ToDictionary(l => l.LocationIdentifier!, l => l.Id);

            for (int i = 0; i < v4Signals.Count; i += BatchSize)
            {
                var batch = v4Signals.Skip(i).Take(BatchSize).ToList();

                foreach (var v4Signal in batch)
                {
                    try
                    {
                        // Check if location already exists by LocationIdentifier (using SignalID as the unique key)
                        if (!string.IsNullOrEmpty(v4Signal.SignalID) &&
                            existingLocationMap.TryGetValue(v4Signal.SignalID, out var existingLocationId))
                        {
                            _logger.LogInformation("Skipping duplicate location: {LocationIdentifier}", v4Signal.SignalID);
                            // Still map for reference in case it's needed
                            _signalToLocationMap[v4Signal.SignalID] = existingLocationId;
                            continue;
                        }

                        // Create new location with mapped foreign keys
                        var location = new Location
                        {
                            LocationIdentifier = v4Signal.SignalID,
                            PrimaryName = v4Signal.PrimaryName ?? $"Signal {v4Signal.SignalID}",
                            SecondaryName = v4Signal.SecondaryName ?? string.Empty,
                            Latitude = v4Signal.Latitude,
                            Longitude = v4Signal.Longitude,
                            ChartEnabled = false,
                            VersionAction = LocationVersionActions.New,
                            Note = v4Signal.Note ?? "Migrated from v4",
                            Start = DateTime.Now,
                            PedsAre1to1 = v4Signal.Pedsare1to1,
                            LocationTypeId = 1,
                            // Map jurisdiction from v4 to v5 ID
                            JurisdictionId = (v4Signal.JurisdictionId.HasValue && _v4ToV5JurisdictionMap.ContainsKey(v4Signal.JurisdictionId.Value))
                                ? _v4ToV5JurisdictionMap[v4Signal.JurisdictionId.Value]
                                : null,
                            // Map region from v4 to v5 ID
                            RegionId = (v4Signal.RegionID.HasValue && _v4ToV5RegionMap.ContainsKey(v4Signal.RegionID.Value))
                                ? _v4ToV5RegionMap[v4Signal.RegionID.Value]
                                : null
                        };

                        locationsToAdd.Add(location);
                        _logger.LogDebug("Prepared location: {LocationIdentifier}", location.LocationIdentifier);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error preparing signal {SignalID}", v4Signal.SignalID);
                        throw;
                    }
                }

                // Batch add
                if (locationsToAdd.Count > 0)
                {
                    _locationRepository.AddRange(locationsToAdd);
                    _logger.LogInformation("Added {Count} locations to repository", locationsToAdd.Count);

                    await _configContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Persisted {Count} locations to database", locationsToAdd.Count);

                    // Map the newly added locations using tracked entities (IDs assigned after AddRange save)
                    foreach (var added in locationsToAdd)
                    {
                        if (string.IsNullOrEmpty(added.LocationIdentifier))
                        {
                            continue;
                        }

                        _signalToLocationMap[added.LocationIdentifier] = added.Id;
                        existingLocationMap[added.LocationIdentifier] = added.Id;
                        _logger.LogDebug("Mapped v4 Signal {SignalID} to v5 Location {LocationId}", added.LocationIdentifier, added.Id);
                    }

                    locationsToAdd.Clear();
                }
            }

            _logger.LogInformation("Location migration completed. Migrated {Count} locations", _signalToLocationMap.Count);
        }

        private async Task MigrateApproachesAsync(List<V4Approach> v4Approaches, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting approach migration...");
            var approachesToAdd = new List<Approach>();
            var v4ApproachesToTracking = new List<(V4Approach v4, Approach v5)>();

            var existingApproachKeyToId = _configContext.Approaches
                .AsNoTracking()
                .Where(a => a.Description != null)
                .Select(a => new { a.Id, a.LocationId, a.Description })
                .ToList()
                .ToDictionary(
                    a => BuildApproachDuplicateKey(a.LocationId, a.Description),
                    a => a.Id,
                    StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < v4Approaches.Count; i += BatchSize)
            {
                var batch = v4Approaches.Skip(i).Take(BatchSize).ToList();
                var pendingApproachesByKey = new Dictionary<string, Approach>(StringComparer.OrdinalIgnoreCase);

                foreach (var v4Approach in batch)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(v4Approach.SignalID) || !_signalToLocationMap.ContainsKey(v4Approach.SignalID))
                        {
                            _logger.LogWarning("Skipping approach {ApproachID}: parent signal {SignalID} not found in mapping",
                                v4Approach.ApproachID, v4Approach.SignalID ?? "NULL");
                            continue;
                        }

                        var locationId = _signalToLocationMap[v4Approach.SignalID];
                        var description = v4Approach.Description ?? $"Approach {v4Approach.ApproachID}";
                        var approachDuplicateKey = BuildApproachDuplicateKey(locationId, description);

                        if (existingApproachKeyToId.TryGetValue(approachDuplicateKey, out var existingApproachId))
                        {
                            _approachMap[v4Approach.ApproachID] = existingApproachId;
                            _logger.LogInformation("Skipping duplicate approach for location {LocationId} and description {Description}", locationId, description);
                            continue;
                        }

                        if (pendingApproachesByKey.TryGetValue(approachDuplicateKey, out var pendingApproach))
                        {
                            v4ApproachesToTracking.Add((v4Approach, pendingApproach));
                            _logger.LogInformation("Skipping duplicate approach in current batch for location {LocationId} and description {Description}", locationId, description);
                            continue;
                        }

                        // Map DirectionTypeID to DirectionTypes enum
                        var directionType = MapDirectionTypeFromId(v4Approach.DirectionTypeID);

                        var approach = new Approach
                        {
                            LocationId = locationId,
                            DirectionTypeId = directionType,
                            Description = description,
                            Mph = v4Approach.Mph,
                            ProtectedPhaseNumber = v4Approach.ProtectedPhaseNumber > 0 ? v4Approach.ProtectedPhaseNumber : 1,
                            IsProtectedPhaseOverlap = v4Approach.IsProtectedPhaseOverlap,
                            PermissivePhaseNumber = v4Approach.PermissivePhaseNumber,
                            IsPermissivePhaseOverlap = v4Approach.IsPermissivePhaseOverlap,
                            PedestrianPhaseNumber = v4Approach.PedestrianPhaseNumber,
                            IsPedestrianPhaseOverlap = v4Approach.IsPedestrianPhaseOverlap,
                            PedestrianDetectors = v4Approach.PedestrianDetectors
                        };

                        approachesToAdd.Add(approach);
                        v4ApproachesToTracking.Add((v4Approach, approach));
                        pendingApproachesByKey[approachDuplicateKey] = approach;
                        _logger.LogDebug("Prepared approach: {ApproachID}", v4Approach.ApproachID);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error preparing approach {ApproachID}", v4Approach.ApproachID);
                        throw;
                    }
                }

                // Batch add
                if (approachesToAdd.Count > 0)
                {
                    foreach (var approach in approachesToAdd)
                    {
                        var insertedId = await InsertApproachWithoutTransitSignalPriorityAsync(approach, cancellationToken);
                        if (insertedId > 0)
                        {
                            approach.Id = insertedId;
                            existingApproachKeyToId[BuildApproachDuplicateKey(approach.LocationId, approach.Description)] = insertedId;
                        }
                    }

                    foreach (var (v4, v5) in v4ApproachesToTracking)
                    {
                        if (v5.Id > 0)
                        {
                            _approachMap[v4.ApproachID] = v5.Id;
                            _logger.LogDebug("Mapped v4 Approach {ApproachID} to v5 Approach {Id}", v4.ApproachID, v5.Id);
                        }
                    }

                    _logger.LogInformation("Persisted {Count} approaches to database", approachesToAdd.Count);

                    approachesToAdd.Clear();
                    v4ApproachesToTracking.Clear();
                }
            }

            _logger.LogInformation("Approach migration completed. Migrated {Count} approaches", _approachMap.Count);
        }

        private static string BuildApproachDuplicateKey(int locationId, string? description)
            => $"{locationId}|{description?.Trim()}";

        private async Task<int> InsertApproachWithoutTransitSignalPriorityAsync(Approach approach, CancellationToken cancellationToken)
        {
            if (!_configContext.Database.IsSqlServer())
            {
                await _approachRepository.AddAsync(approach);
                return approach.Id;
            }

            var connection = (SqlConnection)_configContext.Database.GetDbConnection();
            var closeWhenDone = connection.State != System.Data.ConnectionState.Open;

            if (closeWhenDone)
            {
                await connection.OpenAsync(cancellationToken);
            }

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"
INSERT INTO [dbo].[Approaches]
(
    [Description],
    [Mph],
    [ProtectedPhaseNumber],
    [IsProtectedPhaseOverlap],
    [PermissivePhaseNumber],
    [IsPermissivePhaseOverlap],
    [PedestrianPhaseNumber],
    [IsPedestrianPhaseOverlap],
    [PedestrianDetectors],
    [LocationId],
    [DirectionTypeId],
    [Created],
    [Modified],
    [CreatedBy],
    [ModifiedBy]
)
OUTPUT INSERTED.[Id]
VALUES
(
    @Description,
    @Mph,
    @ProtectedPhaseNumber,
    @IsProtectedPhaseOverlap,
    @PermissivePhaseNumber,
    @IsPermissivePhaseOverlap,
    @PedestrianPhaseNumber,
    @IsPedestrianPhaseOverlap,
    @PedestrianDetectors,
    @LocationId,
    @DirectionTypeId,
    @Created,
    @Modified,
    @CreatedBy,
    @ModifiedBy
);";

                command.Parameters.AddWithValue("@Description", (object?)approach.Description ?? DBNull.Value);
                command.Parameters.AddWithValue("@Mph", (object?)approach.Mph ?? DBNull.Value);
                command.Parameters.AddWithValue("@ProtectedPhaseNumber", approach.ProtectedPhaseNumber);
                command.Parameters.AddWithValue("@IsProtectedPhaseOverlap", approach.IsProtectedPhaseOverlap);
                command.Parameters.AddWithValue("@PermissivePhaseNumber", (object?)approach.PermissivePhaseNumber ?? DBNull.Value);
                command.Parameters.AddWithValue("@IsPermissivePhaseOverlap", approach.IsPermissivePhaseOverlap);
                command.Parameters.AddWithValue("@PedestrianPhaseNumber", (object?)approach.PedestrianPhaseNumber ?? DBNull.Value);
                command.Parameters.AddWithValue("@IsPedestrianPhaseOverlap", approach.IsPedestrianPhaseOverlap);
                command.Parameters.AddWithValue("@PedestrianDetectors", (object?)approach.PedestrianDetectors ?? DBNull.Value);
                command.Parameters.AddWithValue("@LocationId", approach.LocationId);
                command.Parameters.AddWithValue("@DirectionTypeId", (int)approach.DirectionTypeId);
                command.Parameters.AddWithValue("@Created", (object?)approach.Created ?? DBNull.Value);
                command.Parameters.AddWithValue("@Modified", (object?)approach.Modified ?? DBNull.Value);
                command.Parameters.AddWithValue("@CreatedBy", (object?)approach.CreatedBy ?? DBNull.Value);
                command.Parameters.AddWithValue("@ModifiedBy", (object?)approach.ModifiedBy ?? DBNull.Value);

                var result = await command.ExecuteScalarAsync(cancellationToken);
                return result == null ? 0 : Convert.ToInt32(result);
            }
            finally
            {
                if (closeWhenDone)
                {
                    await connection.CloseAsync();
                }
            }
        }

        private async Task MigrateDetectorsAsync(List<V4Detector> v4Detectors, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting detector migration...");
            var detectorsToAdd = new List<Detector>();
            var v4DetectorsToTracking = new List<(V4Detector v4, Detector v5)>();

            var existingDetectorIdentifierToId = _configContext.Detectors
                .AsNoTracking()
                .Where(d => d.DectectorIdentifier != null)
                .Select(d => new { d.Id, d.DectectorIdentifier })
                .ToList()
                .ToDictionary(d => d.DectectorIdentifier!, d => d.Id, StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < v4Detectors.Count; i += BatchSize)
            {
                var batch = v4Detectors.Skip(i).Take(BatchSize).ToList();
                var pendingDetectorsByIdentifier = new Dictionary<string, Detector>(StringComparer.OrdinalIgnoreCase);

                foreach (var v4Detector in batch)
                {
                    try
                    {
                        if (!_approachMap.ContainsKey(v4Detector.ApproachID))
                        {
                            _logger.LogWarning("Skipping detector {DetectorID}: parent approach {ApproachID} not found in mapping",
                                v4Detector.DetectorID, v4Detector.ApproachID);
                            continue;
                        }

                        var detectorIdentifier = v4Detector.DetectorID ?? $"DET_{v4Detector.ID}";

                        if (existingDetectorIdentifierToId.TryGetValue(detectorIdentifier, out var existingDetectorId))
                        {
                            _detectorMap[v4Detector.ID] = existingDetectorId;
                            _logger.LogInformation("Skipping duplicate detector: {DetectorIdentifier}", detectorIdentifier);
                            continue;
                        }

                        if (pendingDetectorsByIdentifier.TryGetValue(detectorIdentifier, out var pendingDetector))
                        {
                            v4DetectorsToTracking.Add((v4Detector, pendingDetector));
                            _logger.LogInformation("Skipping duplicate detector in current batch: {DetectorIdentifier}", detectorIdentifier);
                            continue;
                        }

                        var approachId = _approachMap[v4Detector.ApproachID];

                        var detector = new Detector
                        {
                            ApproachId = approachId,
                            DectectorIdentifier = detectorIdentifier,
                            DetectorChannel = v4Detector.DetChannel,
                            DistanceFromStopBar = v4Detector.DistanceFromStopBar,
                            MinSpeedFilter = v4Detector.MinSpeedFilter,
                            DateAdded = v4Detector.DateAdded,
                            DateDisabled = v4Detector.DateDisabled,
                            LaneNumber = v4Detector.LaneNumber,
                            MovementType = v4Detector.MovementTypeID.HasValue ? (MovementTypes)v4Detector.MovementTypeID.Value : MovementTypes.NA,
                            LaneType = v4Detector.LaneTypeID.HasValue ? (LaneTypes)v4Detector.LaneTypeID.Value : LaneTypes.NA,
                            DetectionHardware = (DetectionHardwareTypes)v4Detector.DetectionHardwareID,
                            DecisionPoint = v4Detector.DecisionPoint,
                            MovementDelay = v4Detector.MovementDelay,
                            LatencyCorrection = v4Detector.LatencyCorrection
                        };

                        detectorsToAdd.Add(detector);
                        v4DetectorsToTracking.Add((v4Detector, detector));
                        pendingDetectorsByIdentifier[detectorIdentifier] = detector;
                        _logger.LogDebug("Prepared detector: {DetectorIdentifier}", detector.DectectorIdentifier);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error preparing detector {DetectorID}", v4Detector.DetectorID);
                        throw;
                    }
                }

                // Batch add
                if (detectorsToAdd.Count > 0)
                {
                    _detectorRepository.AddRange(detectorsToAdd);
                    _logger.LogInformation("Added {Count} detectors to repository", detectorsToAdd.Count);

                    // Save changes to generate Detector IDs
                    await _configContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Persisted {Count} detectors to database", detectorsToAdd.Count);

                    // Map the newly added detectors
                    foreach (var (v4, v5) in v4DetectorsToTracking)
                    {
                        if (v5.Id > 0)
                        {
                            _detectorMap[v4.ID] = v5.Id;
                            existingDetectorIdentifierToId[v5.DectectorIdentifier] = v5.Id;
                            _logger.LogDebug("Mapped v4 Detector {DetectorID} to v5 Detector {Id}", v4.ID, v5.Id);
                        }
                    }

                    detectorsToAdd.Clear();
                    v4DetectorsToTracking.Clear();
                }
            }

            _logger.LogInformation("Detector migration completed. Migrated {Count} detectors", _detectorMap.Count);
        }

        private async Task MigrateDetectorCommentsAsync(List<V4DetectorComment> v4Comments, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting detector comment migration...");
            var commentsToAdd = new List<DetectorComment>();

            var existingCommentKeys = _configContext.DetectorComments
                .AsNoTracking()
                .Select(c => new { c.DetectorId, c.TimeStamp })
                .ToList()
                .Select(c => $"{c.DetectorId}|{c.TimeStamp.Ticks}")
                .ToHashSet();

            for (int i = 0; i < v4Comments.Count; i += BatchSize)
            {
                var batch = v4Comments.Skip(i).Take(BatchSize).ToList();

                foreach (var v4Comment in batch)
                {
                    try
                    {
                        if (!_detectorMap.ContainsKey(v4Comment.ID))
                        {
                            _logger.LogWarning("Skipping comment {CommentID}: detector {ID} not found in mapping",
                                v4Comment.CommentID, v4Comment.ID);
                            continue;
                        }

                        var detectorId = _detectorMap[v4Comment.ID];
                        var commentKey = $"{detectorId}|{v4Comment.TimeStamp.Ticks}";

                        if (existingCommentKeys.Contains(commentKey))
                        {
                            _logger.LogInformation("Skipping duplicate detector comment for detector {DetectorId} at {TimeStamp}", detectorId, v4Comment.TimeStamp);
                            continue;
                        }

                        var comment = new DetectorComment
                        {
                            DetectorId = detectorId,
                            TimeStamp = v4Comment.TimeStamp,
                            Comment = v4Comment.CommentText ?? string.Empty
                        };

                        commentsToAdd.Add(comment);
                        existingCommentKeys.Add(commentKey);
                        _logger.LogDebug("Prepared comment: {CommentID}", v4Comment.CommentID);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error preparing comment {CommentID}", v4Comment.CommentID);
                        throw;
                    }
                }

                // Batch add
                if (commentsToAdd.Count > 0)
                {
                    _detectorCommentRepository.AddRange(commentsToAdd);
                    _logger.LogInformation("Added {Count} detector comments to repository", commentsToAdd.Count);

                    // Save changes to persist comments
                    await _configContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Persisted {Count} detector comments to database", commentsToAdd.Count);

                    commentsToAdd.Clear();
                }
            }

            _logger.LogInformation("Detector comment migration completed. Migrated {Count} comments", v4Comments.Count);
        }

        private DirectionTypes MapDirectionType(string v4Direction)
        {
            if (string.IsNullOrWhiteSpace(v4Direction))
                return DirectionTypes.NA;

            return v4Direction.ToUpperInvariant() switch
            {
                "N" or "NORTH" or "NB" or "NORTHBOUND" => DirectionTypes.NB,
                "S" or "SOUTH" or "SB" or "SOUTHBOUND" => DirectionTypes.SB,
                "E" or "EAST" or "EB" or "EASTBOUND" => DirectionTypes.EB,
                "W" or "WEST" or "WB" or "WESTBOUND" => DirectionTypes.WB,
                "NE" or "NORTHEAST" => DirectionTypes.NE,
                "NW" or "NORTHWEST" => DirectionTypes.NW,
                "SE" or "SOUTHEAST" => DirectionTypes.SE,
                "SW" or "SOUTHWEST" => DirectionTypes.SW,
                _ => DirectionTypes.NA
            };
        }

        private DirectionTypes MapDirectionTypeFromId(int directionTypeId)
        {
            // Map v4 DirectionTypeID to v5 DirectionTypes enum
            // Common v4 values: 1=NB, 2=SB, 3=EB, 4=WB, 5=NE, 6=NW, 7=SE, 8=SW, 9=NA, 0=NA
            return directionTypeId switch
            {
                1 => DirectionTypes.NB,
                2 => DirectionTypes.SB,
                3 => DirectionTypes.EB,
                4 => DirectionTypes.WB,
                5 => DirectionTypes.NE,
                6 => DirectionTypes.NW,
                7 => DirectionTypes.SE,
                8 => DirectionTypes.SW,
                _ => DirectionTypes.NA
            };
        }
    }

    // Data transfer objects for v4 entities
    internal class V4Signal
    {
        public string SignalID { get; set; }
        public string PrimaryName { get; set; }
        public string SecondaryName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Note { get; set; }
        public int? RegionID { get; set; }
        public int? JurisdictionId { get; set; }
        public bool Pedsare1to1 { get; set; }
    }

    internal class V4Approach
    {
        public int ApproachID { get; set; }
        public string SignalID { get; set; }
        public int DirectionTypeID { get; set; }
        public string Description { get; set; }
        public int ProtectedPhaseNumber { get; set; }
        public int? PermissivePhaseNumber { get; set; }
        public int? PedestrianPhaseNumber { get; set; }
        public int? Mph { get; set; }
        public bool IsProtectedPhaseOverlap { get; set; }
        public bool IsPermissivePhaseOverlap { get; set; }
        public bool IsPedestrianPhaseOverlap { get; set; }
        public string PedestrianDetectors { get; set; }
    }

    internal class V4Detector
    {
        public int ID { get; set; }
        public int ApproachID { get; set; }
        public string DetectorID { get; set; }
        public int DetChannel { get; set; }
        public int? DistanceFromStopBar { get; set; }
        public int? MinSpeedFilter { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateDisabled { get; set; }
        public int? LaneNumber { get; set; }
        public int? MovementTypeID { get; set; }
        public int? LaneTypeID { get; set; }
        public int DetectionHardwareID { get; set; }
        public int? DecisionPoint { get; set; }
        public int? MovementDelay { get; set; }
        public double LatencyCorrection { get; set; }
    }

    internal class V4DetectorComment
    {
        public int CommentID { get; set; }
        public int ID { get; set; }
        public DateTime TimeStamp { get; set; }
        public string CommentText { get; set; }
    }
}
