#region license
// Copyright 2025 Utah Departement of Transportation
// for DatabaseInstaller - %Namespace%/SetupTestCommandHostedService.cs
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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;

public class SetupTestCommandHostedService : IHostedService
{
    private readonly ILogger<SetupTestCommandHostedService> _logger;
    private readonly IJurisdictionRepository _jurisdictionRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly IDeviceConfigurationRepository _deviceConfigurationRepository;
    private readonly IProductRepository _productRepository;
    private readonly IRegionsRepository _regionsRepository;
    private readonly IAreaRepository _areaRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly TestingConfiguration _config;

    public SetupTestCommandHostedService(
        ILogger<SetupTestCommandHostedService> logger,
        IJurisdictionRepository jurisdictionRepository,
        ILocationRepository locationRepository,
        IDeviceRepository deviceRepository,
        IDeviceConfigurationRepository deviceConfigurationRepository,
        IProductRepository productRepository,
        IRegionsRepository regionsRepository,
        IAreaRepository areaRepository,
        IOptions<TestingConfiguration> config
        )
    {
        _logger = logger;
        _jurisdictionRepository = jurisdictionRepository;
        _locationRepository = locationRepository;
        _deviceConfigurationRepository = deviceConfigurationRepository;
        _productRepository = productRepository;
        _regionsRepository = regionsRepository;
        _areaRepository = areaRepository;
        _deviceRepository = deviceRepository;
        _config = config.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        SeedProducts();
        SeedDeviceConfigurations();
        EnsureDefaultArea();
        EnsureDefaultRegion();
        EnsureDefaultJurisdiction();
        var path = Path.Combine(Directory.GetCurrentDirectory(), "devices.json");
        await SeedEmulatedDevicesFromConfigAsync(path, _config.Protocol);
    }

