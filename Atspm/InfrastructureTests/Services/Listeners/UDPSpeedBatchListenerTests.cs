//using System;
//using System.Collections.Generic;
//using System.Net.Http;
//using System.Net.Http.Json;
//using System.Threading;
//using System.Threading.Tasks;
//using FluentAssertions;
//using Microsoft.Extensions.Options;
//using Moq;
//using Moq.Protected;
//using Xunit;
//using Utah.Udot.Atspm.Data.Models.EventLogModels;
//using Utah.Udot.Atspm.Infrastructure.Configuration;
//using Utah.Udot.Atspm.Infrastructure.Services.Listeners;
//using Utah.Udot.Atspm.Infrastructure.Services.Receivers;
//using Microsoft.Extensions.Logging.Abstractions;
//using Utah.Udot.Atspm.Infrastructure.Messaging;
//using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;  // for IUdpReceiver

//namespace Utah.Udot.Atspm.Infrastructure.Services.Listeners.Tests
//{
//    public class UdpSpeedBatchListenerTests
//    {
//        private EventListenerConfiguration GetConfig(
//            int batchSize = 2,
//            int intervalSeconds = 60,
//            int udpPort = 12000,
//            string baseUrl = "http://unused")
//        {
//            return new EventListenerConfiguration
//            {
//                BatchSize = batchSize,
//                IntervalSeconds = intervalSeconds,
//                UdpPort = udpPort,
//                ApiBaseUrl = baseUrl
//            };
//        }

//        [Fact]
//        public async Task Enqueue_ShouldPost_WhenBatchSizeReached()
//        {
//            // 1) Arrange a fake receiver (we won't actually receive anything for Enqueue tests)
//            var fakeReceiver = new Mock<IUdpReceiver>();

//            // 2) Arrange a fake HttpMessageHandler to capture outgoing HTTP requests
//            HttpRequestMessage? captured = null;
//            var handlerMock = new Mock<HttpMessageHandler>();
//            handlerMock
//               .Protected()
//               .Setup<Task<HttpResponseMessage>>(
//                   "SendAsync",
//                   ItExpr.IsAny<HttpRequestMessage>(),
//                   ItExpr.IsAny<CancellationToken>()
//               )
//               .Callback<HttpRequestMessage, CancellationToken>((req, _) => captured = req)
//               .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

//            var httpClient = new HttpClient(handlerMock.Object)
//            {
//                BaseAddress = new Uri("http://unused")
//            };
//            var clientFactory = new Mock<IHttpClientFactory>();
//            clientFactory
//              .Setup(f => f.CreateClient(It.IsAny<string>()))
//              .Returns(httpClient);

//            var nullLogger = NullLogger<UDPSpeedBatchListener>.Instance;

//            // 3) Mock out IDeviceRepository
//            var deviceRepoMock = new Mock<IDeviceRepository>();

//            // 3) Create the listener, injecting the fake receiver & http factory
//            var options = Options.Create(GetConfig(batchSize: 2));
//            var listener = new UDPSpeedBatchListener(
//                fakeReceiver.Object,
//                clientFactory.Object,
//                options,
//                nullLogger,
//                deviceRepoMock.Object
//            );
//            // 4) Act: enqueue two SpeedEvents → should trigger a POST
//            listener.Enqueue(new RawSpeedPacket
//            {   SenderIp = "127.0.0.1",
//                Timestamp = DateTime.UtcNow,
//                SensorId = "D1",
//                Mph = 30,
//                Kph = 48
//            });
//            listener.Enqueue(new RawSpeedPacket
//            {
//                SenderIp = "127.0.0.1",
//                Timestamp = DateTime.UtcNow.AddSeconds(1),
//                SensorId = "D2",
//                Mph = 25,
//                Kph = 40
//            });

//            // allow any async flush to run
//            await Task.Delay(50);

//            // 5) Assert: we did POST exactly once to /SpeedEvent with our two events
//            captured.Should().NotBeNull();
//            captured!.Method.Should().Be(HttpMethod.Post);
//            captured.RequestUri!.PathAndQuery.Should().Be("/api/v1/SpeedEvent");

//            var body = await captured.Content!.ReadFromJsonAsync<List<SpeedEvent>>();
//            body.Should().HaveCount(2);
//            body![0].DetectorId.Should().Be("D1");
//            body[1].DetectorId.Should().Be("D2");
//        }
//    }
//}
