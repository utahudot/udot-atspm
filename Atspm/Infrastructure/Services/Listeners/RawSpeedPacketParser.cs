using Microsoft.Extensions.Logging;
using System.Text;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Infrastructure.Services.Listeners
{
    public static class RawSpeedPacketParser
    {
        public static SpeedEvent Parse(byte[] data, ILogger logger)
        {
            if (data == null || data.Length < 16)
                throw new ArgumentException($"Expected at least 16 bytes, got {data?.Length ?? 0}.", nameof(data));

            // Original packet fields
            byte header = data[7];
            byte mph = data[8];
            byte kph = data[9];
            string sensorId = Encoding.ASCII.GetString(data, 10, 6);

            // Optional appended timestamp
            string? timestampStr = null;
            if (data.Length > 16)
            {
                timestampStr = Encoding.ASCII.GetString(data, 16, data.Length - 16)
                    .TrimStart('~', '\r', '\n', ' ')
                    .TrimEnd('\r', '\n', '\0', ' ');

                logger.LogDebug("Parsed UTC timestamp: {Timestamp}", timestampStr);
            }

            // Try parsing timestamp if needed
            DateTime? parsedTimestamp = null;
            if (DateTime.TryParse(timestampStr, out var utcTime))
            {
                parsedTimestamp = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc).ToLocalTime();
            }

            return new SpeedEvent
            {
                DetectorId = sensorId,
                Mph = mph,
                Kph = kph,
                Timestamp = parsedTimestamp ?? DateTime.Now
            };
        }


    }
}