    private async Task SeedEmulatedDevicesFromConfigAsync(string path, string? protocolFilter)
    {
        // Instead of reading from devices.json, generate devices dynamically
        var devices = new List<DeviceDefinition>();

        // Counts per protocol (from env vars or config)
        int ftpCount = _config.FtpCount;
        var sftpCount = _config.SftpCount;
        var httpCount = _config.HttpCount;

        // FTP
        for (int i = 1; i <= ftpCount; i++)
        {
            devices.Add(new DeviceDefinition
            {
                DeviceIdentifier = $"FTP-{i:D3}",
                Protocol = "ftp",
                IpAddress = _config.FtpIp,
                Port = 21,
                LogDirectory = Path.Combine(_config.FtpLogBase, $"FTP-{i:D3}"),
                UseCompression = false
            });
        }

        // SFTP
        for (int i = 1; i <= sftpCount; i++)
        {
            devices.Add(new DeviceDefinition
            {
                DeviceIdentifier = $"SFTP-{i:D3}",
                Protocol = "sftp",
                IpAddress = _config.SftpIp,
                Port = 22,
                LogDirectory = Path.Combine(_config.SftpLogBase, $"SFTP-{i:D3}"),
                UseCompression = false
            });
        }

        // HTTP
        for (int i = 1; i <= httpCount; i++)
        {
            devices.Add(new DeviceDefinition
            {
                DeviceIdentifier = $"HTTP-{i:D3}",
                Protocol = "http",
                IpAddress = "127.0.0.1",
                Port = 80,
                LogDirectory = Path.Combine(_config.HttpLogBase, $"HTTP-{i:D3}"),
                UseCompression = false
            });
        }

        if (!string.IsNullOrEmpty(protocolFilter))
        {
            devices = devices
                .Where(d => d.Protocol.Equals(protocolFilter, StringComparison.OrdinalIgnoreCase))
                .ToList();

            _logger.LogInformation($"Filtered devices to protocol '{protocolFilter}', count: {devices.Count}");
        }

        if (!devices.Any())
        {
            _logger.LogWarning("No devices generated from configuration.");
            return;
        }

        var defaultRegionId = EnsureDefaultRegion();
        var defaultAreaId = EnsureDefaultArea();
        var defaultJurisdictionId = EnsureDefaultJurisdiction();
        var productLookup = _productRepository.GetList().ToDictionary(p => p.Model, p => p.Id);

        foreach (var device in devices)
        {
            if (_deviceRepository.GetList().Any(d => d.DeviceIdentifier == device.DeviceIdentifier))
                continue;

            var protocolEnum = Enum.TryParse<TransportProtocols>(device.Protocol, true, out var proto) ? proto : TransportProtocols.Ftp;
            var productId = productLookup.First().Value; // Default to first if unknown

            var product = _productRepository.GetList()
                .ToList()
                .FirstOrDefault(p => device.DeviceIdentifier.Contains(p.Model, StringComparison.OrdinalIgnoreCase));

            if (product != null)
            {
                productId = product.Id;
            }
            var Path = "";
            var username = "";

            switch (protocolEnum)
            {
                case TransportProtocols.Ftp:
                    Path = $"{_config.FtpLogBase}/{device.DeviceIdentifier}";
                    username = "ftpuser";
                    break;
                case TransportProtocols.Sftp:
                    Path = $"{_config.SftpLogBase}/{device.DeviceIdentifier}";
                    username = "sftpuser";
                    break;
                case TransportProtocols.Http:
                    Path = $"{_config.HttpLogBase}/{device.DeviceIdentifier}";
                    username = "sftpuser";
                    break;
            }

            var deviceConfig = new DeviceConfiguration
            {
                Description = $"Test config for {device.DeviceIdentifier}",
                Protocol = protocolEnum,
                Port = device.Port,
                Path = Path,
                Query = new[] { "dat", "datZ" },
                ConnectionProperties = new Dictionary<string, object>
                {
                    ["DataConnectionConnectTimeout"] = 5000,
                    ["DataConnectionReadTimeout"] = 15000,
                    ["DataConnectionType"] = "AutoActive",
                    ["AutoConnect"] = true
                },
                ConnectionTimeout = 5000,
                OperationTimeout = 30000,
                LoggingOffset = 0,
                Decoders = new[] { "AscToIndianaDecoder" },
                UserName = username,
                Password = "password",
                ProductId = productId
            };
            _deviceConfigurationRepository.Add(deviceConfig);

            var location = new Location
            {
                LocationIdentifier = device.DeviceIdentifier,
                PrimaryName = device.DeviceIdentifier,
                SecondaryName = device.DeviceIdentifier,
                Latitude = 40.7,
                Longitude = -111.9,
                ChartEnabled = true,
                Note = "Seeded from dynamic config",
                VersionAction = LocationVersionActions.New,
                Start = DateTime.UtcNow,
                PedsAre1to1 = false,
                LocationTypeId = 1,
                JurisdictionId = defaultJurisdictionId,
                RegionId = defaultRegionId,
            };

            var newDevice = new Device
            {
                DeviceIdentifier = device.DeviceIdentifier,
                Ipaddress = device.IpAddress,
                DeviceStatus = DeviceStatus.Active,
                DeviceType = DeviceTypes.SignalController,
                LoggingEnabled = true,
                Notes = "Auto-attached from dynamic config",
                DeviceConfigurationId = deviceConfig.Id,
                DeviceConfiguration = deviceConfig,
            };

            location.Devices.Add(newDevice);
            _locationRepository.Add(location);

            _logger.LogInformation($"Seeded device {device.DeviceIdentifier} at IP {device.IpAddress} using dedicated config {deviceConfig.Description}");
        }

    }

    //private async Task SeedEmulatedDevicesFromConfigAsync(string path, string? protocolFilter)
    //{
    //    if (!File.Exists(path))
    //    {
    //        _logger.LogWarning($"devices.json file not found at path: {path}");
    //        return;
    //    }

    //    var json = await File.ReadAllTextAsync(path);
    //    var devices = JsonSerializer.Deserialize<List<DeviceDefinition>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    //    if (devices == null || devices.Count == 0)
    //    {
    //        _logger.LogWarning("No devices found in devices.json");
    //        return;
    //    }

    //    if (!string.IsNullOrEmpty(protocolFilter))
    //    {
    //        devices = devices
    //            .Where(d => d.Protocol.Equals(protocolFilter, StringComparison.OrdinalIgnoreCase))
    //            .ToList();

