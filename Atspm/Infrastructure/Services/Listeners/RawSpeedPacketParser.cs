using System.Text;
using Utah.Udot.Atspm.Infrastructure.Messaging;

namespace Utah.Udot.Atspm.Infrastructure.Services.Listeners
{
    public static class RawSpeedPacketParser
    {
        public static RawSpeedPacket Parse(byte[] data, string senderIp)
        {
            // exactly your old logic, but return the DTO
            var timestamp = DateTime.UtcNow;
            var sensorId = Encoding.ASCII.GetString(data, 10, 6);
            var mph = data[8];
            var kph = data[9];

            return new RawSpeedPacket
            {
                SensorId = sensorId,
                Timestamp = timestamp,
                Mph = mph,
                Kph = kph,
                SenderIp = senderIp
            };
        }
    }

}
