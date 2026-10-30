using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Moq;
using Utah.Udot.Atspm.ConfigApi.Controllers;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Services;

namespace ConfigApiTests;

[TestClass]
public class DeviceZoneProviderTests
{
    private sealed class ZoneHandler : HttpMessageHandler
    {
        public string Path { get; private set; } = "";
        public string Body { get; set; } = "{\"zones\":[{\"name\":\"NB shared\"}]}";
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Path = request.RequestUri!.AbsolutePath;
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent(Body) });
        }
    }

    [TestMethod]
    public async Task VisionProviderUsesSavedPathAndIdentifierAndRejectsInvalidInventory()
    {
        var handler = new ZoneHandler();
        using var client = new HttpClient(handler);
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("DeviceZones")).Returns(client);
        var provider = new Utah.Udot.Atspm.Infrastructure.Services.DeviceZones.VisionDeviceZoneProvider(factory.Object);
        var device = new Device
        {
            DeviceType = Utah.Udot.Atspm.Data.Enums.DeviceTypes.AICamera,
            Ipaddress = "10.10.10.26", DeviceIdentifier = "123-bins",
            DeviceConfiguration = new DeviceConfiguration
            {
                Port = 80, Path = "/custom/cameras/[Device:DeviceIdentifier]/bin-statistics",
                Product = new Product { Manufacturer = "Econolite", Model = "Vision" }
            }
        };
        Assert.IsTrue(provider.CanRead(device));
        var zones = await provider.ReadZonesAsync(device, CancellationToken.None);
        Assert.AreEqual("/custom/cameras/123/zones", handler.Path);
        Assert.AreEqual("NB shared", zones.Single());
        handler.Body = "{\"zones\":[{\"name\":null}]}";
        await Assert.ThrowsExceptionAsync<InvalidDataException>(() => provider.ReadZonesAsync(device, CancellationToken.None));
        device.DeviceConfiguration.Product.Manufacturer = "Other vendor";
        Assert.IsFalse(provider.CanRead(device));
    }

    [TestMethod]
    public async Task ConfigApiSelectsAnotherDeviceProviderWithoutReportDecoder()
    {
        var device = new Device { Id = 42, DeviceIdentifier = "radar" };
        var repository = new Mock<IDeviceRepository>();
        repository.Setup(r => r.GetList()).Returns(new[] { device }.AsQueryable());
        var provider = new Mock<IDeviceZoneProvider>();
        provider.Setup(p => p.CanRead(device)).Returns(true);
        provider.Setup(p => p.ReadZonesAsync(device, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { "Radar zone A", "Radar zone B" });
        var controller = new DetectorController(Mock.Of<IDetectorRepository>(), repository.Object, new[] { provider.Object })
        { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } };
        var result = await controller.RetrieveDetectionZoneIdentifierBasedOnDetectionType(
            new ODataActionParameters { ["configurationDeviceId"] = 42 });
        CollectionAssert.AreEqual(new[] { "Radar zone A", "Radar zone B" },
            ((IReadOnlyList<string>)((OkObjectResult)result).Value!).ToArray());
    }

    [DataTestMethod]
    [DataRow(0)]
    [DataRow(2)]
    public async Task MissingOrAmbiguousProviderIsNotAnEmptyInventory(int count)
    {
        var device = new Device { Id = 42 };
        var repository = new Mock<IDeviceRepository>();
        repository.Setup(r => r.GetList()).Returns(new[] { device }.AsQueryable());
        var providers = Enumerable.Range(0, count).Select(_ => Mock.Of<IDeviceZoneProvider>(p => p.CanRead(device) == true)).ToArray();
        var controller = new DetectorController(Mock.Of<IDetectorRepository>(), repository.Object, providers);
        var result = await controller.RetrieveDetectionZoneIdentifierBasedOnDetectionType(
            new ODataActionParameters { ["configurationDeviceId"] = 42 });
        Assert.IsInstanceOfType<Microsoft.AspNetCore.OData.Results.BadRequestODataResult>(result);
    }
}