    //        _logger.LogInformation($"Filtered devices to protocol '{protocolFilter}', count: {devices.Count}");
    //    }

    //    var defaultRegionId = EnsureDefaultRegion();
    //    var defaultAreaId = EnsureDefaultArea();
    //    var defaultJurisdictionId = EnsureDefaultJurisdiction();
    //    var productLookup = _productRepository.GetList().ToDictionary(p => p.Model, p => p.Id);

    //    foreach (var device in devices)
    //    {
    //        if (_deviceRepository.GetList().Any(d => d.DeviceIdentifier == device.DeviceIdentifier))
    //            continue;

    //        var protocolEnum = Enum.TryParse<TransportProtocols>(device.Protocol, true, out var proto) ? proto : TransportProtocols.Ftp;
    //        var productId = productLookup.First().Value; // Default to first if unknown
    //        var product = _productRepository.GetList()
    //            .ToList() // forces EF to evaluate in memory
    //            .FirstOrDefault(p => device.DeviceIdentifier.Contains(p.Model, StringComparison.OrdinalIgnoreCase));

    //        if (product != null)
    //        {
    //            productId = product.Id;
    //        }

    //        var deviceConfig = new DeviceConfiguration
    //        {
    //            Description = $"Test config for {device.DeviceIdentifier}",
    //            Protocol = protocolEnum,
    //            Port = device.Port,
    //            Path = $"/files/{device.DeviceIdentifier}",
    //            Query = new[] { "dat", "datZ" },
    //            ConnectionProperties = new Dictionary<string, object>
    //            {
    //                ["DataConnectionConnectTimeout"] = 5000,
    //                ["DataConnectionReadTimeout"] = 15000,
    //                ["DataConnectionType"] = "AutoActive",
    //                ["AutoConnect"] = true
    //            },
    //            ConnectionTimeout = 2000,
    //            OperationTimeout = 30000,
    //            LoggingOffset = 0,
    //            Decoders = new[] { "AscToIndianaDecoder" },
    //            UserName = "ftpuser",
    //            Password = "password",
    //            ProductId = productId
    //        };
    //        _deviceConfigurationRepository.Add(deviceConfig);

    //        var location = new Location
    //        {
    //            LocationIdentifier = device.DeviceIdentifier,
    //            PrimaryName = device.DeviceIdentifier,
    //            SecondaryName = device.DeviceIdentifier,
    //            Latitude = 40.7,
    //            Longitude = -111.9,
    //            ChartEnabled = true,
    //            Note = "Seeded from devices.json",
    //            VersionAction = LocationVersionActions.New,
    //            Start = DateTime.UtcNow,
    //            PedsAre1to1 = false,
    //            LocationTypeId = 1,
    //            JurisdictionId = defaultJurisdictionId,
    //            RegionId = defaultRegionId,
    //        };

    //        var newDevice = new Device
    //        {
    //            DeviceIdentifier = device.DeviceIdentifier,
    //            Ipaddress = device.IpAddress,
    //            DeviceStatus = DeviceStatus.Active,
    //            DeviceType = DeviceTypes.SignalController,
    //            LoggingEnabled = true,
    //            Notes = "Auto-attached from devices.json",
    //            DeviceConfigurationId = deviceConfig.Id,
    //            DeviceConfiguration = deviceConfig,
    //        };

    //        location.Devices.Add(newDevice);
    //        _locationRepository.Add(location);

    //        _logger.LogInformation($"Seeded device {device.DeviceIdentifier} at IP {device.IpAddress} using dedicated config {deviceConfig.Description}");
    //    }
    //}

    private int EnsureDefaultJurisdiction()
    {
        var existing = _jurisdictionRepository.GetList().FirstOrDefault(j => j.Name == "Default Jurisdiction");
        if (existing != null)
            return existing.Id;

        var jurisdiction = new Jurisdiction
        {
            Name = "Default Jurisdiction"
        };

        _jurisdictionRepository.Add(jurisdiction);
        return jurisdiction.Id;
    }

    private int EnsureDefaultRegion()
    {
        var existing = _regionsRepository.GetList().FirstOrDefault(r => r.Description == "Default Region");
        if (existing != null)
            return existing.Id;

        var region = new Region
        {
            Description = "Default Region"
        };

        _regionsRepository.Add(region);
        return region.Id;
    }

