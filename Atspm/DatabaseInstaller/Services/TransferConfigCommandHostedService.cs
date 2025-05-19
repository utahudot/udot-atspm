#region license
// Copyright 2025 Utah Departement of Transportation
// for DatabaseInstaller - %Namespace%/TransferConfigCommandHostedService.cs
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;

public class TransferConfigCommandHostedService : IHostedService
{
    private readonly ILogger<TransferConfigCommandHostedService> _logger;
    private readonly IJurisdictionRepository _jurisdictionRepository;
    private readonly ILocationTypeRepository _locationTypeRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly IApproachRepository _approachRepository;
    private readonly IDetectorRepository _detectorRepository;
    private readonly IDeviceConfigurationRepository _deviceConfigurationRepository;
    private readonly IProductRepository _productRepository;
    private readonly IRegionsRepository _regionsRepository;
    private readonly IAreaRepository _areaRepository;
    private readonly IDetectionTypeRepository _detectionTypeRepository;
    private readonly IMeasureTypeRepository _measureTypeRepository;
    private readonly IRouteRepository _routeRepository;
    private readonly IRouteLocationsRepository _routeLocationsRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly IDeviceRepository _deviceRepository;
    private readonly TransferConfigCommandConfiguration _config;

    public TransferConfigCommandHostedService(
        ILogger<TransferConfigCommandHostedService> logger,
        IJurisdictionRepository jurisdictionRepository,
        ILocationTypeRepository locationTypeRepository,
        ILocationRepository locationRepository,
        IApproachRepository approachRepository,
        IDetectorRepository detectorRepository,
        IDeviceRepository deviceRepository,
        IDeviceConfigurationRepository deviceConfigurationRepository,
        IProductRepository productRepository,
        IRegionsRepository regionsRepository,
        IAreaRepository areaRepository,
        IDetectionTypeRepository detectionTypeRepository,
        IMeasureTypeRepository measureTypeRepository,
        IRouteRepository routeRepository,
        IRouteLocationsRepository routeLocationsRepository,
        IOptions<TransferConfigCommandConfiguration> config,
        IServiceProvider serviceProvider
        )
    {
        _logger = logger;
        _jurisdictionRepository = jurisdictionRepository;
        _locationTypeRepository = locationTypeRepository;
        _locationRepository = locationRepository;
        _approachRepository = approachRepository;
        _detectorRepository = detectorRepository;
        _deviceConfigurationRepository = deviceConfigurationRepository;
        _productRepository = productRepository;
        _regionsRepository = regionsRepository;
        _areaRepository = areaRepository;
        _detectionTypeRepository = detectionTypeRepository;
        _measureTypeRepository = measureTypeRepository;
        _routeRepository = routeRepository;
        _routeLocationsRepository = routeLocationsRepository;
        _serviceProvider = serviceProvider;
        _deviceRepository = deviceRepository;
        _config = config.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_config.Delete)
        {
            DeleteLocations();
            DeleteRegions();
            DeleteAreas();
            DeleteJurisdictions();
            DeleteDevices();
            DeleteDevicesConfigurations();
            DeleteProducts();
        }

        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        Dictionary<string, string> queries = GetLocationQueries(config);
        var columnMappings = GetColumnMappings(config);
        if (_config.UpdateLocations)
        {
            SetDetectionTypeMesureType();
            ImportProducts(queries, columnMappings);
            ImportDeviceConfigurations(queries, columnMappings);
            ImportRegions(queries, columnMappings);
            ImportAreas(queries, columnMappings);
            ImportJurisdictions(queries, columnMappings);
            ImportLocations(queries, columnMappings);
            ImportApproaches(queries, columnMappings);
            ImportDetectors(queries, columnMappings);
            ImportRoutes(queries, columnMappings);
            ImportRouteLocations(queries, columnMappings);
            ImportDevices(queries, columnMappings);
            ResetSequences();
        }
        if (_config.ImportSpeedDevices)
        {
            ImportSpeedDevices(queries, columnMappings);
            ResetSequences();
        }
    }

    private void ResetSequences()
    {
        using var scope = _serviceProvider.CreateScope();
        var configContext = scope.ServiceProvider.GetRequiredService<ConfigContext>();

        // Check if the provider is PostgreSQL
        var databaseProvider = configContext.Database.ProviderName;
        if (databaseProvider != "Npgsql.EntityFrameworkCore.PostgreSQL")
        {
            Console.WriteLine("Skipping sequence reset: Database provider is not PostgreSQL.");
            return;
        }

        string[] sequences = new string[]
        {
        "\"Locations_Id_seq\"",
        "\"Jurisdictions_Id_seq\"",
        "\"Approaches_Id_seq\"",
        "\"Detectors_Id_seq\"",
        "\"Products_Id_seq\"",
        "\"DeviceConfigurations_Id_seq\"",
        "\"Regions_Id_seq\"",
        "\"Devices_Id_seq\"",
        "\"Routes_Id_seq\"",
        "\"Areas_Id_seq\"",
        "\"MenuItems_Id_seq\"",
        "\"RouteLocations_Id_seq\""
        };

        foreach (string sequence in sequences)
        {
            // Correctly format table name derived from sequence name
            string tableName = sequence.Replace("_Id_seq\"", "").Replace("\"", "");

            string query = $"SELECT setval('public.{sequence}',(SELECT COALESCE(MAX(\"Id\"), 0) FROM public.\"{tableName}\") + 1);";

            configContext.Database.ExecuteSqlRaw(query);

            Console.WriteLine($"Sequence {sequence} reset successfully.");
        }
    }


    private void ImportSpeedDevices(Dictionary<string, string> queries, Dictionary<string, Dictionary<string, string>> columnMappings)
    {
        var products = _productRepository.GetList().ToList();
        if (_productRepository.GetList().Any(p => p.Manufacturer == "Wavetronix"))
        {
            _logger.LogInformation("Speed Product already exist");
        }
        else
        {
            _productRepository.Add(new Product { Manufacturer = "Wavetronix", Model = "Speed Detection" });
        }
        if (_deviceConfigurationRepository.GetList().Any(dc => dc.Description == "Speed"))
        {
            _logger.LogInformation("Speed Device Configuration already exist");
        }
        else
        {
            _deviceConfigurationRepository.Add(new DeviceConfiguration
            {
                Description = "Speed",
                Protocol = TransportProtocols.Unknown,
                ConnectionTimeout = 2000,
                Path = "Unkown",
                OperationTimeout = 2000,
                Port = 0,
                UserName = "Unknown",
                Password = "Unkown",
                ProductId = _productRepository.GetList().First(p => p.Manufacturer == "Wavetronix").Id
            });
        }
        _logger.LogInformation($"Importing Speed Devices");
        var devices = ImportData<Device>(queries["SpeedDevices"], columnMappings["SpeedDevices"]);
        //check if device cofiguration exists
        var speedDeviceConfiguration = _deviceConfigurationRepository.GetList().First(dc => dc.Description == "Speed");
        if (speedDeviceConfiguration == null)
        {
            _logger.LogInformation($"Speed Device Configuration not found for configuration.");
            return;
        }
        foreach (var device in devices)
        {
            device.DeviceConfiguration = speedDeviceConfiguration;
            var location = _locationRepository.GetLatestVersionOfLocation(device.DeviceIdentifier);
            if (location == null)
            {
                _logger.LogInformation($"Location not found for device {device.DeviceIdentifier}");
                continue;
            }
            device.LocationId = location.Id;
        }
        if (_config.Delete)
        {
            try
            {
                _deviceRepository.AddRange(devices);
                _logger.LogInformation($"Speed Devices Imported");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Error importing speed devices");
                throw;
            }
        }
        else
        {
            var deviceIds = _deviceRepository.GetList().Select(d => d.Id).ToList();
            var newDevices = devices.Where(d => !deviceIds.Contains(d.Id)).ToList();
            foreach (var device in newDevices)
            {
                try
                {
                    _deviceRepository.Add(device);
                    _logger.LogInformation($"Speed Device ${device.DeviceIdentifier} Imported");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, $"Error importing speed device {device.DeviceIdentifier}");
                }
            }
        }
    }

    private void DeleteProducts()
    {
        _logger.LogInformation($"Deleting all products");
        try
        {
            _productRepository.RemoveRange(_productRepository.GetList().ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, "Error deleting products");
            throw;
        }
    }

    private void DeleteDevicesConfigurations()
    {
        _logger.LogInformation($"Deleting all device configurations");
        try
        {
            _deviceConfigurationRepository.RemoveRange(_deviceConfigurationRepository.GetList().ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, "Error deleting device configurations");
            throw;
        }
    }

    private void ImportJurisdictions(Dictionary<string, string> queries, Dictionary<string, Dictionary<string, string>> columnMappings)
    {
        if (_jurisdictionRepository.GetList().Any())
        {
            _logger.LogInformation("Jurisdictions already exist");
            return;
        }
        _logger.LogInformation($"Importing Jurisdictions...");
        var jurisdictions = ImportData<Jurisdiction>(queries["Jurisdictions"], columnMappings["Jurisdictions"]);
        _jurisdictionRepository.AddRange(jurisdictions);
        _logger.LogInformation($"Jurisdictions Imported");
    }

    private void ImportAreas(Dictionary<string, string> queries, Dictionary<string, Dictionary<string, string>> columnMappings)
    {
        if (_areaRepository.GetList().Any())
        {
            _logger.LogInformation("Areas already exist");
            return;
        }
        _logger.LogInformation($"Importing Areas...");
        var areas = ImportData<Area>(queries["Areas"], columnMappings["Areas"]);
        _areaRepository.AddRange(areas);
        _logger.LogInformation($"Areas Imported");
    }

    private void ImportRegions(Dictionary<string, string> queries, Dictionary<string, Dictionary<string, string>> columnMappings)
    {
        if (_regionsRepository.GetList().Any())
        {
            _logger.LogInformation("Regions already exist");
            return;
        }
        _logger.LogInformation($"Importing Regions...");
        var regions = ImportData<Region>(queries["Regions"], columnMappings["Regions"]);
        _regionsRepository.AddRange(regions);
        _logger.LogInformation($"Regions Imported");
    }

    private void ImportRoutes(Dictionary<string, string> queries, Dictionary<string, Dictionary<string, string>> columnMappings)
    {
        if (_routeRepository.GetList().Any())
        {
            _logger.LogInformation("Routes already exist");
            return;
        }
        _logger.LogInformation($"Importing Routes");
        var routes = ImportData<Route>(queries["Routes"], columnMappings["Routes"]);
        _routeRepository.AddRange(routes);
        _logger.LogInformation($"Routes Imported");
    }

    private void ImportRouteLocations(Dictionary<string, string> queries, Dictionary<string, Dictionary<string, string>> columnMappings)
    {
        if (_routeLocationsRepository.GetList().Any())
        {
            _logger.LogInformation("Route Locations already exist");
            return;
        }
        _logger.LogInformation($"Importing Route Locations");
        var routeLocations = ImportData<RouteLocation>(queries["RouteLocations"], columnMappings["RouteLocations"]);
        _routeLocationsRepository.AddRange(routeLocations);
        _logger.LogInformation($"Route Locations Imported");
    }


    private void ImportDeviceConfigurations(Dictionary<string, string> queries, Dictionary<string, Dictionary<string, string>> columnMappings)
    {
        if (_deviceConfigurationRepository.GetList().Any())
        {
            _logger.LogInformation("Device Configurations already exist");
            return;
        }
        _logger.LogInformation("Adding Device Configurations");
        var deviceConfigurations = ImportData<DeviceConfiguration>(queries["DeviceConfigurations"], columnMappings["DeviceConfigurations"]);
        _deviceConfigurationRepository.AddRange(deviceConfigurations);
        _logger.LogInformation("Device Configurations Added");
    }

    private void ImportProducts(Dictionary<string, string> queries, Dictionary<string, Dictionary<string, string>> columnMappings)
    {
        if (_productRepository.GetList().Any())
        {
            _logger.LogInformation("Products already exist");
            return;
        }
        _logger.LogInformation("Adding Products");
        var products = ImportData<Product>(queries["Products"], columnMappings["Products"]);
        _productRepository.AddRange(products);
        _logger.LogInformation("Products Added");
    }

    private void ImportApproaches(Dictionary<string, string> queries, Dictionary<string, Dictionary<string, string>> columnMappings)
    {
        if (_config.Delete && _approachRepository.GetList().Any())
        {
            _logger.LogInformation("Approaches already exist");
            return;
        }

        _logger.LogInformation($"Importing Approaches");

        // Import all approaches at once
        var approaches = ImportData<Approach>(queries["Approaches"], columnMappings["Approaches"]);

        if (_config.Delete == true)
        {
            const int batchSize = 5000;
            int total = approaches.Count;
            int batches = (int)Math.Ceiling(total / (double)batchSize);

            for (int i = 0; i < batches; i++)
            {
                var batch = approaches.Skip(i * batchSize).Take(batchSize).ToList();
                _approachRepository.AddRange(batch);
                _logger.LogInformation($"Batch {i + 1}/{batches} imported ({batch.Count} approaches).");
            }

            _logger.LogInformation($"All Approaches Imported");
        }
        else
        {
            var approachIds = approaches.Select(a => a.Id).ToList();
            //Get approaches that are not in the approachIds list
            var newApproaches = approaches.Where(a => !approachIds.Contains(a.Id)).ToList();
            foreach (var approach in newApproaches)
            {
                try
                {
                    _approachRepository.Add(approach);
                    _logger.LogInformation($"Approach {approach.Id} Imported");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, $"Error importing approach {approach.Id}");
                }

            }
        }
    }


    private void ImportDevices(Dictionary<string, string> queries, Dictionary<string, Dictionary<string, string>> columnMappings)
    {
        if (_deviceRepository.GetList().Any())
        {
            _logger.LogInformation("Devices already exist");
            return;
        }
        _logger.LogInformation($"Importing Devices");
        var devices = ImportData<Device>(queries["Devices"], columnMappings["Devices"]);
        //check if device cofiguration exists
        var configurations = _deviceConfigurationRepository.GetList().ToList();
        foreach (var device in devices)
        {
            var configuration = configurations.FirstOrDefault(c => c.Id == device.DeviceConfigurationId);
            if (configuration == null)
            {
                _logger.LogInformation($"Device Configuration not found for configuration {device.DeviceConfigurationId} on location{device.Notes}");
                continue;
            }
            device.DeviceConfiguration = configuration;
        }

        _deviceRepository.AddRange(devices);
        _logger.LogInformation($"Devices Imported");
    }

    private void ImportDetectors(Dictionary<string, string> queries, Dictionary<string, Dictionary<string, string>> columnMappings)
    {
        if (_config.Delete == true && _detectorRepository.GetList().Any())
        {
            _logger.LogInformation("Detectors already exist");
            return;
        }
        _logger.LogInformation($"Importing Detectors");

        var detectionTypes = _detectionTypeRepository.GetList().ToList();

        var detectionTypeDetectors = ImportData<DetectionTypeDetector>(queries["DetectionTypeDetector"], columnMappings["DetectionTypeDetector"]);

        var detectors = ImportData<Detector>(queries["Detectors"], columnMappings["Detectors"]);


        if (_config.Delete)
        {
            const int batchSize = 5000;
            for (int i = 0; i < detectors.Count; i += batchSize)
            {
                var batch = detectors.Skip(i).Take(batchSize).ToList();
                foreach (var detectionType in detectionTypes)
                {

                    if (detectionType.Id == DetectionTypes.B)
                    {
                        foreach (var detector in batch)
                        {
                            detectionType.Detectors.Add(detector);
                        }
                    }
                    else
                    {
                        var detectorIds = detectionTypeDetectors
                        .Where(d => d.DetectionTypesId == (int)detectionType.Id)
                        .Select(d => d.DetectorsId)
                        .ToList();
                        foreach (var detector in batch.Where(d => detectorIds.Contains(d.Id)))
                        {
                            detectionType.Detectors.Add(detector);
                        }
                    }
                }
                _detectorRepository.AddRange(batch);
                _logger.LogInformation($"Processed batch of {batch.Count} detectors");
            }

            _logger.LogInformation($"Detectors Imported");
        }
        else
        {
            var detectorIds = _detectorRepository.GetList().Select(d => d.Id).ToList();
            var newDetectors = detectors.Where(d => !detectorIds.Contains(d.Id)).ToList();
            foreach (var detector in newDetectors)
            {
                try
                {
                    _detectorRepository.Add(detector);
                    _logger.LogInformation($"Detector {detector.DectectorIdentifier} Imported");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, $"Error importing detector {detector.DectectorIdentifier}");
                }
            }
        }
    }

    private void ImportLocations(Dictionary<string, string> queries, Dictionary<string, Dictionary<string, string>> columnMappings)
    {
        if (_config.Delete == true && _locationRepository.GetList().Any())
        {
            _logger.LogInformation("Locations already exist");
            return;
        }
        _logger.LogInformation($"Importing Locations...");

        var areas = _areaRepository.GetList().ToList();

        var locationAreas = ImportData<AreaLocation>(queries["AreaLocations"], columnMappings["AreaLocations"]);

        var locations = ImportData<Location>(queries["Locations"], columnMappings["Locations"]);

        foreach (var area in areas)
        {
            var locationIds = locationAreas.Where(l => l.AreasId == area.Id).Select(l => l.LocationsId).ToList();
            foreach (var location in locations.Where(l => locationIds.Contains(l.Id)))
            {
                location.Areas.Add(area);
            }
        }
        if (_config.Delete)
        {
            try
            {
                _locationRepository.AddRange(locations);
                _logger.LogInformation($"Locations Imported");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Error importing locations");
                throw;
            }
        }
        else
        {
            var locationIds = _locationRepository.GetList().Select(l => l.Id).ToList();
            var newLocations = locations.Where(l => !locationIds.Contains(l.Id)).ToList();
            foreach (var location in newLocations)
            {
                try
                {
                    _locationRepository.Add(location);
                    _logger.LogInformation($"Location {location.LocationIdentifier} Imported");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, $"Error importing location {location.LocationIdentifier}");
                }
            }
        }
    }

    private void SetDetectionTypeMesureType()
    {
        if (_detectionTypeRepository.GetList()
            .Include(d => d.MeasureTypes)
            .SelectMany(d => d.MeasureTypes)
            .Any())
        {
            _logger.LogInformation("Detection Types to Measure Types already exist");
            return;
        }
        var detectionTypes = _detectionTypeRepository.GetList().ToList();
        var measureTypes = _measureTypeRepository.GetList().ToList();
        var measureTypesForBasic = measureTypes.Where(m => new List<int> { 1, 2, 3, 4, 14, 15, 17, 31 }.Contains(m.Id)).ToList();
        var measureTypesForAdvanceCount = measureTypes.Where(m => new List<int> { 6, 7, 8, 9, 13, 32 }.Contains(m.Id)).ToList();
        var measureTypesForAdvanceSpeed = measureTypes.Where(m => new List<int> { 10 }.Contains(m.Id)).ToList();
        var measureTypesForLlc = measureTypes.Where(m => new List<int> { 5, 7, 31, 36 }.Contains(m.Id)).ToList();
        var measureTypesForLls = measureTypes.Where(m => new List<int> { 11 }.Contains(m.Id)).ToList();
        var measureTypesForStopBarPresence = measureTypes.Where(m => new List<int> { 12, 31, 32 }.Contains(m.Id)).ToList();
        foreach (var detectionType in detectionTypes)
        {
            switch (detectionType.Id)
            {
                case DetectionTypes.B:
                    foreach (var measureType in measureTypesForBasic)
                    {
                        detectionType.MeasureTypes.Add(measureType);
                    }
                    break;
                case DetectionTypes.AC:
                    foreach (var measureType in measureTypesForAdvanceCount)
                    {
                        detectionType.MeasureTypes.Add(measureType);
                    }
                    break;
                case DetectionTypes.AS:
                    foreach (var measureType in measureTypesForAdvanceSpeed)
                    {
                        detectionType.MeasureTypes.Add(measureType);
                    }
                    break;
                case DetectionTypes.LLC:
                    foreach (var measureType in measureTypesForLlc)
                    {
                        detectionType.MeasureTypes.Add(measureType);
                    }
                    break;
                case DetectionTypes.LLS:
                    foreach (var measureType in measureTypesForLls)
                    {
                        detectionType.MeasureTypes.Add(measureType);
                    }
                    break;
                case DetectionTypes.SBP:
                    foreach (var measureType in measureTypesForStopBarPresence)
                    {
                        detectionType.MeasureTypes.Add(measureType);
                    }
                    break;
            }
            _detectionTypeRepository.Update(detectionType);
        }
    }

    private void DeleteJurisdictions()
    {
        _logger.LogInformation($"Deleting all jurisdictions");
        try
        {
            var jurisdictions = _jurisdictionRepository.GetList().ToList();
            _jurisdictionRepository.RemoveRange(jurisdictions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, "Error deleting jurisdictions");
            throw;
        }
    }

    private void DeleteAreas()
    {
        _logger.LogInformation($"Deleting all areas");
        try
        {
            var areas = _areaRepository.GetList().ToList();
            _areaRepository.RemoveRange(areas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, "Error deleting areas");
            throw;
        }
    }

    private void DeleteRegions()
    {
        _logger.LogInformation($"Deleting all regions");
        try
        {
            var regions = _regionsRepository.GetList().ToList();
            _regionsRepository.RemoveRange(regions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, "Error deleting regions");
            throw;
        }

    }


    private List<T> ImportData<T>(string query, Dictionary<string, string> columnMappings) where T : new()
    {
        var entities = new List<T>();

        using (var sourceConnection = new SqlConnection(_config.Source))
        {
            sourceConnection.Open();
            using (SqlCommand sourceCommand = new SqlCommand(query, sourceConnection))
            {
                using (SqlDataReader reader = sourceCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var entity = new T(); // Create an instance of the generic type

                        // Iterate through column mappings

                        foreach (var mapping in columnMappings)
                        {
                            var propertyName = mapping.Value; // Name of the property in the class
                            var columnName = mapping.Key;       // Name of the column in the data reader

                            // Get the value from the reader
                            var value = reader[columnName];

                            // Use reflection to set the property value
                            var propertyInfo = entity.GetType().GetProperty(propertyName);
                            if (propertyInfo != null && value != DBNull.Value)
                            {
                                var propertyType = propertyInfo.PropertyType;
                                // Handle nullable types
                                var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

                                // Special handling for enums
                                if (targetType.IsEnum)
                                {
                                    var enumValue = Enum.ToObject(targetType, value);
                                    propertyInfo.SetValue(entity, enumValue);
                                }
                                else if (targetType == typeof(Dictionary<string, object>))
                                {
                                    // Convert the value to string (which should be valid JSON) and deserialize it
                                    string jsonString = Convert.ChangeType(value, typeof(string)) as string;
                                    if (!string.IsNullOrWhiteSpace(jsonString))
                                    {
                                        try
                                        {
                                            var dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);
                                            propertyInfo.SetValue(entity, dictionary);
                                        }
                                        catch (JsonException ex)
                                        {
                                            throw new InvalidOperationException(
                                                $"Failed to deserialize JSON for property '{propertyName}'.", ex);
                                        }
                                    }
                                }
                                else if (targetType == typeof(string[]))
                                {
                                    // Convert the value to string and then deserialize the JSON array
                                    string jsonString = Convert.ChangeType(value, typeof(string)) as string;
                                    if (!string.IsNullOrWhiteSpace(jsonString))
                                    {
                                        try
                                        {
                                            string[] arrayValue = JsonSerializer.Deserialize<string[]>(jsonString);
                                            propertyInfo.SetValue(entity, arrayValue);
                                        }
                                        catch (JsonException ex)
                                        {
                                            throw new InvalidOperationException(
                                                $"Failed to deserialize JSON array for property '{propertyName}'.", ex);
                                        }
                                    }
                                }
                                else if (targetType == typeof(string))
                                {
                                    // For string, simply convert it (optionally, unescape if needed)
                                    string stringValue = Convert.ChangeType(value, typeof(string)) as string;
                                    propertyInfo.SetValue(entity, stringValue);
                                }
                                else
                                {
                                    // Convert and assign other types
                                    propertyInfo.SetValue(entity, Convert.ChangeType(value, targetType));
                                }
                            }
                        }


                        entities.Add(entity);
                    }
                }
            }
        }

        return entities;
    }

    public static Dictionary<string, Dictionary<string, string>> GetColumnMappings(IConfiguration configuration)
    {
        // Retrieve the "ColumnMappings" section from the configuration
        var columnMappingsSection = configuration.GetSection("ColumnMappings");

        if (!columnMappingsSection.Exists())
        {
            throw new KeyNotFoundException("The 'ColumnMappings' section was not found in the configuration.");
        }

        // Create the dictionary to hold the mappings
        var columnMappings = new Dictionary<string, Dictionary<string, string>>();

        foreach (var tableSection in columnMappingsSection.GetChildren())
        {
            var tableName = tableSection.Key;
            var columnMap = new Dictionary<string, string>();

            foreach (var columnSection in tableSection.GetChildren())
            {
                columnMap[columnSection.Key] = columnSection.Value;
            }

            columnMappings[tableName] = columnMap;
        }

        return columnMappings;
    }

    private Dictionary<string, string> GetLocationQueries(IConfiguration config)
    {
        var queries = new Dictionary<string, string>();

        // Get the "LocationQueries" section from the config
        var locationQueriesSection = config.GetSection("LocationQueries");

        if (!locationQueriesSection.Exists())
            throw new InvalidOperationException("The 'LocationQueries' section is missing in the configuration.");

        // Iterate through each key-value pair in the section
        foreach (var child in locationQueriesSection.GetChildren())
        {
            queries[child.Key] = child.Value ?? string.Empty;
        }

        return queries;
    }

    //private void ManuallyAddSpeedDevice()
    //{
    //    productRepository.Add(new Product { Id = 11, Manufacturer = "Wavetronix", Model = "Wavetronix Advance Detection" });
    //    deviceConfigurationRepository.Add(new Device
    //    {
    //        Id = 11,
    //        Description = "None",
    //        Protocol = ATSPM.Table.Enums.TransportProtocols.Unknown,
    //        ConnectionTimeout = 2000,
    //        Directory = "Unkown",
    //        OperationTimout = 2000,
    //        Port = 0,
    //        UserName = "Unknown",
    //        Password = "Unkown",
    //        ProductId = 11
    //    });
    //    deviceRepository.Add(new Device
    //    {
    //         DeviceConfigurationId= 11, DeviceStatus = ATSPM.Table.Enums.DeviceStatus.Active, Ipaddress = "127.0.0.1", 
    //    });
    //}



    private void DeleteLocations()
    {
        _logger.LogInformation($"Deleting all locations");
        try
        {
            var locations = _locationRepository.GetList().ToList();
            _locationRepository.RemoveRange(locations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, "Error deleting locations");
            throw;
        }
    }

    private void DeleteDevices()
    {
        _logger.LogInformation($"Deleting all devices");
        try
        {
            _deviceRepository.RemoveRange(_deviceRepository.GetList().ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, "Error deleting devices");
            throw;
        }
    }


    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

}


public class AreaLocation
{
    public int AreasId { get; set; }
    public int LocationsId { get; set; }
}

public class DetectionTypeDetector
{
    public int DetectionTypesId { get; set; }
    public int DetectorsId { get; set; }
}


