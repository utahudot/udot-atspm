using DatabaseInstaller.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System.CommandLine;
using System.Reflection;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.ConfigurationModels;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Xunit;

namespace DatabaseInstallerTests;

public class TransferConfigUpdateSpeedTests
{
    [Fact]
    public void ApiBaseUrlOption_DefaultsToConfigApiRoute()
    {
        var command = new TransferConfigCommand();

        var parseResult = command.Parse(Array.Empty<string>());

        Assert.Equal(
            "https://atspm.udot.utah.gov/config/",
            parseResult.GetValueForOption(command.ApiBaseUrlOption));
    }

    [Fact]
    public void UpdateSpeedOption_ParsesTrue()
    {
        var command = new TransferConfigCommand();

        var parseResult = command.Parse(new[] { "--update-speed", "true" });

        Assert.True(parseResult.GetValueForOption(command.ImportSpeedDevicesOption));
    }

    [Fact]
    public async Task StartAsync_UpdateSpeedWithoutBearerToken_Throws()
    {
        var service = CreateHostedService(new TransferConfigCommandConfiguration
        {
            ImportSpeedDevices = true,
            ApiBaseUrl = "https://example.test/"
        });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.StartAsync(CancellationToken.None));

        Assert.Equal(
            "A bearer token is required when transferring configuration from the Config API.",
            exception.Message);
    }

    [Fact]
    public async Task StartAsync_DeleteSpeedWithoutBearerToken_DoesNotDeleteDevices()
    {
        var deviceRepository = new Mock<IDeviceRepository>();
        var service = CreateHostedService(
            new TransferConfigCommandConfiguration
            {
                ImportSpeedDevices = true,
                Delete = true,
                ApiBaseUrl = "https://example.test/"
            },
            deviceRepository.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.StartAsync(CancellationToken.None));

        deviceRepository.Verify(
            repository => repository.RemoveRange(It.IsAny<IEnumerable<Device>>()),
            Times.Never);
    }

    [Fact]
    public async Task StartAsync_WithoutUpdateFlags_DoesNotRequireBearerToken()
    {
        var deviceRepository = new Mock<IDeviceRepository>();
        var service = CreateHostedService(
            new TransferConfigCommandConfiguration(),
            deviceRepository.Object);

        await service.StartAsync(CancellationToken.None);

        deviceRepository.VerifyNoOtherCalls();
    }

    [Fact]
    public void IdentifySpeedDevices_IncludesSpeedConfigurationAndWavetronixProduct()
    {
        var products = new List<Product>
        {
            new() { Id = 10, Manufacturer = "wAvEtRoNiX" },
            new() { Id = 11, Manufacturer = "Other" }
        };
        var configurations = new List<DeviceConfiguration>
        {
            new() { Id = 100, Description = "sPeEd", ProductId = 11 },
            new() { Id = 101, Description = "Radar", ProductId = 10 },
            new() { Id = 102, Description = "Controller", ProductId = 11 }
        };
        var devices = new List<Device>
        {
            new() { Id = 1, DeviceConfigurationId = 100 },
            new() { Id = 2, DeviceConfigurationId = 101 },
            new() { Id = 3, DeviceConfigurationId = 102 },
            new() { Id = 4, DeviceConfigurationId = null }
        };

        var method = typeof(TransferConfigCommandHostedService).GetMethod(
            "IdentifySpeedDevices",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        var result = Assert.IsType<List<Device>>(
            method.Invoke(null, new object[] { devices, configurations, products }));

        Assert.Equal(new[] { 1, 2 }, result.Select(device => device.Id));
    }

    [Fact]
    public void EnsureSpeedDependencies_AddsMissingReferencedProductAndConfiguration()
    {
        var product = new Product { Id = 10, Manufacturer = "Wavetronix", Model = "SmartSensor" };
        var configuration = new DeviceConfiguration
        {
            Id = 100,
            Description = "Speed",
            ProductId = product.Id
        };
        var devices = new[]
        {
            new Device
            {
                Id = 1,
                DeviceIdentifier = "5358",
                DeviceConfigurationId = configuration.Id
            },
            new Device
            {
                Id = 2,
                DeviceIdentifier = "5359",
                DeviceConfigurationId = configuration.Id
            }
        };
        var productRepository = new Mock<IProductRepository>();
        productRepository
            .Setup(repository => repository.GetList())
            .Returns(new[] { new Product { Id = 11, Manufacturer = "Other", Model = "Other" } }.AsQueryable());
        var configurationRepository = new Mock<IDeviceConfigurationRepository>();
        configurationRepository
            .Setup(repository => repository.GetList())
            .Returns(new[] { new DeviceConfiguration { Id = 101, Description = "Controller", ProductId = 11 } }.AsQueryable());
        var service = CreateHostedService(
            new TransferConfigCommandConfiguration { ImportSpeedDevices = true },
            productRepository: productRepository.Object,
            deviceConfigurationRepository: configurationRepository.Object);

        var result = InvokeEnsureSpeedDependencies(
            service,
            devices,
            new[] { configuration },
            new[] { product });

        Assert.Equal(devices, result);
        productRepository.Verify(
            repository => repository.Add(It.Is<Product>(item => item.Id == product.Id)),
            Times.Once);
        configurationRepository.Verify(
            repository => repository.Add(It.Is<DeviceConfiguration>(item => item.Id == configuration.Id)),
            Times.Once);
    }

    [Fact]
    public void EnsureSpeedDependencies_MissingSourceProduct_Throws()
    {
        var configuration = new DeviceConfiguration
        {
            Id = 100,
            Description = "Speed",
            ProductId = 10
        };
        var device = new Device
        {
            Id = 1,
            DeviceIdentifier = "5358",
            DeviceConfigurationId = configuration.Id
        };
        var productRepository = new Mock<IProductRepository>();
        productRepository.Setup(repository => repository.GetList()).Returns(Array.Empty<Product>().AsQueryable());
        var configurationRepository = new Mock<IDeviceConfigurationRepository>();
        configurationRepository
            .Setup(repository => repository.GetList())
            .Returns(Array.Empty<DeviceConfiguration>().AsQueryable());
        var service = CreateHostedService(
            new TransferConfigCommandConfiguration { ImportSpeedDevices = true },
            productRepository: productRepository.Object,
            deviceConfigurationRepository: configurationRepository.Object);

        var exception = Assert.Throws<TargetInvocationException>(() => InvokeEnsureSpeedDependencies(
            service,
            new[] { device },
            new[] { configuration },
            Array.Empty<Product>()));

        Assert.IsType<InvalidOperationException>(exception.InnerException);
        productRepository.Verify(repository => repository.Add(It.IsAny<Product>()), Times.Never);
        configurationRepository.Verify(
            repository => repository.Add(It.IsAny<DeviceConfiguration>()),
            Times.Never);
    }

    [Fact]
    public void EnsureSpeedDependencies_ExistingIds_DoesNotCompareOrAddDependencies()
    {
        var sourceProduct = new Product { Id = 10, Manufacturer = "Wavetronix", Model = "SmartSensor" };
        var configuration = new DeviceConfiguration
        {
            Id = 100,
            Description = "Speed",
            ProductId = sourceProduct.Id
        };
        var device = new Device
        {
            Id = 1,
            DeviceIdentifier = "5358",
            DeviceConfigurationId = configuration.Id
        };
        var productRepository = new Mock<IProductRepository>();
        productRepository
            .Setup(repository => repository.GetList())
            .Returns(new[] { new Product { Id = 10, Manufacturer = "Other", Model = "Different" } }.AsQueryable());
        var configurationRepository = new Mock<IDeviceConfigurationRepository>();
        configurationRepository
            .Setup(repository => repository.GetList())
            .Returns(new[] { new DeviceConfiguration { Id = 100, Description = "Existing", ProductId = 99 } }.AsQueryable());
        var service = CreateHostedService(
            new TransferConfigCommandConfiguration { ImportSpeedDevices = true },
            productRepository: productRepository.Object,
            deviceConfigurationRepository: configurationRepository.Object);

        var result = InvokeEnsureSpeedDependencies(
            service,
            new[] { device },
            new[] { configuration },
            new[] { sourceProduct });

        Assert.Same(device, Assert.Single(result));
        productRepository.Verify(
            repository => repository.Add(It.IsAny<Product>()),
            Times.Never);
        configurationRepository.Verify(
            repository => repository.Add(It.IsAny<DeviceConfiguration>()),
            Times.Never);
    }

    [Fact]
    public void FindNewSpeedDevices_MatchesExistingSpeedDeviceByLocationOrIdentifier()
    {
        var existingDevices = new List<Device>
        {
            new()
            {
                Id = 1,
                DeviceIdentifier = "5358",
                LocationId = 100,
                DeviceType = DeviceTypes.SpeedSensor
            },
            new()
            {
                Id = 2,
                DeviceIdentifier = "controller",
                LocationId = 200,
                DeviceType = DeviceTypes.SignalController
            }
        };
        var sourceDevices = new List<Device>
        {
            new() { Id = 90, DeviceIdentifier = "different", LocationId = 100 },
            new() { Id = 91, DeviceIdentifier = " 5358 ", LocationId = 900 },
            new() { Id = 2, DeviceIdentifier = "id-collision", LocationId = 300 },
            new() { Id = 92, DeviceIdentifier = "6000", LocationId = 200 },
            new() { Id = 93, DeviceIdentifier = "7000", LocationId = 300 },
            new() { Id = 94, DeviceIdentifier = "7000", LocationId = 400 }
        };

        var method = typeof(TransferConfigCommandHostedService).GetMethod(
            "FindNewSpeedDevices",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        var result = Assert.IsType<List<Device>>(
            method.Invoke(null, new object[] { sourceDevices, existingDevices }));

        Assert.Equal(new[] { 92, 93 }, result.Select(device => device.Id));
        Assert.Equal(1, existingDevices[0].Id);
    }

    [Fact]
    public void MapSpeedDevicesToActiveLocations_UsesCurrentTargetVersion()
    {
        var sourceDevices = new List<Device>
        {
            new() { Id = 50, DeviceIdentifier = "5358", LocationId = 10 },
            new() { Id = 51, DeviceIdentifier = "missing-target", LocationId = 20 },
            new() { Id = 52, DeviceIdentifier = "missing-source", LocationId = 999 }
        };
        var sourceLocations = new List<Location>
        {
            new() { Id = 10, LocationIdentifier = "5358", Start = new DateTime(2020, 1, 1) },
            new() { Id = 11, LocationIdentifier = "5358", Start = new DateTime(2025, 1, 1) },
            new() { Id = 20, LocationIdentifier = "6000", Start = new DateTime(2025, 1, 1) }
        };
        var activeTargetLocations = new List<Location>
        {
            new() { Id = 900, LocationIdentifier = "5358", Start = new DateTime(2024, 1, 1) },
            new() { Id = 901, LocationIdentifier = "5358", Start = new DateTime(2026, 1, 1) }
        };

        var method = typeof(TransferConfigCommandHostedService).GetMethod(
            "MapSpeedDevicesToActiveLocations",
            BindingFlags.NonPublic | BindingFlags.Static)!;

        var result = Assert.IsType<List<Device>>(
            method.Invoke(null, new object[] { sourceDevices, sourceLocations, activeTargetLocations }));

        var mappedDevice = Assert.Single(result);
        Assert.Equal(50, mappedDevice.Id);
        Assert.Equal(901, mappedDevice.LocationId);
    }

    [Fact]
    public void ImportSpeedDevices_WithoutDelete_AddsOnlyNewSpeedDevices()
    {
        var existingDevice = new Device
        {
            Id = 1,
            DeviceIdentifier = "5358",
            LocationId = 100,
            DeviceType = DeviceTypes.SpeedSensor
        };
        var sameDeviceFromSource = new Device
        {
            Id = 50,
            DeviceIdentifier = "5358",
            LocationId = 100,
            DeviceType = DeviceTypes.SpeedSensor
        };
        var newDevice = new Device
        {
            Id = 51,
            DeviceIdentifier = "6000",
            LocationId = 200,
            DeviceType = DeviceTypes.SpeedSensor
        };
        var deviceRepository = new Mock<IDeviceRepository>();
        deviceRepository
            .Setup(repository => repository.GetList())
            .Returns(new[] { existingDevice }.AsQueryable());

        var service = CreateHostedService(
            new TransferConfigCommandConfiguration { ImportSpeedDevices = true },
            deviceRepository.Object);

        InvokeImportSpeedDevices(service, new List<Device> { sameDeviceFromSource, newDevice });

        deviceRepository.Verify(
            repository => repository.Add(It.Is<Device>(device => device.Id == newDevice.Id)),
            Times.Once);
        deviceRepository.Verify(
            repository => repository.Add(It.Is<Device>(device => device.Id == sameDeviceFromSource.Id)),
            Times.Never);
        deviceRepository.Verify(
            repository => repository.AddRange(It.IsAny<IEnumerable<Device>>()),
            Times.Never);
    }

    [Fact]
    public void ImportSpeedDevices_WithDelete_AddsAllDevicesAsRange()
    {
        var devices = new List<Device>
        {
            new() { Id = 1, DeviceIdentifier = "first" },
            new() { Id = 2, DeviceIdentifier = "second" }
        };
        var deviceRepository = new Mock<IDeviceRepository>();
        var service = CreateHostedService(
            new TransferConfigCommandConfiguration
            {
                ImportSpeedDevices = true,
                Delete = true
            },
            deviceRepository.Object);

        InvokeImportSpeedDevices(service, devices);

        deviceRepository.Verify(
            repository => repository.AddRange(It.Is<IEnumerable<Device>>(
                imported => imported.Select(device => device.Id).SequenceEqual(new[] { 1, 2 }))),
            Times.Once);
        deviceRepository.Verify(
            repository => repository.Add(It.IsAny<Device>()),
            Times.Never);
    }

    [Fact]
    public void DeleteSpeedDevices_RemovesOnlySpeedSensors()
    {
        var speedDevice = new Device
        {
            Id = 1,
            DeviceIdentifier = "speed",
            DeviceType = DeviceTypes.SpeedSensor
        };
        var controller = new Device
        {
            Id = 2,
            DeviceIdentifier = "controller",
            DeviceType = DeviceTypes.SignalController
        };
        var deviceRepository = new Mock<IDeviceRepository>();
        deviceRepository
            .Setup(repository => repository.GetList())
            .Returns(new[] { speedDevice, controller }.AsQueryable());
        var service = CreateHostedService(
            new TransferConfigCommandConfiguration
            {
                ImportSpeedDevices = true,
                Delete = true
            },
            deviceRepository.Object);

        InvokeDeleteSpeedDevices(service);

        deviceRepository.Verify(
            repository => repository.RemoveRange(It.Is<IEnumerable<Device>>(
                removed => removed.Select(device => device.Id).SequenceEqual(new[] { speedDevice.Id }))),
            Times.Once);
    }

    private static TransferConfigCommandHostedService CreateHostedService(
        TransferConfigCommandConfiguration configuration,
        IDeviceRepository? deviceRepository = null,
        IProductRepository? productRepository = null,
        IDeviceConfigurationRepository? deviceConfigurationRepository = null)
    {
        return new TransferConfigCommandHostedService(
            NullLogger<TransferConfigCommandHostedService>.Instance,
            Mock.Of<IJurisdictionRepository>(),
            Mock.Of<ILocationTypeRepository>(),
            Mock.Of<ILocationRepository>(),
            Mock.Of<IApproachRepository>(),
            Mock.Of<IDetectorRepository>(),
            deviceRepository ?? Mock.Of<IDeviceRepository>(),
            deviceConfigurationRepository ?? Mock.Of<IDeviceConfigurationRepository>(),
            productRepository ?? Mock.Of<IProductRepository>(),
            Mock.Of<IRegionsRepository>(),
            Mock.Of<IAreaRepository>(),
            Mock.Of<IDetectionTypeRepository>(),
            Mock.Of<IMeasureTypeRepository>(),
            Mock.Of<IRouteRepository>(),
            Mock.Of<IRouteLocationsRepository>(),
            Options.Create(configuration),
            new ServiceCollection().BuildServiceProvider());
    }

    private static List<Device> InvokeEnsureSpeedDependencies(
        TransferConfigCommandHostedService service,
        IEnumerable<Device> devices,
        IEnumerable<DeviceConfiguration> configurations,
        IEnumerable<Product> products)
    {
        var method = typeof(TransferConfigCommandHostedService).GetMethod(
            "EnsureSpeedDependencies",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        return Assert.IsType<List<Device>>(
            method.Invoke(service, new object[] { devices, configurations, products }));
    }

    private static void InvokeImportSpeedDevices(
        TransferConfigCommandHostedService service,
        List<Device> devices)
    {
        var method = typeof(TransferConfigCommandHostedService).GetMethod(
            "ImportSpeedDevices",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        method.Invoke(service, new object[] { devices });
    }

    private static void InvokeDeleteSpeedDevices(TransferConfigCommandHostedService service)
    {
        var method = typeof(TransferConfigCommandHostedService).GetMethod(
            "DeleteSpeedDevices",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        method.Invoke(service, null);
    }
}