    private int EnsureDefaultArea()
    {
        var existing = _areaRepository.GetList().FirstOrDefault(a => a.Name == "Default Area");
        if (existing != null)
            return existing.Id;

        var area = new Area
        {
            Name = "Default Area"
        };

        _areaRepository.Add(area);
        return area.Id;
    }


    private void SeedProducts()
    {
        var products = new List<Product>
    {
        new() { Manufacturer = "Econolite", Model = "ASC-3" },
        new() { Manufacturer = "Econolite", Model = "Cobalt", WebPage = "https://www.econolite.com/solutions/traffic-signal-controller/cobalt-traffic-signal-controller/" },
        new() { Manufacturer = "QFree", Model = "MaxTime ic", WebPage = "https://www.q-free.com/product/intelight-maxtime-ic/" },
        new() { Manufacturer = "QFree", Model = "MaxTime rm", WebPage = "https://www.q-free.com/product/intelight-maxtime-rm/" },
    };

        foreach (var product in products)
        {
            if (!_productRepository.GetList().Any(p => p.Manufacturer == product.Manufacturer && p.Model == product.Model))
            {
                _productRepository.Add(product);
            }
        }
    }

    private void SeedDeviceConfigurations()
    {
        var productLookup = _productRepository.GetList().ToDictionary(p => p.Model, p => p.Id);

        var configs = new List<DeviceConfiguration>
    {
        new()
        {
            Description = ">02.47",
            Protocol = TransportProtocols.Ftp,
            Port = 21,
            Path = "//Set1",
            Query = new[] { "dat", "datZ" },
            ConnectionProperties = new Dictionary<string, object>
            {
                ["DataConnectionConnectTimeout"] = 5000,
                ["DataConnectionReadTimeout"] = 15000,
                ["DataConnectionType"] = "AutoActive",
                ["AutoConnect"] = true
            },
            ConnectionTimeout = 2000,
            OperationTimeout = 30000,
            LoggingOffset = 0,
            Decoders = new[] { "Asc To Indiana Decoder" },
            UserName = "test",
            Password = "test",
            ProductId = productLookup["ASC-3"]
        },
        new()
        {
            Description = ">32.xx.xx but < 32.67.20",
            Protocol = TransportProtocols.Ftp,
            Port = 21,
            Path = "/set1",
            Query = new[] { "dat", "datZ" },
            ConnectionProperties = new Dictionary<string, object>
            {
                ["DataConnectionConnectTimeout"] = 5000,
                ["DataConnectionReadTimeout"] = 15000,
                ["DataConnectionType"] = "AutoActive",
                ["AutoConnect"] = true
            },
            ConnectionTimeout = 2000,
            OperationTimeout = 3000,
            LoggingOffset = 0,
            Decoders = new[] { "Asc To Indiana Decoder" },
            UserName = "test",
            Password = "test",
            ProductId = productLookup["Cobalt"]
        },
        new()
        {
            Description = "1.9.x or 2.x.x",
            Protocol = TransportProtocols.Http,
            Port = 80,
            Path = "v1/asclog/xml/full",
            Query = new[] { "?since=[LogStartTime:MM-dd-yyyy HH:mm:ss.f]" },
            ConnectionProperties = new Dictionary<string, object>
            {
                ["Accept"] = "application/xml"
            },
            ConnectionTimeout = 5000,
            OperationTimeout = 60000,
            LoggingOffset = 30,
            Decoders = new[] { "Maxtime To Indiana Decoder" },
            UserName = "test",
            Password = "test",
            ProductId = productLookup["MaxTime ic"]
        },
        new()
        {
            Description = ">= 32.67.20 < 32.68.20",
            Protocol = TransportProtocols.Sftp,
            Port = 22,
            Path = "/opt/admin/set1",
            Query = new[] { "dat", "datZ" },
            ConnectionProperties = new Dictionary<string, object>
            {
                ["DataConnectionConnectTimeout"] = 5000,
                ["DataConnectionReadTimeout"] = 15000,
                ["DataConnectionType"] = "AutoActive",
                ["AutoConnect"] = true
            },
            ConnectionTimeout = 2000,
            OperationTimeout = 30000,
            LoggingOffset = 0,
            Decoders = new[] { "Asc To Indiana Decoder" },
            UserName = "test",
            Password = "test",
            ProductId = productLookup["Cobalt"]
        },
        new()
        {
            Description = ">= 32.68.40",
            Protocol = TransportProtocols.Sftp,
            Port = 22,
            Path = "/opt/admin/set1",
            Query = new[] { "dat", "datZ" },
            ConnectionProperties = new Dictionary<string, object>
            {
                ["DataConnectionConnectTimeout"] = 5000,
                ["DataConnectionReadTimeout"] = 15000,
                ["DataConnectionType"] = "AutoActive",
                ["AutoConnect"] = true
            },
            ConnectionTimeout = 2000,
            OperationTimeout = 30000,
            LoggingOffset = 0,
            Decoders = new[] { "Asc To Indiana Decoder" },
            UserName = "test",
            Password = "test",
            ProductId = productLookup["Cobalt"]
        },
        new()
        {
            Description = "2.x.x",
            Protocol = TransportProtocols.Http,
            Port = 80,
            Path = "maxtime-rampmeter/v1/rmclog/xml/full",
            Query = new[] { "?since=[LogStartTime:MM-dd-yyyy HH:mm:ss.f]" },
            ConnectionProperties = new Dictionary<string, object>
            {
                ["Accept"] = "application/xml"
            },
            ConnectionTimeout = 5000,
            OperationTimeout = 60000,
            LoggingOffset = 30,
            Decoders = new[] { "Maxtime To Indiana Decoder" },
            UserName = "test",
            Password = "test",
            ProductId = productLookup["MaxTime rm"]
        }
    };

        foreach (var config in configs)
        {
            if (!_deviceConfigurationRepository.GetList().Any(c =>
                    c.Description == config.Description && c.ProductId == config.ProductId))
            {
                _deviceConfigurationRepository.Add(config);
            }
        }
    }


