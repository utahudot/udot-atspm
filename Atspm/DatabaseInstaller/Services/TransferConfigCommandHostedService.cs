#region license
// Copyright 2026 Utah Departement of Transportation
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.ConfigurationModels;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;

public class TransferConfigCommandHostedService : IHostedService
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    private readonly ILogger<TransferConfigCommandHostedService> _logger;
    private readonly IJurisdictionRepository _jurisdictionRepository;
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

        SourceConfigData? sourceData = null;
        if (_config.UpdateLocations || _config.ImportSpeedDevices)
        {
            sourceData = await LoadSourceConfigDataAsync(cancellationToken);
        }

        if (_config.UpdateLocations && sourceData is not null)
        {
            SetDetectionTypeMesureType();
            ImportProducts(sourceData.Products);
            ImportDeviceConfigurations(sourceData.DeviceConfigurations);
            ImportRegions(sourceData.Regions);
            ImportAreas(sourceData.Areas);
            ImportJurisdictions(sourceData.Jurisdictions);
            ImportLocations(sourceData.Locations, sourceData.LocationAreaIds);
            ImportApproaches(sourceData.Approaches);
            ImportDetectors(sourceData.Detectors, sourceData.DetectorDetectionTypeIds);
            ImportRoutes(sourceData.Routes);
            ImportRouteLocations(sourceData.RouteLocations);
            ImportDevices(sourceData.Devices);
            ResetSequences();
        }

        if (_config.ImportSpeedDevices && sourceData is not null)
        {
            ImportProducts(sourceData.Products);
            ImportDeviceConfigurations(sourceData.DeviceConfigurations);
            ImportSpeedDevices(sourceData.SpeedDevices);
            ResetSequences();
        }
    }

    private async Task<SourceConfigData> LoadSourceConfigDataAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_config.BearerToken))
        {
            throw new InvalidOperationException("A bearer token is required when transferring configuration from the Config API.");
        }

        using var client = CreateApiClient();

        _logger.LogInformation("Downloading configuration from {ApiBaseUrl}", _config.ApiBaseUrl);

        var products = SanitizeProducts(await GetODataCollectionAsync<Product>(
            client,
            "api/v1/Product",
            cancellationToken));

        var deviceConfigurations = SanitizeDeviceConfigurations(await GetODataCollectionAsync<DeviceConfiguration>(
            client,
            "api/v1/DeviceConfiguration",
            cancellationToken));

        var regions = SanitizeRegions(await GetODataCollectionAsync<Region>(
            client,
            "api/v1/Region",
            cancellationToken));

        var areas = SanitizeAreas(await GetODataCollectionAsync<Area>(
            client,
            "api/v1/Area",
            cancellationToken));

        var jurisdictions = SanitizeJurisdictions(await GetODataCollectionAsync<Jurisdiction>(
            client,
            "api/v1/Jurisdiction",
            cancellationToken));

        var rawLocations = await GetODataCollectionAsync<Location>(
            client,
            "api/v1/Location?$expand=Areas&$orderby=Start desc",
            cancellationToken);

        var latestLocations = rawLocations
            .GroupBy(l => l.LocationIdentifier, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.OrderByDescending(l => l.Start).First())
            .ToList();

        var locationAreaIds = latestLocations.ToDictionary(
            l => l.Id,
            l => l.Areas.Select(a => a.Id).Distinct().ToList());

        var locations = SanitizeLocations(latestLocations);
        var locationIds = locations.Select(l => l.Id).ToHashSet();

        var approaches = SanitizeApproaches((await GetODataCollectionAsync<Approach>(
            client,
            "api/v1/Approach",
            cancellationToken))
            .Where(a => locationIds.Contains(a.LocationId))
            .ToList());

        var approachIds = approaches.Select(a => a.Id).ToHashSet();

        var rawDetectors = (await GetODataCollectionAsync<Detector>(
            client,
            "api/v1/Detector?$expand=DetectionTypes",
            cancellationToken))
            .Where(d => approachIds.Contains(d.ApproachId))
            .ToList();

        var detectorDetectionTypeIds = rawDetectors.ToDictionary(
            d => d.Id,
            d => d.DetectionTypes.Select(dt => dt.Id).Distinct().ToList());

        var detectors = SanitizeDetectors(rawDetectors);

        var devices = SanitizeDevices((await GetODataCollectionAsync<Device>(
            client,
            "api/v1/Device",
            cancellationToken))
            .Where(d => locationIds.Contains(d.LocationId))
            .ToList());

        var routes = SanitizeRoutes(await GetODataCollectionAsync<Route>(
            client,
            "api/v1/Route",
            cancellationToken));

        var routeLocations = SanitizeRouteLocations(await GetODataCollectionAsync<RouteLocation>(
            client,
            "api/v1/RouteLocation",
            cancellationToken));

        var speedConfigurationIds = deviceConfigurations
            .Where(dc =>
                string.Equals(dc.Description, "Speed", StringComparison.OrdinalIgnoreCase) ||
                products.Any(p =>
                    p.Id == dc.ProductId &&
                    string.Equals(p.Manufacturer, "Wavetronix", StringComparison.OrdinalIgnoreCase)))
            .Select(dc => dc.Id)
            .ToHashSet();

        var speedDevices = devices
            .Where(d => d.DeviceConfigurationId.HasValue && speedConfigurationIds.Contains(d.DeviceConfigurationId.Value))
            .ToList();

        return new SourceConfigData
        {
            Products = products,
            DeviceConfigurations = deviceConfigurations,
            Regions = regions,
            Areas = areas,
            Jurisdictions = jurisdictions,
            Locations = locations,
            LocationAreaIds = locationAreaIds,
            Approaches = approaches,
            Detectors = detectors,
            DetectorDetectionTypeIds = detectorDetectionTypeIds,
            Routes = routes,
            RouteLocations = routeLocations,
            Devices = devices,
            SpeedDevices = speedDevices
        };
    }

    private HttpClient CreateApiClient()
    {
        var client = new HttpClient
        {
            BaseAddress = BuildBaseUri(_config.ApiBaseUrl)
        };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config.BearerToken);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }

    private static Uri BuildBaseUri(string apiBaseUrl)
    {
        if (string.IsNullOrWhiteSpace(apiBaseUrl))
        {
            throw new InvalidOperationException("An API base URL is required when transferring configuration from the Config API.");
        }

        return apiBaseUrl.EndsWith("/", StringComparison.Ordinal)
            ? new Uri(apiBaseUrl, UriKind.Absolute)
            : new Uri($"{apiBaseUrl}/", UriKind.Absolute);
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private async Task<List<T>> GetODataCollectionAsync<T>(HttpClient client, string relativeOrAbsoluteUrl, CancellationToken cancellationToken)
    {
        var items = new List<T>();
        var nextUrl = relativeOrAbsoluteUrl;

        while (!string.IsNullOrWhiteSpace(nextUrl))
        {
            using var response = await client.GetAsync(nextUrl, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Request to '{nextUrl}' failed with status {(int)response.StatusCode}: {content}");
            }

            var payload = JsonSerializer.Deserialize<ODataResponse<T>>(content, JsonOptions);
            if (payload?.Value != null)
            {
                items.AddRange(payload.Value);
            }

            nextUrl = payload?.NextLink;
        }

        return items;
    }

    private void ResetSequences()
    {
        using var scope = _serviceProvider.CreateScope();
        var configContext = scope.ServiceProvider.GetRequiredService<ConfigContext>();

        var databaseProvider = configContext.Database.ProviderName;
        if (databaseProvider != "Npgsql.EntityFrameworkCore.PostgreSQL")
        {
            Console.WriteLine("Skipping sequence reset: Database provider is not PostgreSQL.");
            return;
        }

        string[] sequences =
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
            string tableName = sequence.Replace("_Id_seq\"", "").Replace("\"", "");
            string query = $"SELECT setval('public.{sequence}',(SELECT COALESCE(MAX(\"Id\"), 0) FROM public.\"{tableName}\") + 1);";

            configContext.Database.ExecuteSqlRaw(query);
            Console.WriteLine($"Sequence {sequence} reset successfully.");
        }
    }

    private void ImportSpeedDevices(List<Device> devices)
    {
        _logger.LogInformation("Importing Speed Devices");

        if (_config.Delete)
        {
            try
            {
                _deviceRepository.AddRange(devices);
                _logger.LogInformation("Speed Devices Imported");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing speed devices");
                throw;
            }
        }
        else
        {
            var deviceIds = _deviceRepository.GetList().Select(d => d.Id).ToHashSet();
            var newDevices = devices.Where(d => !deviceIds.Contains(d.Id)).ToList();
            foreach (var device in newDevices)
            {
                try
                {
                    _deviceRepository.Add(device);
                    _logger.LogInformation("Speed Device {DeviceIdentifier} Imported", device.DeviceIdentifier);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error importing speed device {DeviceIdentifier}", device.DeviceIdentifier);
                }
            }
        }
    }

    private void DeleteProducts()
    {
        _logger.LogInformation("Deleting all products");
        try
        {
            _productRepository.RemoveRange(_productRepository.GetList().ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting products");
            throw;
        }
    }

    private void DeleteDevicesConfigurations()
    {
        _logger.LogInformation("Deleting all device configurations");
        try
        {
            _deviceConfigurationRepository.RemoveRange(_deviceConfigurationRepository.GetList().ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting device configurations");
            throw;
        }
    }

    private void ImportJurisdictions(List<Jurisdiction> jurisdictions)
    {
        if (_jurisdictionRepository.GetList().Any())
        {
            _logger.LogInformation("Jurisdictions already exist");
            return;
        }

        _logger.LogInformation("Importing Jurisdictions...");
        _jurisdictionRepository.AddRange(jurisdictions);
        _logger.LogInformation("Jurisdictions Imported");
    }

    private void ImportAreas(List<Area> areas)
    {
        if (_areaRepository.GetList().Any())
        {
            _logger.LogInformation("Areas already exist");
            return;
        }

        _logger.LogInformation("Importing Areas...");
        _areaRepository.AddRange(areas);
        _logger.LogInformation("Areas Imported");
    }

    private void ImportRegions(List<Region> regions)
    {
        if (_regionsRepository.GetList().Any())
        {
            _logger.LogInformation("Regions already exist");
            return;
        }

        _logger.LogInformation("Importing Regions...");
        _regionsRepository.AddRange(regions);
        _logger.LogInformation("Regions Imported");
    }

    private void ImportRoutes(List<Route> routes)
    {
        if (_routeRepository.GetList().Any())
        {
            _logger.LogInformation("Routes already exist");
            return;
        }

        _logger.LogInformation("Importing Routes");
        _routeRepository.AddRange(routes);
        _logger.LogInformation("Routes Imported");
    }

    private void ImportRouteLocations(List<RouteLocation> routeLocations)
    {
        if (_routeLocationsRepository.GetList().Any())
        {
            _logger.LogInformation("Route Locations already exist");
            return;
        }

        _logger.LogInformation("Importing Route Locations");
        _routeLocationsRepository.AddRange(routeLocations);
        _logger.LogInformation("Route Locations Imported");
    }

    private void ImportDeviceConfigurations(List<DeviceConfiguration> deviceConfigurations)
    {
        if (_deviceConfigurationRepository.GetList().Any())
        {
            _logger.LogInformation("Device Configurations already exist");
            return;
        }

        _logger.LogInformation("Adding Device Configurations");
        _deviceConfigurationRepository.AddRange(deviceConfigurations);
        _logger.LogInformation("Device Configurations Added");
    }

    private void ImportProducts(List<Product> products)
    {
        if (_productRepository.GetList().Any())
        {
            _logger.LogInformation("Products already exist");
            return;
        }

        _logger.LogInformation("Adding Products");
        _productRepository.AddRange(products);
        _logger.LogInformation("Products Added");
    }

    private void ImportApproaches(List<Approach> approaches)
    {
        if (_config.Delete && _approachRepository.GetList().Any())
        {
            _logger.LogInformation("Approaches already exist");
            return;
        }

        _logger.LogInformation("Importing Approaches");

        if (_config.Delete)
        {
            const int batchSize = 5000;
            int total = approaches.Count;
            int batches = (int)Math.Ceiling(total / (double)batchSize);

            for (int i = 0; i < batches; i++)
            {
                var batch = approaches.Skip(i * batchSize).Take(batchSize).ToList();
                _approachRepository.AddRange(batch);
                _logger.LogInformation("Batch {Batch}/{Batches} imported ({Count} approaches).", i + 1, batches, batch.Count);
            }

            _logger.LogInformation("All Approaches Imported");
        }
        else
        {
            var existingApproachIds = _approachRepository.GetList().Select(a => a.Id).ToHashSet();
            var newApproaches = approaches.Where(a => !existingApproachIds.Contains(a.Id)).ToList();
            foreach (var approach in newApproaches)
            {
                try
                {
                    _approachRepository.Add(approach);
                    _logger.LogInformation("Approach {ApproachId} Imported", approach.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error importing approach {ApproachId}", approach.Id);
                }
            }
        }
    }

    private void ImportDevices(List<Device> devices)
    {
        if (_deviceRepository.GetList().Any())
        {
            _logger.LogInformation("Devices already exist");
            return;
        }

        _logger.LogInformation("Importing Devices");
        _deviceRepository.AddRange(devices);
        _logger.LogInformation("Devices Imported");
    }

    private void ImportDetectors(List<Detector> detectors, Dictionary<int, List<DetectionTypes>> detectorDetectionTypeIds)
    {
        if (_config.Delete && _detectorRepository.GetList().Any())
        {
            _logger.LogInformation("Detectors already exist");
            return;
        }

        _logger.LogInformation("Importing Detectors");

        var detectionTypes = _detectionTypeRepository.GetList().ToList();

        if (_config.Delete)
        {
            const int batchSize = 5000;
            for (int i = 0; i < detectors.Count; i += batchSize)
            {
                var batch = detectors.Skip(i).Take(batchSize).ToList();
                AttachDetectionTypes(batch, detectionTypes, detectorDetectionTypeIds);
                _detectorRepository.AddRange(batch);
                _logger.LogInformation("Processed batch of {Count} detectors", batch.Count);
            }

            _logger.LogInformation("Detectors Imported");
        }
        else
        {
            var existingDetectorIds = _detectorRepository.GetList().Select(d => d.Id).ToHashSet();
            var newDetectors = detectors.Where(d => !existingDetectorIds.Contains(d.Id)).ToList();
            foreach (var detector in newDetectors)
            {
                try
                {
                    AttachDetectionTypes(new List<Detector> { detector }, detectionTypes, detectorDetectionTypeIds);
                    _detectorRepository.Add(detector);
                    _logger.LogInformation("Detector {DetectorIdentifier} Imported", detector.DectectorIdentifier);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error importing detector {DetectorIdentifier}", detector.DectectorIdentifier);
                }
            }
        }
    }

    private static void AttachDetectionTypes(
        List<Detector> detectors,
        List<DetectionType> detectionTypes,
        Dictionary<int, List<DetectionTypes>> detectorDetectionTypeIds)
    {
        foreach (var detectionType in detectionTypes)
        {
            if (detectionType.Id == DetectionTypes.B)
            {
                foreach (var detector in detectors)
                {
                    detectionType.Detectors.Add(detector);
                }

                continue;
            }

            foreach (var detector in detectors.Where(d =>
                         detectorDetectionTypeIds.TryGetValue(d.Id, out var ids) && ids.Contains(detectionType.Id)))
            {
                detectionType.Detectors.Add(detector);
            }
        }
    }

    private void ImportLocations(List<Location> locations, Dictionary<int, List<int>> locationAreaIds)
    {
        if (_config.Delete && _locationRepository.GetList().Any())
        {
            _logger.LogInformation("Locations already exist");
            return;
        }

        _logger.LogInformation("Importing Locations...");

        var areasById = _areaRepository.GetList().ToDictionary(a => a.Id);
        foreach (var location in locations)
        {
            if (locationAreaIds.TryGetValue(location.Id, out var areaIds))
            {
                foreach (var areaId in areaIds.Where(areasById.ContainsKey))
                {
                    location.Areas.Add(areasById[areaId]);
                }
            }
        }

        if (_config.Delete)
        {
            try
            {
                _locationRepository.AddRange(locations);
                _logger.LogInformation("Locations Imported");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing locations");
                throw;
            }
        }
        else
        {
            var existingLocationIds = _locationRepository.GetList().Select(l => l.Id).ToHashSet();
            var newLocations = locations.Where(l => !existingLocationIds.Contains(l.Id)).ToList();
            foreach (var location in newLocations)
            {
                try
                {
                    _locationRepository.Add(location);
                    _logger.LogInformation("Location {LocationIdentifier} Imported", location.LocationIdentifier);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error importing location {LocationIdentifier}", location.LocationIdentifier);
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
        _logger.LogInformation("Deleting all jurisdictions");
        try
        {
            _jurisdictionRepository.RemoveRange(_jurisdictionRepository.GetList().ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting jurisdictions");
            throw;
        }
    }

    private void DeleteAreas()
    {
        _logger.LogInformation("Deleting all areas");
        try
        {
            _areaRepository.RemoveRange(_areaRepository.GetList().ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting areas");
            throw;
        }
    }

    private void DeleteRegions()
    {
        _logger.LogInformation("Deleting all regions");
        try
        {
            _regionsRepository.RemoveRange(_regionsRepository.GetList().ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting regions");
            throw;
        }
    }

    private void DeleteLocations()
    {
        _logger.LogInformation("Deleting all locations");
        try
        {
            _locationRepository.RemoveRange(_locationRepository.GetList().ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting locations");
            throw;
        }
    }

    private void DeleteDevices()
    {
        _logger.LogInformation("Deleting all devices");
        try
        {
            _deviceRepository.RemoveRange(_deviceRepository.GetList().ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting devices");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static List<Product> SanitizeProducts(IEnumerable<Product> products) =>
        products.Select(product => CopyAuditFields(product, new Product
        {
            Id = product.Id,
            Manufacturer = product.Manufacturer,
            Model = product.Model,
            WebPage = product.WebPage,
            Notes = product.Notes
        })).ToList();

    private static List<DeviceConfiguration> SanitizeDeviceConfigurations(IEnumerable<DeviceConfiguration> deviceConfigurations) =>
        deviceConfigurations.Select(dc => CopyAuditFields(dc, new DeviceConfiguration
        {
            Id = dc.Id,
            Description = dc.Description,
            Notes = dc.Notes,
            Protocol = dc.Protocol,
            Port = dc.Port,
            ConnectionProperties = dc.ConnectionProperties,
            Path = dc.Path,
            Query = dc.Query,
            ConnectionTimeout = dc.ConnectionTimeout,
            OperationTimeout = dc.OperationTimeout,
            LoggingOffset = dc.LoggingOffset,
            Decoders = dc.Decoders,
            UserName = dc.UserName,
            Password = dc.Password,
            ProductId = dc.ProductId
        })).ToList();

    private static List<Region> SanitizeRegions(IEnumerable<Region> regions) =>
        regions.Select(region => CopyAuditFields(region, new Region
        {
            Id = region.Id,
            Description = region.Description
        })).ToList();

    private static List<Area> SanitizeAreas(IEnumerable<Area> areas) =>
        areas.Select(area => CopyAuditFields(area, new Area
        {
            Id = area.Id,
            Name = area.Name
        })).ToList();

    private static List<Jurisdiction> SanitizeJurisdictions(IEnumerable<Jurisdiction> jurisdictions) =>
        jurisdictions.Select(jurisdiction => CopyAuditFields(jurisdiction, new Jurisdiction
        {
            Id = jurisdiction.Id,
            Name = jurisdiction.Name,
            Mpo = jurisdiction.Mpo,
            CountyParish = jurisdiction.CountyParish,
            OtherPartners = jurisdiction.OtherPartners
        })).ToList();

    private static List<Location> SanitizeLocations(IEnumerable<Location> locations) =>
        locations.Select(location => CopyAuditFields(location, new Location
        {
            Id = location.Id,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            PrimaryName = location.PrimaryName,
            SecondaryName = location.SecondaryName,
            ChartEnabled = location.ChartEnabled,
            VersionAction = location.VersionAction,
            Note = location.Note,
            Start = location.Start,
            PedsAre1to1 = location.PedsAre1to1,
            LocationIdentifier = location.LocationIdentifier,
            JurisdictionId = location.JurisdictionId,
            LocationTypeId = location.LocationTypeId,
            RegionId = location.RegionId
        })).ToList();

    private static List<Approach> SanitizeApproaches(IEnumerable<Approach> approaches) =>
        approaches.Select(approach => CopyAuditFields(approach, new Approach
        {
            Id = approach.Id,
            Description = approach.Description,
            Mph = approach.Mph,
            ProtectedPhaseNumber = approach.ProtectedPhaseNumber,
            IsProtectedPhaseOverlap = approach.IsProtectedPhaseOverlap,
            PermissivePhaseNumber = approach.PermissivePhaseNumber,
            IsPermissivePhaseOverlap = approach.IsPermissivePhaseOverlap,
            PedestrianPhaseNumber = approach.PedestrianPhaseNumber,
            IsPedestrianPhaseOverlap = approach.IsPedestrianPhaseOverlap,
            PedestrianDetectors = approach.PedestrianDetectors,
            LocationId = approach.LocationId,
            DirectionTypeId = approach.DirectionTypeId
        })).ToList();

    private static List<Detector> SanitizeDetectors(IEnumerable<Detector> detectors) =>
        detectors.Select(detector => CopyAuditFields(detector, new Detector
        {
            Id = detector.Id,
            DectectorIdentifier = detector.DectectorIdentifier,
            DetectorChannel = detector.DetectorChannel,
            DistanceFromStopBar = detector.DistanceFromStopBar,
            MinSpeedFilter = detector.MinSpeedFilter,
            DateAdded = detector.DateAdded,
            DateDisabled = detector.DateDisabled,
            LaneNumber = detector.LaneNumber,
            MovementType = detector.MovementType,
            LaneType = detector.LaneType,
            DetectionHardware = detector.DetectionHardware,
            DecisionPoint = detector.DecisionPoint,
            MovementDelay = detector.MovementDelay,
            LatencyCorrection = detector.LatencyCorrection,
            ApproachId = detector.ApproachId
        })).ToList();

    private static List<Route> SanitizeRoutes(IEnumerable<Route> routes) =>
        routes.Select(route => CopyAuditFields(route, new Route
        {
            Id = route.Id,
            Name = route.Name
        })).ToList();

    private static List<RouteLocation> SanitizeRouteLocations(IEnumerable<RouteLocation> routeLocations) =>
        routeLocations.Select(routeLocation => CopyAuditFields(routeLocation, new RouteLocation
        {
            Id = routeLocation.Id,
            Order = routeLocation.Order,
            PrimaryPhase = routeLocation.PrimaryPhase,
            OpposingPhase = routeLocation.OpposingPhase,
            PrimaryDirectionId = routeLocation.PrimaryDirectionId,
            OpposingDirectionId = routeLocation.OpposingDirectionId,
            IsPrimaryOverlap = routeLocation.IsPrimaryOverlap,
            IsOpposingOverlap = routeLocation.IsOpposingOverlap,
            PreviousLocationDistanceId = routeLocation.PreviousLocationDistanceId,
            NextLocationDistanceId = routeLocation.NextLocationDistanceId,
            LocationIdentifier = routeLocation.LocationIdentifier,
            RouteId = routeLocation.RouteId
        })).ToList();

    private static List<Device> SanitizeDevices(IEnumerable<Device> devices) =>
        devices.Select(device => CopyAuditFields(device, new Device
        {
            Id = device.Id,
            DeviceIdentifier = device.DeviceIdentifier,
            DeviceProperties = device.DeviceProperties,
            LoggingEnabled = device.LoggingEnabled,
            Ipaddress = null,
            DeviceStatus = device.DeviceStatus,
            DeviceType = device.DeviceType,
            Notes = device.Notes,
            LocationId = device.LocationId,
            DeviceConfigurationId = device.DeviceConfigurationId
        })).ToList();

    private static TTarget CopyAuditFields<TKey, TTarget>(AtspmConfigModelBase<TKey> source, TTarget target)
        where TTarget : AtspmConfigModelBase<TKey>
    {
        target.Created = source.Created;
        target.Modified = source.Modified;
        target.CreatedBy = source.CreatedBy;
        target.ModifiedBy = source.ModifiedBy;
        return target;
    }

    private sealed class ODataResponse<T>
    {
        [JsonPropertyName("value")]
        public List<T>? Value { get; set; }

        [JsonPropertyName("@odata.nextLink")]
        public string? NextLink { get; set; }
    }

    private sealed class SourceConfigData
    {
        public List<Product> Products { get; set; } = new();
        public List<DeviceConfiguration> DeviceConfigurations { get; set; } = new();
        public List<Region> Regions { get; set; } = new();
        public List<Area> Areas { get; set; } = new();
        public List<Jurisdiction> Jurisdictions { get; set; } = new();
        public List<Location> Locations { get; set; } = new();
        public Dictionary<int, List<int>> LocationAreaIds { get; set; } = new();
        public List<Approach> Approaches { get; set; } = new();
        public List<Detector> Detectors { get; set; } = new();
        public Dictionary<int, List<DetectionTypes>> DetectorDetectionTypeIds { get; set; } = new();
        public List<Route> Routes { get; set; } = new();
        public List<RouteLocation> RouteLocations { get; set; } = new();
        public List<Device> Devices { get; set; } = new();
        public List<Device> SpeedDevices { get; set; } = new();
    }
}
