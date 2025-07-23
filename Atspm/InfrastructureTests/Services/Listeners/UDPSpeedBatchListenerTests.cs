using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Infrastructure.Configuration;
using Utah.Udot.Atspm.Infrastructure.Messaging;
using Utah.Udot.Atspm.Infrastructure.Services.Listeners;
using Utah.Udot.Atspm.Infrastructure.Services.Receivers;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Services;
using Xunit;

namespace Utah.Udot.Atspm.Infrastructure.Services.Listeners.Tests
{
    public class UdpSpeedBatchListenerTests
    {
        private EventListenerConfiguration GetConfig(
            int batchSize = 2,
            int udpPort = 12000,
            string baseUrl = "http://unused")
        {
            return new EventListenerConfiguration
            {
                BatchSize = batchSize,
                UdpPort = udpPort,
                ApiBaseUrl = baseUrl,
                threads = 1
            };
        }

        [Fact]
        public async Task Enqueue_ShouldPublish_WhenBatchSizeReached()
        {
            // Arrange
            var fakeReceiver = new Mock<IUdpReceiver>();

            var deviceRepoMock = new Mock<IDeviceRepository>();
            deviceRepoMock.Setup(repo => repo.GetList()).Returns(new List<Device>
            {
                new Device
                {
                    Id = 1,
                    DeviceIdentifier = "D1",
                    DeviceType = Data.Enums.DeviceTypes.SpeedSensor,
                    Location = new Data.Models.Location
                    {
                        Id = 10,
                        LocationIdentifier = "L1"
                    }
                },
                new Device
                {
                    Id = 2,
                    DeviceIdentifier = "D2",
                    DeviceType = Data.Enums.DeviceTypes.SpeedSensor,
                    Location = new Data.Models.Location
                    {
                        Id = 11,
                        LocationIdentifier = "L2"
                    }
                }
            }.AsQueryable());

            var publisherMock = new Mock<IEventPublisher<EventBatchEnvelope>>();
            List<EventBatchEnvelope>? publishedEnvelopes = null;

            publisherMock
                .Setup(p => p.PublishAsync(It.IsAny<IReadOnlyList<EventBatchEnvelope>>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<EventBatchEnvelope>, int, CancellationToken>((envelopes, _, _) =>
                {
                    publishedEnvelopes = envelopes.ToList();
                })
                .Returns(Task.CompletedTask)
                .Verifiable();

            var options = Options.Create(GetConfig(batchSize: 2));

            var listener = new UDPSpeedBatchListener(
                fakeReceiver.Object,
                options,
                new LoggerFactory(),
                deviceRepoMock.Object,
                publisherMock.Object
            );

            // Act
            listener.Enqueue(new SpeedEvent
            {
                Timestamp = DateTime.UtcNow,
                DetectorId = "D1",
                Mph = 30,
                Kph = 48
            });

            listener.Enqueue(new SpeedEvent
            {
                Timestamp = DateTime.UtcNow.AddSeconds(1),
                DetectorId = "D2",
                Mph = 25,
                Kph = 40
            });

            await Task.Delay(50); // allow async batch to complete

            // Assert
            publisherMock.Verify(p => p.PublishAsync(It.IsAny<IReadOnlyList<EventBatchEnvelope>>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            publishedEnvelopes.Should().NotBeNull();
            publishedEnvelopes!.Count.Should().Be(2);
            publishedEnvelopes.Select(e => e.DeviceId).Should().BeEquivalentTo(new[] { 1, 2 });
        }

        [Fact]
        public async Task StartListeningAsync_ShouldParseAndEnqueue()
        {
            // Arrange
            var packet = new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 83, 30, 48, // header, mph, kph
                (byte)'D', (byte)'1', (byte)' ', (byte)' ', (byte)' ', (byte)' ', // sensorId (6 chars, padded)
                (byte)'0', (byte)'7', (byte)'-', (byte)'2', (byte)'2', (byte)'-', (byte)'2', (byte)'0', (byte)'2', (byte)'5', (byte)' ', (byte)'1', (byte)'8', (byte)':', (byte)'3', (byte)'0'
            };

            Func<byte[], EndPoint, Task>? callback = null;
            var fakeReceiver = new Mock<IUdpReceiver>();
            fakeReceiver
                .Setup(r => r.ReceiveAsync(It.IsAny<Func<byte[], EndPoint, Task>>(), It.IsAny<CancellationToken>()))
                .Callback<Func<byte[], EndPoint, Task>, CancellationToken>((cb, _) => callback = cb)
                .Returns(Task.CompletedTask);

            var deviceRepoMock = new Mock<IDeviceRepository>();
            deviceRepoMock.Setup(repo => repo.GetList()).Returns(new List<Device>
            {
                new Device
                {
                    Id = 1,
                    DeviceIdentifier = "D1",
                    DeviceType = Data.Enums.DeviceTypes.SpeedSensor,
                    Location = new Data.Models.Location
                    {
                        Id = 10,
                        LocationIdentifier = "L1"
                    }
                }
            }.AsQueryable());

            var publisherMock = new Mock<IEventPublisher<EventBatchEnvelope>>();
            publisherMock
                .Setup(p => p.PublishAsync(It.IsAny<IReadOnlyList<EventBatchEnvelope>>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var options = Options.Create(GetConfig(batchSize: 1));

            var listener = new UDPSpeedBatchListener(
                fakeReceiver.Object,
                options,
                new LoggerFactory(),
                deviceRepoMock.Object,
                publisherMock.Object
            );

            // Act
            await listener.StartListeningAsync(CancellationToken.None);
            await callback!(packet, new IPEndPoint(IPAddress.Loopback, 12000));
            await Task.Delay(50);

            // Assert
            publisherMock.Verify(p =>
                p.PublishAsync(It.Is<IReadOnlyList<EventBatchEnvelope>>(e => e.Any(env => env.DeviceId == 1)),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
