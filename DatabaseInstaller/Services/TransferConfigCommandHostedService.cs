
using DatabaseInstaller.Commands;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Npgsql.Internal;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Org.BouncyCastle.Math.EC.ECCurve;

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
        IOptions<TransferConfigCommandConfiguration> config
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
        if(_config.UpdateLocations)
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();


            Dictionary<string, string> queries = GetLocationQueries(config);
            //Dictionary<string, Dictionary<string, string>> columnMappings = GetColumnMappings();
            var columnMappings = GetColumnMappings(config);

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
        if(_regionsRepository.GetList().Any())
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
        if (_approachRepository.GetList().Any())
        {
            _logger.LogInformation("Approaches already exist");
            return;
        }

        _logger.LogInformation($"Importing Approaches");

        // Import all approaches at once
        var approaches = ImportData<Approach>(queries["Approaches"], columnMappings["Approaches"]);

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
        if (_detectorRepository.GetList().Any())
        {
            _logger.LogInformation("Detectors already exist");
            return;
        }
        _logger.LogInformation($"Importing Detectors");

        var detectionTypes = _detectionTypeRepository.GetList().ToList();

        var detectionTypeDetectors = ImportData<DetectionTypeDetector>(queries["DetectionTypeDetector"], columnMappings["DetectionTypeDetector"]);

        var detectors = ImportData<Detector>(queries["Detectors"], columnMappings["Detectors"]);

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

    private void ImportLocations(Dictionary<string, string> queries, Dictionary<string, Dictionary<string, string>> columnMappings)
    {
        if (_locationRepository.GetList().Any())
        {
            _logger.LogInformation("Locations already exist");
            return;
        }
        _logger.LogInformation($"Importing Locations...");

        var areas = _areaRepository.GetList().ToList();

        var locationAreas = ImportData<AreaLocation>(queries["AreaLocations"], columnMappings["AreaLocations"]);

        var locations = ImportData<Location>(queries["Locations"], columnMappings["Locations"]);

        foreach(var area in areas)
        {
            var locationIds = locationAreas.Where(l => l.AreasId == area.Id).Select(l => l.LocationsId).ToList();
            foreach (var location in locations.Where(l => locationIds.Contains(l.Id)))
            {
                location.Areas.Add(area);
            }
        }

        _locationRepository.AddRange(locations);
        _logger.LogInformation($"Locations Imported");
    }

    private void SetDetectionTypeMesureType()
    {
        if(_detectionTypeRepository.GetList()
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
        var measureTypesForAdvanceCount = measureTypes.Where(m => new List<int> { 6,7,8,9,13,32 }.Contains(m.Id)).ToList();
        var measureTypesForAdvanceSpeed = measureTypes.Where(m => new List<int> { 10 }.Contains(m.Id)).ToList();
        var measureTypesForLlc = measureTypes.Where(m => new List<int> { 5,7,31,36 }.Contains(m.Id)).ToList();
        var measureTypesForLls = measureTypes.Where(m => new List<int> { 11 }.Contains(m.Id)).ToList();
        var measureTypesForStopBarPresence = measureTypes.Where(m => new List<int> { 12,31,32 }.Contains(m.Id)).ToList();
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
                            var columnName = mapping.Key;    // Name of the column in the data reader

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
    //    deviceConfigurationRepository.Add(new DeviceConfiguration
    //    {
    //        Id = 11,
    //        Firmware = "None",
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

