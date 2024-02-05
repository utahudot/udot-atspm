using ATSPM.Domain.Common;

#nullable disable

namespace ATSPM.Data.Models
{
    public class ControllerEventLog : ITimestamp
    {
        public string SignalIdentifier { get; set; }
        public DateTime Timestamp { get; set; }
        public int EventCode { get; set; }
        public int EventParam { get; set; }

        public override string ToString()
        {
            return $"{SignalIdentifier}-{EventCode}-{EventParam}-{Timestamp}";
        }
    }
}
