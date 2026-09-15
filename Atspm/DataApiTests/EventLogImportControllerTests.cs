using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Text;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.DataApi.Controllers;
using Utah.Udot.Atspm.DataApi.Services;

namespace DataApiTests
{
    public class EventLogImportControllerTests
    {
        [Fact]
        public async Task UploadEventsFromCompressedJson_ImportsGzipPayload()
        {
            const string locationIdentifier = "7521";
            var importer = new Mock<IEventLogImporterService>();
            IReadOnlyCollection<IndianaEvent>? importedEvents = null;
            var compressedLogs = new List<CompressedEventLogs<IndianaEvent>>
            {
                new()
                {
                    LocationIdentifier = locationIdentifier,
                    DeviceId = 12,
                    Start = new DateTime(2026, 7, 1, 6, 0, 0),
                    End = new DateTime(2026, 7, 1, 6, 0, 0),
                    Data = []
                }
            };

            importer
                .Setup(service => service.CompressEvents(
                    locationIdentifier,
                    It.IsAny<IReadOnlyCollection<IndianaEvent>>()))
                .Callback<string, IReadOnlyCollection<IndianaEvent>>(
                    (_, events) => importedEvents = events)
                .Returns(compressedLogs);
            importer
                .Setup(service => service.InsertLogsWithRetryAsync(
                    compressedLogs,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var controller = CreateController(
                importer.Object,
                CreateGzipBody(
                [
                    new IndianaEvent
                    {
                        Timestamp = new DateTime(2026, 7, 1, 6, 0, 0),
                        EventCode = 82,
                        EventParam = 1
                    }
                ]));

            var result = await controller.UploadEventsFromCompressedJsonAsync(
                locationIdentifier,
                CancellationToken.None);

            Assert.IsType<OkObjectResult>(result);
            var importedEvent = Assert.Single(importedEvents!);
            Assert.Equal(locationIdentifier, importedEvent.LocationIdentifier);
            importer.VerifyAll();
        }

        [Fact]
        public async Task UploadEventsFromCompressedJson_RejectsInvalidGzip()
        {
            var importer = new Mock<IEventLogImporterService>(MockBehavior.Strict);
            var controller = CreateController(
                importer.Object,
                new MemoryStream(Encoding.UTF8.GetBytes("not gzip")));

            var result = await controller.UploadEventsFromCompressedJsonAsync(
                "7521",
                CancellationToken.None);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        private static EventLogImportController CreateController(
            IEventLogImporterService importer,
            Stream body)
        {
            var controller = new EventLogImportController(
                importer,
                Mock.Of<ILogger<EventLogImportController>>());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.Request.Body = body;
            return controller;
        }

        private static MemoryStream CreateGzipBody(IEnumerable<IndianaEvent> events)
        {
            var stream = new MemoryStream();
            using (var gzip = new GZipStream(stream, CompressionMode.Compress, leaveOpen: true))
            using (var writer = new StreamWriter(gzip, Encoding.UTF8))
            {
                writer.Write(JsonConvert.SerializeObject(events));
            }

            stream.Position = 0;
            return stream;
        }
    }
}
