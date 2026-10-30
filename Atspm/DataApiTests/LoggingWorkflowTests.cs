using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.DataApi.Controllers;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Repositories.EventLogRepositories;
using Utah.Udot.Atspm.Services;
using Xunit;

namespace DataApiTests;

public class LoggingWorkflowTests
{
    private static async IAsyncEnumerable<T> Empty<T>() { await Task.CompletedTask; yield break; }

    [Fact]
    public async Task ReachableDeviceRunsInitializedWorkflowAndReturnsCounts()
    {
        var device = new Device { Id = 1, DeviceIdentifier = "1", LoggingEnabled = true, Ipaddress = "127.0.0.1",
            Location = new Location { LocationIdentifier = "1" },
            DeviceConfiguration = new DeviceConfiguration { Protocol = TransportProtocols.Http, Port = 8080, Path = "/test" } };
        var devices = new Mock<IDeviceRepository>();
        devices.Setup(d => d.GetList()).Returns(new[] { device }.AsQueryable());
        var client = new Mock<IDownloaderClient>();
        client.SetupGet(c => c.Protocol).Returns(TransportProtocols.Http);
        client.SetupGet(c => c.IsConnected).Returns(true);
        var downloader = new Mock<IDeviceDownloader>();
        downloader.Setup(d => d.CanExecute(It.IsAny<Device>())).Returns(true);
        downloader.Setup(d => d.Execute(It.IsAny<Device>(), It.IsAny<CancellationToken>())).Returns(Empty<Tuple<Device, FileInfo>>());
        var events = new Mock<IEventLogRepository>();
        events.Setup(e => e.GetData("1", It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1)).Returns(() => Empty<CompressedEventLogBase>());
        using var services = new ServiceCollection().AddSingleton(client.Object).AddSingleton(downloader.Object).AddSingleton(events.Object).BuildServiceProvider();
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Features:DeviceTestDownload"] = "true" }).Build();
        var controller = new LoggingController(devices.Object, services.GetRequiredService<IServiceScopeFactory>(), NullLogger<LoggingController>.Instance, config);
        var response = await controller.SyncDeviceEventsAsync(new() { DeviceIds = new[] { 1 } }, default).WaitAsync(TimeSpan.FromSeconds(10));
        var result = Assert.Single(response.Value!);
        Assert.Equal(0, result.BeforeWorkflowEventCount);
        Assert.Equal(0, result.AfterWorkflowEventCount);
        downloader.Verify(d => d.Execute(It.IsAny<Device>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
