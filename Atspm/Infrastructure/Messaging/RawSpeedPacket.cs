namespace Utah.Udot.Atspm.Infrastructure.Messaging
{
    public class RawSpeedPacket
    {
        public string SensorId { get; set; }
        public DateTime Timestamp { get; set; }
        public int Mph { get; set; }
        public int Kph { get; set; }
        public string SenderIp { get; set; }
    }


}
