using DeviceEmulator.Models;
using System.Globalization;
using System.Text;
using System.Xml;

namespace DeviceEmulator.Services
{
    public class HttpXmlDeviceRunner : IDeviceProtocolRunner
    {
        private readonly DeviceDefinition _device;
        private readonly ILogger _logger;
        private readonly string _logFilePath;

        public HttpXmlDeviceRunner(DeviceDefinition device, ILogger logger)
        {
            _device = device;
            _logger = logger;
            _logFilePath = Path.Combine("data", _device.DeviceIdentifier, $"eventlog_{DateTime.UtcNow:yyyy-MM-dd}.xml");

            Directory.CreateDirectory(Path.Combine("data", _device.DeviceIdentifier));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Maxtime HTTP device emulator started for {DeviceId} on port {Port}", _device.DeviceIdentifier, _device.Port);
            return Task.CompletedTask;
        }

        public async Task GenerateLogAsync()
        {
            var eventsXml = new StringBuilder();
            using var xmlWriter = XmlWriter.Create(eventsXml, new XmlWriterSettings { Indent = true });

            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("EventResponses");

            xmlWriter.WriteStartElement("EventResponse");

            // Description
            xmlWriter.WriteStartElement("Description");
            xmlWriter.WriteElementString("IntersectionNumber", "1015");
            xmlWriter.WriteElementString("IP", _device.IpAddress);
            xmlWriter.WriteElementString("MAC", "00,11,22,33,44,55");
            xmlWriter.WriteElementString("StartTime", DateTime.UtcNow.ToString("M-d-yyyy HH:mm:ss.0", CultureInfo.InvariantCulture));
            xmlWriter.WriteElementString("PhasesInUse", "1,2,3,4");
            xmlWriter.WriteEndElement(); // Description

            // Events
            var now = DateTime.UtcNow;
            for (int i = 0; i < 5; i++)
            {
                xmlWriter.WriteStartElement("Event");
                xmlWriter.WriteAttributeString("ID", (i + 1).ToString());
                xmlWriter.WriteAttributeString("TimeStamp", now.AddSeconds(-i * 5).ToString("M-d-yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture));
                xmlWriter.WriteAttributeString("EventTypeID", (80 + i).ToString());
                xmlWriter.WriteAttributeString("Parameter", (1 + i).ToString());
                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement(); // EventResponse
            xmlWriter.WriteEndElement(); // EventResponses
            xmlWriter.WriteEndDocument();
            xmlWriter.Flush();

            await File.WriteAllTextAsync(_logFilePath, eventsXml.ToString());
            _logger.LogInformation("Maxtime device {DeviceId} wrote XML log file to {Path}.", _device.DeviceIdentifier, _logFilePath);
        }
    }
}