using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Polly;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.DataApi.Services;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;

namespace DataApiTests
{
    public class EventLogImporterServiceTests
    {
        [Fact]
        public void CompressEvents_GroupsEventsByDayAndUsesSignalController()
        {
            var locations = new Mock<ILocationRepository>();
            locations
                .Setup(repository => repository.GetLatestVersionOfLocation("7521"))
                .Returns(new Location { Id = 7, LocationIdentifier = "7521" });

            var devices = new Mock<IDeviceRepository>();
            devices
                .Setup(repository => repository.GetActiveDevicesByLocation(7))
                .Returns(
                [
                    new Device { Id = 14, DeviceType = DeviceTypes.RampController },
                    new Device { Id = 15, DeviceType = DeviceTypes.SignalController }
                ]);

            var service = new EventLogImporterService(
                Policy.Handle<Exception>().RetryAsync(0),
                Mock.Of<IServiceScopeFactory>(),
                locations.Object,
                devices.Object,
                Mock.Of<ILogger<EventLogImporterService>>());
            var events = new[]
            {
                new IndianaEvent { Timestamp = new DateTime(2026, 7, 2, 0, 5, 0) },
                new IndianaEvent { Timestamp = new DateTime(2026, 7, 1, 23, 55, 0) },
                new IndianaEvent { Timestamp = new DateTime(2026, 7, 1, 23, 50, 0) }
            };

            var result = service.CompressEvents("7521", events);

            Assert.Collection(
                result,
                first =>
                {
                    Assert.Equal(15, first.DeviceId);
                    Assert.Equal(new DateTime(2026, 7, 1, 23, 50, 0), first.Start);
                    Assert.Equal(new DateTime(2026, 7, 1, 23, 55, 0), first.End);
                    Assert.Equal(2, first.Data.Count);
                },
                second =>
                {
                    Assert.Equal(15, second.DeviceId);
                    Assert.Equal(new DateTime(2026, 7, 2, 0, 5, 0), second.Start);
                    Assert.Equal(new DateTime(2026, 7, 2, 0, 5, 0), second.End);
                    Assert.Single(second.Data);
                });
        }

        [Fact]
        public void CompressEvents_RejectsLocationWithoutActiveController()
        {
            var locations = new Mock<ILocationRepository>();
            locations
                .Setup(repository => repository.GetLatestVersionOfLocation("7521"))
                .Returns(new Location { Id = 7, LocationIdentifier = "7521" });

            var devices = new Mock<IDeviceRepository>();
            devices
                .Setup(repository => repository.GetActiveDevicesByLocation(7))
                .Returns([]);

            var service = new EventLogImporterService(
                Policy.Handle<Exception>().RetryAsync(0),
                Mock.Of<IServiceScopeFactory>(),
                locations.Object,
                devices.Object,
                Mock.Of<ILogger<EventLogImporterService>>());

            var exception = Assert.Throws<InvalidOperationException>(() =>
                service.CompressEvents(
                    "7521",
                    [new IndianaEvent { Timestamp = new DateTime(2026, 7, 1) }]));

            Assert.Contains("No active controller", exception.Message);
        }
    }
}
