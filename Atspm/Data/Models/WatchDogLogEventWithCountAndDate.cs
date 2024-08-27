using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.Data.Models
{
    public class WatchDogLogEventWithCountAndDate : WatchDogLogEvent
    {
        public WatchDogLogEventWithCountAndDate(int locationId, string locationIdentifier, DateTime timestamp, WatchDogComponentTypes componentType, int componentId, WatchDogIssueTypes issueType, string details, int? phase) : base(locationId, locationIdentifier, timestamp, componentType, componentId, issueType, details, phase)
        {
        }

        public int EventCount { get; set; }
        public DateTime DateOfFirstInstance { get; set; }
    }
}
