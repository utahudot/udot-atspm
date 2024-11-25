#region license
// Copyright 2024 Utah Departement of Transportation
// for DatabaseInstaller - DatabaseInstaller.Services/CopyConfigCommandHostedService.cs
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;
using Utah.Udot.Atspm.Business.PedDelay;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;

namespace DatabaseInstaller.Services
{
    public class CopyConfigCommandHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<CopyConfigCommandConfiguration> _config;
        private readonly ILogger<CopyConfigCommandHostedService> _logger;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IProductRepository _productRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IApproachRepository _approachRepository;
        private readonly IDetectorRepository _detectorRepository;
        private readonly IDeviceConfigurationRepository _deviceConfigurationRepository;
        private readonly IDetectionTypeRepository _detectionTypeRepository;

        public CopyConfigCommandHostedService(
            IServiceProvider serviceProvider,
            IOptions<CopyConfigCommandConfiguration> config,
            ILogger<CopyConfigCommandHostedService> logger,
            IHostApplicationLifetime hostApplicationLifetime,
            IConfiguration appSettings,
            IProductRepository productRepository,
            ILocationRepository locationRepository,
            IApproachRepository approachRepository,
            IDetectorRepository detectorRepository,
            IDeviceConfigurationRepository deviceConfigurationRepository,
            IDetectionTypeRepository detectionTypeRepository)
        {
            _serviceProvider = serviceProvider;
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger;
            _hostApplicationLifetime = hostApplicationLifetime;
            _productRepository = productRepository;
            _locationRepository = locationRepository;
            _approachRepository = approachRepository;
            _detectorRepository = detectorRepository;
            _deviceConfigurationRepository = deviceConfigurationRepository;
            _detectionTypeRepository = detectionTypeRepository;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting CopyConfigCommandHostedService...");

            try
            {
                _logger.LogInformation($"Source: {_config.Value.Source}, Target: {_config.Value.Target}");
                await MigrateProducts();
                await CreateDeviceConfigurationsFromControllerTypes();
                await SyncLocationsFromSourceWithApproachesAndDetectors();
                _logger.LogInformation("Data copied and bulk inserted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while copying and bulk inserting data.");
                throw;
            }
        }


        public async Task MigrateProducts()
        {
            var productsToInsert = new List<Product>
            {
                new Product { Id = 1, Manufacturer = "Econolite", Model = "ASC3" },
                new Product { Id = 2, Manufacturer = "QFree", Model = "Cobalt" },
                new Product { Id = 3, Manufacturer = "Econolite", Model = "2070" },
                new Product { Id = 4, Manufacturer = "QFree", Model = "MaxTime" },
                new Product { Id = 5, Manufacturer = "Trafficware", Model = "Trafficware" },
                new Product { Id = 6, Manufacturer = "Siemens", Model = "SEPAC" },
                new Product { Id = 7, Manufacturer = "McCain", Model = "ATC EX" },
                new Product { Id = 8, Manufacturer = "Peek", Model = "Peek" },
                new Product { Id = 9, Manufacturer = "Econolite", Model = "EOS" },
                new Product { Id = 10, Manufacturer = "Econolite", Model = "32.68.40" }
            };
            var existingProducts = _productRepository.GetList().ToList();
            foreach (var product in productsToInsert)
            {
                var existingProduct = existingProducts.FirstOrDefault(p => p.Manufacturer == product.Manufacturer && p.Model == product.Model);
                if (existingProduct == null)
                {
                    _logger.LogInformation($"Adding new product: {product.Manufacturer} - {product.Model}");
                    _productRepository.Add(product);
                }
                else
                {
                    _logger.LogInformation($"Product already exists: {product.Manufacturer} - {product.Model}");
                }
            }
        }

