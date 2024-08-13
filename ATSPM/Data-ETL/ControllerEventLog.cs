namespace ArchiveLogs.Data.Models
{

    public class ControllerEventLog
    {
        public string SignalId { get; set; }
        public DateTime Timestamp { get; set; }
        public short EventCode { get; set; }
        public short EventParam { get; set; }

        public override string ToString()
        {
            return $"{SignalId}-{EventCode}-{EventParam}-{Timestamp}";
        }
    }
}


