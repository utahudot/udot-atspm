using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Utah.Udot.Atspm.Business.TurningMovementCounts;
using Utah.Udot.Atspm.Common;
using Utah.Udot.Atspm.ConfigApi.Controllers;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.MeasureOptions;
using Utah.Udot.Atspm.Exceptions;
using Utah.Udot.Atspm.Infrastructure.Attributes;
using Utah.Udot.Atspm.ReportApi.ReportServices;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;

namespace ConfigApiTests;

[TestClass]
public class ReviewRegressionTests
{
    [TestMethod]
    public void NetworkAndSyncActionsRequireEditorPermissions()
    {
        foreach (var (type, method, permission) in new[]
        {
            (typeof(DetectorController), nameof(DetectorController.RetrieveDetectionZoneIdentifierBasedOnDetectionType), AtspmAuthorization.Permissions.LocationConfigurationsEdit),
            (typeof(DeviceController), nameof(DeviceController.RetrieveDetectionIdentifierBasedOnDetectionType), AtspmAuthorization.Permissions.DeviceEdit),
            (typeof(LocationController), nameof(LocationController.SyncLocation), AtspmAuthorization.Permissions.LocationConfigurationsEdit)
        })
            Assert.AreEqual(AtspmAuthorization.GetPolicyName(permission),
                type.GetMethod(method)!.GetCustomAttribute<AuthorizePermissionAttribute>()?.Policy);
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("atspm")]
    public async Task IndianaSourceKeepsLongRangesAndLargeBins(string? source)
    {
        var options = new TurningMovementCountsOptions { Source = source, LocationIdentifier = "1",
            Start = new DateTime(2026, 1, 1), End = new DateTime(2026, 3, 1), BinSize = 2880 };
        var locations = new Mock<ILocationRepository>();
        locations.Setup(r => r.GetLatestVersionOfLocation("1", options.Start)).Returns(new Location { Id = 1, LocationIdentifier = "1" });
        var counts = new Mock<ITurningMovementCountSource>();
        counts.Setup(s => s.ReadAsync(It.IsAny<TmcCountSourceRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TmcCountSourceResult("Indiana", Array.Empty<MovementCount>(), Array.Empty<string>()));
        var services = new ServiceCollection().AddKeyedSingleton("atspm", counts.Object).BuildServiceProvider();
        var resolver = new TmcCountSourceResolver(Mock.Of<IDeviceRepository>(), services, new ConfigurationBuilder().Build());
        var result = await new TurningMovementCountReportService(locations.Object, resolver, new TmcResultBuilder()).ExecuteAsync(options, null, default);
        Assert.AreEqual("Indiana", result.Source);
        counts.Verify(s => s.ReadAsync(It.IsAny<TmcCountSourceRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [DataTestMethod]
    [DataRow(2880, 1)]
    [DataRow(15, 32)]
    [DataRow(0, 1)]
    [DataRow(15, 0)]
    public async Task DeviceRequestsRetainLimits(int bin, int days)
    {
        var service = new TurningMovementCountReportService(Mock.Of<ILocationRepository>(), null!, new TmcResultBuilder());
        await Assert.ThrowsExceptionAsync<ReportException>(() => service.ExecuteAsync(new TurningMovementCountsOptions
        { Source = "devices", BinSize = bin, Start = new DateTime(2026, 1, 1), End = new DateTime(2026, 1, 1).AddDays(days) }, null, default));
    }
}
