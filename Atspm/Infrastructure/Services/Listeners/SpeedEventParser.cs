using System.Text;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Infrastructure.Services.Listeners
{
    internal static class SpeedEventParser
    {
        public static SpeedEvent Parse(byte[] buffer, string sourceEndpoint)
        {
            var text = Encoding.UTF8.GetString(buffer).Trim();
            var parts = text.Split(',');

            if (parts.Length != 5)
                throw new FormatException($"Expected 5 CSV fields, got {parts.Length}: '{text}'");

            // ticks → DateTime (UTC)
            var timestamp = new DateTime(long.Parse(parts[1]), DateTimeKind.Utc);
            return new SpeedEvent
            {
                LocationIdentifier = parts[0],
                Timestamp = timestamp,
                DetectorId = parts[2],
                Mph = int.Parse(parts[3]),
                Kph = int.Parse(parts[4])
            };
        }
    }
}
