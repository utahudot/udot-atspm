//using System;
//using System.Collections.Generic;
//using System.Net;
//using System.Net.Http;
//using System.Net.Http.Json;
//using System.Text;
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
//using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;

//namespace Utah.Udot.Atspm.Infrastructure.Services.Listeners.Tests
//{
//    // A fake TCP receiver that lets the test trigger incoming messages
//    internal class FakeTcpReceiver : ITcpReceiver
//    {
//        private Func<byte[], EndPoint, Task>? _onMessage;
//        public Task ReceiveAsync(Func<byte[], EndPoint, Task> onMessage, CancellationToken ct)
//        {
//            _onMessage = onMessage;
//            return Task.CompletedTask;
//        }
//        public Task TriggerAsync(byte[] buffer, EndPoint ep)
//        {
//            if (_onMessage == null) throw new InvalidOperationException("ReceiveAsync not called");
//            return _onMessage(buffer, ep);
//        }
//        public void Dispose() { }
//    }

//    public class TcpspeedBatchListenerTests
//    {
//        private EventListenerConfiguration GetConfig(
//            int batchSize = 2,
//            int intervalSeconds = 60,
//            int udpPort = 12000,
//            int tcpPort = 12000,
//            string baseUrl = "http://unused")
//        {
//            return new EventListenerConfiguration
//            {
//                BatchSize = batchSize,
//                IntervalSeconds = intervalSeconds,
//                UdpPort = udpPort,
//                TcpPort = tcpPort,
//                ApiBaseUrl = baseUrl
//            };
//        }

//        [Fact]
//        public async Task ReceiveAsync_ShouldPost_WhenBatchSizeReached()
//        {
//            // 1) Fake receiver
//            var fakeReceiver = new FakeTcpReceiver();

//            // 2) Mock HTTP handler to capture the outgoing POST
//            HttpRequestMessage? captured = null;
//            var handlerMock = new Mock<HttpMessageHandler>();
//            handlerMock
//               .Protected()
//               .Setup<Task<HttpResponseMessage>>(
//                  "SendAsync",
//                  ItExpr.IsAny<HttpRequestMessage>(),
//                  ItExpr.IsAny<CancellationToken>()
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

//            // 3) Mock out IDeviceRepository
//            var deviceRepoMock = new Mock<IDeviceRepository>();

//            var tcpNullLogger = NullLogger<TCPSpeedBatchListener>.Instance;
//            // 3) Create listener with fake receiver
//            var options = Options.Create(GetConfig(batchSize: 2));
//            var listener = new TCPSpeedBatchListener(
//                fakeReceiver,
//                clientFactory.Object,
//                options,
//                tcpNullLogger,
//                deviceRepoMock.Object
//            );

//            // *** Add this line so the fakeReceiver.ReceiveAsync(...) runs ***
//            await listener.StartListeningAsync(CancellationToken.None);

//            // 4) Act: trigger two TCP “messages” with CSV payloads
//            var ep = new IPEndPoint(IPAddress.Loopback, 1234);
//            string MakeCsv(string loc, string det, int mph, int kph) =>
//                $"{loc},{DateTime.UtcNow.Ticks},{det},{mph},{kph}";

//            await fakeReceiver.TriggerAsync(
//                Encoding.UTF8.GetBytes(MakeCsv("Loc1", "D1", 30, 48)), ep);
//            await fakeReceiver.TriggerAsync(
//                Encoding.UTF8.GetBytes(MakeCsv("Loc2", "D2", 25, 40)), ep);

//            // allow the batch-send timer/task to run
//            await Task.Delay(50);

//            // 5) Assert: we POSTed once to /SpeedEvent with two events
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
