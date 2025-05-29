using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;          // SpeedEvent, CompressedEventLogBase
using Utah.Udot.Atspm.Infrastructure.Messaging;             // EventBatchEnvelope
using Utah.Udot.ATSPM.Infrastructure.Messaging.Database;
using Utah.Udot.Atspm.Repositories.EventLogRepositories;
using Xunit;
using Utah.Udot.Atspm.Extensions;

namespace Utah.Udot.ATSPM.InfrastructureTests.Messaging.Database
{
    public class DatabaseEventPublisherTests
    {
        [Fact]
        public async Task PublishAsync_Batch_CallsUpsertForEachCompressedLog()
        {
            // --- Arrange ---

            // 1) Mock IEventLogRepository so Upsert returns its input
            var repoMock = new Mock<IEventLogRepository>();
            repoMock
                .Setup(r => r.UpsertAsync(It.IsAny<CompressedEventLogBase>()))
                .ReturnsAsync((CompressedEventLogBase c) => c);

            // 2) Build a fake IServiceScope/IServiceProvider that returns our mock
            var providerMock = new Mock<IServiceProvider>();
            providerMock
                .Setup(p => p.GetService(typeof(IEventLogRepository)))
                .Returns(repoMock.Object);

            var scopeMock = new Mock<IServiceScope>();
            scopeMock
                .Setup(s => s.ServiceProvider)
                .Returns(providerMock.Object);

            var scopesMock = new Mock<IServiceScopeFactory>();
            scopesMock
                .Setup(f => f.CreateScope())
                .Returns(scopeMock.Object);

            // 3) Dummy logger
            var loggerMock = new Mock<ILogger<DatabaseEventPublisher>>();

            // 4) System under test
            var publisher = new DatabaseEventPublisher(
                scopesMock.Object,
                loggerMock.Object
            );

            // 5) Create two simple SpeedEvent envelopes
            var now = DateTime.UtcNow;
            var evt1 = new SpeedEvent { DetectorId = "A", Timestamp = now, Mph = 30, Kph = 48 };
            var evt2 = new SpeedEvent { DetectorId = "B", Timestamp = now, Mph = 25, Kph = 40 };

            var envelope1 = new EventBatchEnvelope
            {
                DeviceId = 1,
                LocationIdentifier = "LocA",
                Items = JArray.FromObject(new[] { evt1 })
            };
            var envelope2 = new EventBatchEnvelope
            {
                DeviceId = 2,
                LocationIdentifier = "LocB",
                Items = JArray.FromObject(new[] { evt2 })
            };
            var batch = new List<EventBatchEnvelope> { envelope1, envelope2 };

            // --- Act ---
            await publisher.PublishAsync(batch, CancellationToken.None);

            // --- Assert ---
            // Should have called Upsert exactly once per envelope, 
            // and propagated DeviceId & LocationIdentifier
            repoMock.Verify(r => r.UpsertAsync(
                It.Is<CompressedEventLogBase>(c =>
                    c.DeviceId == envelope1.DeviceId &&
                    c.LocationIdentifier == envelope1.LocationIdentifier
                )
            ), Times.Once);

            repoMock.Verify(r => r.UpsertAsync(
                It.Is<CompressedEventLogBase>(c =>
                    c.DeviceId == envelope2.DeviceId &&
                    c.LocationIdentifier == envelope2.LocationIdentifier
                )
            ), Times.Once);
        }
    }
}