        public async Task CreateDeviceConfigurationsFromControllerTypes()
        {
            _logger.LogInformation("Creating DeviceConfigurations based on ControllerTypes...");

            var deviceConfigurations = new List<DeviceConfiguration>();

            // Fetch existing products from the target database
            var existingProducts = _productRepository.GetList().ToList();

            // Fetch existing device configurations from the target database
            var existingDeviceConfigurations = _deviceConfigurationRepository.GetList().ToList();

            // Define the mapping between ControllerType.Description and Product (Manufacturer and Model)
            var controllerTypeToProductMap = new Dictionary<string, (string Manufacturer, string Model)>
    {
        { "ASC3", ("Econolite", "ASC3") },
        { "Cobalt", ("QFree", "Cobalt") },
        { "ASC3 - 2070", ("Econolite", "2070") },
        { "MaxTime", ("QFree", "MaxTime") },
        { "Trafficware", ("Trafficware", "Trafficware") },
        { "Siemens SEPAC", ("Siemens", "SEPAC") },
        { "McCain ATC EX", ("McCain", "ATC EX") },
        { "Peek", ("Peek", "Peek") },
        { "EOS", ("Econolite", "EOS") },
        { "ASC3-32.68.40", ("Econolite", "32.68.40") }
    };

            using var sqlConnection = new SqlConnection(_config.Value.Source);
            sqlConnection.Open();

            // Fetch Controller Types from the source
            var controllerTypeCommand = new SqlCommand("SELECT * FROM ControllerTypes", sqlConnection);
            using (var controllerTypeReader = await controllerTypeCommand.ExecuteReaderAsync())
            {
                while (await controllerTypeReader.ReadAsync())
                {
                    var controllerTypeDescription = controllerTypeReader.GetString(controllerTypeReader.GetOrdinal("Description"));
                    var snmpPort = controllerTypeReader.GetInt64(controllerTypeReader.GetOrdinal("SNMPPort"));
                    var id = controllerTypeReader.GetInt32(controllerTypeReader.GetOrdinal("ControllerTypeID"));
                    var ftpDirectory = controllerTypeReader.GetString(controllerTypeReader.GetOrdinal("FTPDirectory"));
                    var activeFTP = controllerTypeReader.GetBoolean(controllerTypeReader.GetOrdinal("ActiveFTP"));
                    var userName = controllerTypeReader.IsDBNull(controllerTypeReader.GetOrdinal("UserName"))
                        ? null
                        : controllerTypeReader.GetString(controllerTypeReader.GetOrdinal("UserName"));
                    var password = controllerTypeReader.IsDBNull(controllerTypeReader.GetOrdinal("Password"))
                        ? null
                        : controllerTypeReader.GetString(controllerTypeReader.GetOrdinal("Password"));

                    // Check if the device configuration already exists
                    var existingConfig = existingDeviceConfigurations.FirstOrDefault(dc =>
                        dc.Firmware == controllerTypeDescription &&
                        dc.Port == (int)snmpPort &&
                        dc.Directory == ftpDirectory &&
                        dc.UserName == userName);

                    if (existingConfig != null)
                    {
                        _logger.LogInformation($"DeviceConfiguration already exists for Firmware {controllerTypeDescription}. Skipping...");
                        continue;  // Skip adding this configuration as it already exists
                    }

                    // Create a new DeviceConfiguration in the target
                    var deviceConfiguration = new DeviceConfiguration
                    {
                        Id = id,
                        Firmware = controllerTypeDescription,  // Use ControllerType.Description as Firmware (adjust as needed)
                        Directory = ftpDirectory,              // Use FTP directory as notes
                        Protocol = TransportProtocols.Sftp,    // Example protocol, adjust as needed
                        Port = (int)snmpPort,
                        UserName = userName,
                        Password = password
                    };

                    // Map the ControllerType to a Product using the mapping and then find the Product in the database
                    if (controllerTypeToProductMap.TryGetValue(controllerTypeDescription, out var productMapping))
                    {
                        var matchingProduct = existingProducts.FirstOrDefault(p => p.Manufacturer == productMapping.Manufacturer && p.Model == productMapping.Model);
                        if (matchingProduct != null)
                        {
                            deviceConfiguration.ProductId = matchingProduct.Id;  // Set the correct ProductId
                        }
                        else
                        {
                            _logger.LogWarning($"No matching product found for ControllerType {controllerTypeDescription}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"No mapping found for ControllerType {controllerTypeDescription}");
                    }

                    deviceConfigurations.Add(deviceConfiguration);
                }
            }

            sqlConnection.Close();

            // Save the new device configurations to the database
            foreach (var deviceConfig in deviceConfigurations)
            {
                _logger.LogInformation($"Creating DeviceConfiguration for Firmware {deviceConfig.Firmware} with ProductId {deviceConfig.ProductId}");
                _deviceConfigurationRepository.Add(deviceConfig);
            }

            _logger.LogInformation("DeviceConfigurations creation complete.");
        }


        public async Task<Dictionary<int, List<int>>> GetDetectionTypesMap()
        {
            var detectionTypeMap = new Dictionary<int, List<int>>();
            using (var sqlConnectionTarget = new SqlConnection(_config.Value.Source))
            {
                sqlConnectionTarget.Open();
                var detectionTypeDetectorCommand = new SqlCommand("SELECT * FROM DetectionTypeDetector", sqlConnectionTarget);
                using (var reader = await detectionTypeDetectorCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int detectorId = reader.GetInt32(reader.GetOrdinal("ID"));
                        int detectionTypeId = reader.GetInt32(reader.GetOrdinal("DetectionTypeID"));

                        if (!detectionTypeMap.ContainsKey(detectorId))
                        {
                            detectionTypeMap[detectorId] = new List<int>();
                        }
                        detectionTypeMap[detectorId].Add(detectionTypeId);
                    }
                }
                sqlConnectionTarget.Close();
            }
            return detectionTypeMap;
        }


        public async Task SyncLocationsFromSourceWithApproachesAndDetectors()
        {
            _logger.LogInformation("Syncing locations with approaches and detectors from source...");

            var locationsFromSource = new List<Location>();
            var approachesFromSource = new List<Approach>();
            var detectorsFromSource = new List<Detector>();

            using var sqlConnection = new SqlConnection(_config.Value.Source);
            sqlConnection.Open();

            // Fetch locations
            var locationCommand = new SqlCommand("SELECT * FROM Signals", sqlConnection);
            using (var locationReader = await locationCommand.ExecuteReaderAsync())
            {
                while (await locationReader.ReadAsync())
                {
                    var id = locationReader.GetInt32(locationReader.GetOrdinal("VersionID"));
                    var controllerTypeId = locationReader.GetInt32(locationReader.GetOrdinal("ControllerTypeID"));
                    var location = new Location
                    {
                        Id = id,
                        LocationIdentifier = locationReader.GetString(locationReader.GetOrdinal("SignalID")),
                        PrimaryName = locationReader.IsDBNull(locationReader.GetOrdinal("PrimaryName"))
                    ? null
                    : locationReader.GetString(locationReader.GetOrdinal("PrimaryName")),
                        SecondaryName = locationReader.IsDBNull(locationReader.GetOrdinal("SecondaryName"))
                    ? null
                    : locationReader.GetString(locationReader.GetOrdinal("SecondaryName")),
                        Latitude = locationReader.IsDBNull(locationReader.GetOrdinal("Latitude"))
                    ? 0.0  // Default value for double when Latitude is null
                    : Convert.ToDouble(locationReader.GetString(locationReader.GetOrdinal("Latitude"))),

                        Longitude = locationReader.IsDBNull(locationReader.GetOrdinal("Longitude"))
                    ? 0.0  // Default value for double when Longitude is null
                    : Convert.ToDouble(locationReader.GetString(locationReader.GetOrdinal("Longitude"))),
                        JurisdictionId = locationReader.IsDBNull(locationReader.GetOrdinal("JurisdictionID"))
                    ? (int?)null
                    : locationReader.GetInt32(locationReader.GetOrdinal("JurisdictionID")),
                        RegionId = locationReader.IsDBNull(locationReader.GetOrdinal("RegionID"))
                    ? (int?)null
                    : locationReader.GetInt32(locationReader.GetOrdinal("RegionID")),
                        Start = locationReader.IsDBNull(locationReader.GetOrdinal("Start"))
                    ? DateTime.MinValue // Default value if the Start is null
                    : locationReader.GetDateTime(locationReader.GetOrdinal("Start")),
                        Note = locationReader.IsDBNull(locationReader.GetOrdinal("Note"))
                    ? null
                    : locationReader.GetString(locationReader.GetOrdinal("Note")),
                        ChartEnabled = locationReader.IsDBNull(locationReader.GetOrdinal("Enabled"))
                    ? false
                    : locationReader.GetBoolean(locationReader.GetOrdinal("Enabled")),
                        PedsAre1to1 = locationReader.IsDBNull(locationReader.GetOrdinal("Pedsare1to1"))
                    ? false
                    : locationReader.GetBoolean(locationReader.GetOrdinal("Pedsare1to1")),
                        VersionAction = locationReader.IsDBNull(locationReader.GetOrdinal("VersionActionId"))
                    ? LocationVersionActions.Unknown // Ensure you have an 'NA' or 'Unknown' value in your enum
                    : (LocationVersionActions)locationReader.GetInt32(locationReader.GetOrdinal("VersionActionId")),
                        LocationTypeId = 1,
                        Devices = new List<Device>
                {
                    new Device
                        {
                            LocationId = id,
                            DeviceConfigurationId = controllerTypeId,  // Set controllerTypeId as deviceConfigurationId
                            Ipaddress = "127.0.0.1",
                            DeviceStatus = DeviceStatus.Active,
                            DeviceType = DeviceTypes.SignalController,
                            LoggingEnabled = true
                        }
                }
                    };
                    locationsFromSource.Add(location);
                }
            }

            // Fetch approaches
            var approachCommand = new SqlCommand("SELECT * FROM Approaches", sqlConnection);
            using (var approachReader = await approachCommand.ExecuteReaderAsync())
            {
                while (await approachReader.ReadAsync())
                {
                    var approach = new Approach
                    {
                        Id = approachReader.GetInt32(approachReader.GetOrdinal("ApproachID")),
                        LocationId = approachReader.GetInt32(approachReader.GetOrdinal("VersionID")),
                        Description = approachReader.IsDBNull(approachReader.GetOrdinal("Description"))
                            ? null
                            : approachReader.GetString(approachReader.GetOrdinal("Description")),
                        Mph = approachReader.IsDBNull(approachReader.GetOrdinal("MPH"))
                            ? (int?)null
                            : approachReader.GetInt32(approachReader.GetOrdinal("MPH")),
                        ProtectedPhaseNumber = approachReader.IsDBNull(approachReader.GetOrdinal("ProtectedPhaseNumber"))
                            ? default(int)
                            : approachReader.GetInt32(approachReader.GetOrdinal("ProtectedPhaseNumber")),
                        IsProtectedPhaseOverlap = approachReader.IsDBNull(approachReader.GetOrdinal("IsProtectedPhaseOverlap"))
                            ? false
                            : approachReader.GetBoolean(approachReader.GetOrdinal("IsProtectedPhaseOverlap")),
                        PermissivePhaseNumber = approachReader.IsDBNull(approachReader.GetOrdinal("PermissivePhaseNumber"))
                            ? (int?)null
                            : approachReader.GetInt32(approachReader.GetOrdinal("PermissivePhaseNumber")),
                        IsPermissivePhaseOverlap = approachReader.IsDBNull(approachReader.GetOrdinal("IsPermissivePhaseOverlap"))
                            ? false
                            : approachReader.GetBoolean(approachReader.GetOrdinal("IsPermissivePhaseOverlap")),
                        PedestrianPhaseNumber = approachReader.IsDBNull(approachReader.GetOrdinal("PedestrianPhaseNumber"))
                            ? (int?)null
                            : approachReader.GetInt32(approachReader.GetOrdinal("PedestrianPhaseNumber")),
                        IsPedestrianPhaseOverlap = approachReader.IsDBNull(approachReader.GetOrdinal("IsPedestrianPhaseOverlap"))
                            ? false
                            : approachReader.GetBoolean(approachReader.GetOrdinal("IsPedestrianPhaseOverlap")),
                        PedestrianDetectors = approachReader.IsDBNull(approachReader.GetOrdinal("PedestrianDetectors"))
                            ? null
                            : approachReader.GetString(approachReader.GetOrdinal("PedestrianDetectors")),
                        DirectionTypeId = approachReader.IsDBNull(approachReader.GetOrdinal("DirectionTypeID"))
                            ? DirectionTypes.NA
                            : (DirectionTypes)approachReader.GetInt32(approachReader.GetOrdinal("DirectionTypeID")),
                    };
                    approachesFromSource.Add(approach);
                }
            }

            var detectionTypeMap = await GetDetectionTypesMap();
            var existingDetectionTypesDict = _detectionTypeRepository.GetList().ToList().ToDictionary(dt => (int)dt.Id, dt => dt);


            // Fetch detectors
            var detectorCommand = new SqlCommand("SELECT * FROM Detectors", sqlConnection);
            using (var detectorReader = await detectorCommand.ExecuteReaderAsync())
            {
                while (await detectorReader.ReadAsync())
                {
                    var detector = new Detector
                    {
                        Id = detectorReader.GetInt32(detectorReader.GetOrdinal("ID")),
                        DectectorIdentifier = detectorReader.IsDBNull(detectorReader.GetOrdinal("DetectorID"))
                            ? null
                            : detectorReader.GetString(detectorReader.GetOrdinal("DetectorID")),
                        DetectorChannel = detectorReader.IsDBNull(detectorReader.GetOrdinal("DetChannel"))
                            ? 0
                            : detectorReader.GetInt32(detectorReader.GetOrdinal("DetChannel")),
                        DistanceFromStopBar = detectorReader.IsDBNull(detectorReader.GetOrdinal("DistanceFromStopBar"))
                            ? (int?)null
                            : detectorReader.GetInt32(detectorReader.GetOrdinal("DistanceFromStopBar")),
                        MinSpeedFilter = detectorReader.IsDBNull(detectorReader.GetOrdinal("MinSpeedFilter"))
                            ? (int?)null
                            : detectorReader.GetInt32(detectorReader.GetOrdinal("MinSpeedFilter")),
                        DateAdded = detectorReader.IsDBNull(detectorReader.GetOrdinal("DateAdded"))
                            ? DateTime.MinValue
                            : detectorReader.GetDateTime(detectorReader.GetOrdinal("DateAdded")),
                        DateDisabled = detectorReader.IsDBNull(detectorReader.GetOrdinal("DateDisabled"))
                            ? (DateTime?)null
                            : detectorReader.GetDateTime(detectorReader.GetOrdinal("DateDisabled")),
                        LaneNumber = detectorReader.IsDBNull(detectorReader.GetOrdinal("LaneNumber"))
                            ? (int?)null
                            : detectorReader.GetInt32(detectorReader.GetOrdinal("LaneNumber")),
                        MovementType = detectorReader.IsDBNull(detectorReader.GetOrdinal("MovementTypeID"))
                            ? MovementTypes.NA
                            : (MovementTypes)detectorReader.GetInt32(detectorReader.GetOrdinal("MovementTypeID")),
                        LaneType = detectorReader.IsDBNull(detectorReader.GetOrdinal("LaneTypeID"))
                            ? LaneTypes.NA
                            : (LaneTypes)detectorReader.GetInt32(detectorReader.GetOrdinal("LaneTypeID")),
                        DecisionPoint = detectorReader.IsDBNull(detectorReader.GetOrdinal("DecisionPoint"))
                            ? (int?)null
                            : detectorReader.GetInt32(detectorReader.GetOrdinal("DecisionPoint")),
                        MovementDelay = detectorReader.IsDBNull(detectorReader.GetOrdinal("MovementDelay"))
                            ? (int?)null
                            : detectorReader.GetInt32(detectorReader.GetOrdinal("MovementDelay")),
                        LatencyCorrection = detectorReader.IsDBNull(detectorReader.GetOrdinal("LatencyCorrection"))
                            ? 0.0
                            : detectorReader.GetDouble(detectorReader.GetOrdinal("LatencyCorrection")),
                        ApproachId = detectorReader.IsDBNull(detectorReader.GetOrdinal("ApproachID"))
                            ? default(int)
                            : detectorReader.GetInt32(detectorReader.GetOrdinal("ApproachID")),
                        DetectionHardware = detectorReader.IsDBNull(detectorReader.GetOrdinal("DetectionHardwareID"))
                            ? DetectionHardwareTypes.NA
                            : (DetectionHardwareTypes)detectorReader.GetInt32(detectorReader.GetOrdinal("DetectionHardwareID"))
                    };
                    if (detectionTypeMap.ContainsKey(detector.Id))
                    {
                        foreach (var detectionTypeId in detectionTypeMap[detector.Id])
                        {
                            if (existingDetectionTypesDict.TryGetValue(detectionTypeId, out var detectionType))
                            {
                                _logger.LogInformation($"Adding DetectionType {detectionType.Description} to Detector {detector.Id}");
                                detector.DetectionTypes.Add(detectionType);
                            }
                        }
                    }

                    detectorsFromSource.Add(detector);
                }
            }

            sqlConnection.Close();

            // Add approaches to locations
            foreach (var location in locationsFromSource)
            {
                location.Approaches = approachesFromSource.Where(a => a.LocationId == location.Id).ToList();

                // Add detectors to approaches
                foreach (var approach in location.Approaches)
                {
                    approach.Detectors = detectorsFromSource.Where(d => d.ApproachId == approach.Id).ToList();
                }
            }

            // Now compare using hashes
            await SyncLocationsToTarget(locationsFromSource);
        }



        public async Task SyncLocationsToTarget(List<Location> locationsFromSource)
        {
            var existingLocations = _locationRepository.GetList()
                .Include(i => i.Devices)
                .Include(i => i.Approaches)
                .ThenInclude(i => i.Detectors)
                .ToList();

            foreach (var sourceLocation in locationsFromSource)
            {
                var targetLocation = existingLocations.FirstOrDefault(l => l.Id == sourceLocation.Id);

                if (targetLocation != null && sourceLocation != null)
                {
                    // Generate hashes for comparison
                    //string sourceHash = ComputeObjectHash(sourceLocation);
                    //string targetHash = ComputeObjectHash(targetLocation);

                    if (!LocationComparer.AreEqual(targetLocation, sourceLocation))
                    {
                        _logger.LogInformation($"Location {sourceLocation.LocationIdentifier} has changed. Updating...");
                        _locationRepository.Update(sourceLocation);
                    }
                }
                else
                {
                    _logger.LogInformation($"New location found: {sourceLocation.LocationIdentifier}. Adding...");
                    _locationRepository.Add(sourceLocation);
                }
            }
        }




        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public static class LocationComparer
    {
        public static bool AreEqual(Location source, Location target)
        {
            if (source == null || target == null) return false;

            // Compare basic properties
            bool areBasicPropertiesEqual =
                source.Id == target.Id &&
                source.LocationIdentifier == target.LocationIdentifier &&
                source.PrimaryName == target.PrimaryName &&
                source.SecondaryName == target.SecondaryName &&
                source.Latitude == target.Latitude &&
                source.Longitude == target.Longitude &&
                source.ChartEnabled == target.ChartEnabled &&
                source.VersionAction == target.VersionAction &&
                source.Note == target.Note &&
                source.Start == target.Start &&
                source.PedsAre1to1 == target.PedsAre1to1;

            // Compare related collections (Devices, Approaches)
            bool areDevicesEqual = CompareDevices(source.Devices, target.Devices);
            bool areApproachesEqual = CompareApproaches(source.Approaches, target.Approaches);

            return areBasicPropertiesEqual && areDevicesEqual && areApproachesEqual;
        }

        private static bool CompareDevices(ICollection<Device> sourceDevices, ICollection<Device> targetDevices)
        {
            if (sourceDevices.Count != targetDevices.Count) return false;

            foreach (var sourceDevice in sourceDevices)
            {
                var targetDevice = targetDevices.FirstOrDefault(d => d.Id == sourceDevice.Id);
                if (targetDevice == null || !DeviceComparer.AreEqual(sourceDevice, targetDevice))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool CompareApproaches(ICollection<Approach> sourceApproaches, ICollection<Approach> targetApproaches)
        {
            if (sourceApproaches.Count != targetApproaches.Count) return false;

            foreach (var sourceApproach in sourceApproaches)
            {
                var targetApproach = targetApproaches.FirstOrDefault(a => a.Id == sourceApproach.Id);
                if (targetApproach == null || !ApproachComparer.AreEqual(sourceApproach, targetApproach))
                {
                    return false;
                }
            }
            return true;
        }
    }

    public static class ApproachComparer
    {
        public static bool AreEqual(Approach source, Approach target)
        {
            if (source == null || target == null) return false;

            bool areBasicPropertiesEqual =
                source.Id == target.Id &&
                source.Description == target.Description &&
                source.Mph == target.Mph &&
                source.ProtectedPhaseNumber == target.ProtectedPhaseNumber &&
                source.IsProtectedPhaseOverlap == target.IsProtectedPhaseOverlap &&
                source.PermissivePhaseNumber == target.PermissivePhaseNumber &&
                source.IsPermissivePhaseOverlap == target.IsPermissivePhaseOverlap &&
                source.PedestrianPhaseNumber == target.PedestrianPhaseNumber &&
                source.IsPedestrianPhaseOverlap == target.IsPedestrianPhaseOverlap &&
                source.PedestrianDetectors == target.PedestrianDetectors;

            // Compare related detectors
            bool areDetectorsEqual = CompareDetectors(source.Detectors, target.Detectors);

            return areBasicPropertiesEqual && areDetectorsEqual;
        }

        private static bool CompareDetectors(ICollection<Detector> sourceDetectors, ICollection<Detector> targetDetectors)
        {
            if (sourceDetectors.Count != targetDetectors.Count) return false;

            foreach (var sourceDetector in sourceDetectors)
            {
                var targetDetector = targetDetectors.FirstOrDefault(d => d.Id == sourceDetector.Id);
                if (targetDetector == null || !DetectorComparer.AreEqual(sourceDetector, targetDetector))
                {
                    return false;
                }
            }
            return true;
        }
    }

    public static class DetectorComparer
    {
        public static bool AreEqual(Detector source, Detector target)
        {
            if (source == null || target == null) return false;

            return source.Id == target.Id &&
                   source.DectectorIdentifier == target.DectectorIdentifier &&
                   source.DetectorChannel == target.DetectorChannel &&
                   source.DistanceFromStopBar == target.DistanceFromStopBar &&
                   source.MinSpeedFilter == target.MinSpeedFilter &&
                   source.DateAdded == target.DateAdded &&
                   source.DateDisabled == target.DateDisabled &&
                   source.LaneNumber == target.LaneNumber &&
                   source.MovementType == target.MovementType &&
                   source.LaneType == target.LaneType &&
                   source.DecisionPoint == target.DecisionPoint &&
                   source.MovementDelay == target.MovementDelay &&
                   source.LatencyCorrection == target.LatencyCorrection;
        }
    }


    public static class DeviceComparer
    {
        public static bool AreEqual(Device source, Device target)
        {
            if (source == null || target == null) return false;

            return source.Id == target.Id &&
                   source.Ipaddress == target.Ipaddress &&
                   source.DeviceStatus == target.DeviceStatus &&
                   source.DeviceType == target.DeviceType &&
                   source.LoggingEnabled == target.LoggingEnabled &&
                   source.DeviceConfigurationId == target.DeviceConfigurationId;
        }
    }



}
