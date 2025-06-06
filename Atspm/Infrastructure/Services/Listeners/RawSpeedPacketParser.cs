using System.Text;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Infrastructure.Messaging;

namespace Utah.Udot.Atspm.Infrastructure.Services.Listeners
{
    public static class RawSpeedPacketParser
    {
        public static SpeedEvent Parse(byte[] data, string senderIp)
        {
            if (data == null || data.Length < 16)
            {
                throw new ArgumentException("Data packet is null or too short.", nameof(data));
            }
            // exactly your old logic, but return the DTO
            var timestamp = DateTime.Now;
            var sensorId = Encoding.ASCII.GetString(data, 10, 6);
            var mph = data[8];
            var kph = data[9];

            return new SpeedEvent
            {
                DetectorId = sensorId,
                Timestamp = timestamp,
                Mph = mph,
                Kph = kph
            };
        }
    }

}
