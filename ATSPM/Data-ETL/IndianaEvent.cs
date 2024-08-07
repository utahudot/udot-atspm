
namespace ArchiveLogs.Data.Models
{

    public class IndianaEvent
    {
        public string LocationIdentifier { get; set; }
        public DateTime Timestamp { get; set; }
        public int EventCode { get; set; }
        public int EventParam { get; set; }

        public override string ToString()
        {
            return $"{LocationIdentifier}-{EventCode}-{EventParam}-{Timestamp}";
        }
    }
}


