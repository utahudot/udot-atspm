using ATSPM.Data.Enums;

namespace ATSPM.Data.Models
{
    public class WatchDogLogEvent
    {
        public int Id { get; set; }                     // Unique ID for the event
        public int SignalId { get; set; }               // ID of the signal that the event is associated with
        public string SignalIdentifier { get; set; }
        public DateTime Timestamp { get; set; }         // Time when the event was logged
        public WatchDogComponentType ComponentType { get; set; } // 'Signal', 'Approach', or 'Detector'
        public int ComponentId { get; set; }         // Specific identifier for the component (like signal ID)
        public WatchDogIssueType IssueType { get; set; }
        public string Details { get; set; }              // Additional details about the issue
        public int? Phase { get; set; }

        public WatchDogLogEvent(int signalId, string signalIdentifier, DateTime timestamp, WatchDogComponentType componentType, int componentId, WatchDogIssueType issueType, string details, int? phase)
        {
            SignalId = signalId;
            SignalIdentifier = signalIdentifier;
            Timestamp = timestamp;
            ComponentType = componentType;
            ComponentId = componentId;
            IssueType = issueType;
            Details = details;
            Phase = phase;
        }

        public override string ToString()
        {
            return $"[{SignalId}-{Timestamp}] {ComponentType} (ID: {ComponentId}) - {IssueType}: {Details}";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            WatchDogLogEvent other = (WatchDogLogEvent)obj;

            // Compare all properties except for Details
            return SignalIdentifier == other.SignalIdentifier &&
                    Timestamp == other.Timestamp &&
                   ComponentType == other.ComponentType &&
                   ComponentId == other.ComponentId &&
                   IssueType == other.IssueType;
        }

    }
}