    private void SeedLocationsWithDevices(int locationCount, int deviceConfigurationId)
    {
        var configuration = _deviceConfigurationRepository.Lookup(deviceConfigurationId);
        if (configuration == null)
        {
            _logger.LogWarning($"DeviceConfiguration with ID {deviceConfigurationId} was not found.");
            return;
        }

        for (int i = 1; i <= locationCount; i++)
        {
            var locationIdentifier = $"LOC-{i:D4}";

            if (_locationRepository.GetList().Any(l => l.LocationIdentifier == locationIdentifier))
                continue;

            var location = new Location
            {
                LocationIdentifier = locationIdentifier,
                PrimaryName = $"Test Primary {i}",
                SecondaryName = $"Test Secondary {i}",
                Latitude = 40.7 + (i * 0.001),
                Longitude = -111.9 - (i * 0.001),
                ChartEnabled = true,
                Note = $"Auto-generated location {i}",
                VersionAction = LocationVersionActions.New,
                Start = DateTime.UtcNow,
                PedsAre1to1 = false,
                LocationTypeId = 1, // Default ID, adjust if needed 
                JurisdictionId = 1,
                RegionId = 1,
            };

            var device = new Device
            {
                DeviceIdentifier = $"DEV-{i:D4}",
                Ipaddress = "127.0.0.1",
                DeviceStatus = DeviceStatus.Active,
                DeviceType = DeviceTypes.SignalController,
                LoggingEnabled = true,
                Notes = $"Auto-attached to {locationIdentifier}",
                DeviceConfigurationId = deviceConfigurationId,
                DeviceConfiguration = configuration,
            };

            location.Devices.Add(device);

            _locationRepository.Add(location);
        }

        _logger.LogInformation($"Seeded {locationCount} locations, each with a device using configuration ID {deviceConfigurationId}");
    }





    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private class DeviceDefinition
    {
        public string DeviceIdentifier { get; set; } = string.Empty;
        public string Protocol { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public int Port { get; set; }
        public string LogDirectory { get; set; } = string.Empty;
        public bool UseCompression { get; set; }
    }

}


