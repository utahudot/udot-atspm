using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Interfaces;

namespace Utah.Udot.Atspm.Data.Models
{
    public class WatchDogIgnoreEvent : ILocationLayer
    {

        /// <summary>
        /// Location identifier
        /// </summary>
        public string? LocationIdentifier { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Component type
        /// </summary>
        public WatchDogComponentTypes? ComponentType { get; set; }

        /// <summary>
        /// Component id
        /// </summary>
        public int? ComponentId { get; set; }

        /// <summary>
        /// Issue type
        /// </summary>
        public WatchDogIssueTypes IssueType { get; set; }

        /// <summary>
        /// Phase
        /// </summary>
        public int? Phase { get; set; }

        /// <summary>
        /// Watchdog log event
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="locationIdentifier"></param>
        /// <param name="timestamp"></param>
        /// <param name="componentType"></param>
        /// <param name="componentId"></param>
        /// <param name="issueType"></param>
        /// <param name="details"></param>
        /// <param name="phase"></param>
        public WatchDogIgnoreEvent(string locationIdentifier, DateTime startTime, DateTime endTime, WatchDogComponentTypes componentType, int componentId, WatchDogIssueTypes issueType, string details, int? phase)
        {
            this.LocationIdentifier = locationIdentifier;
            StartTime = startTime;
            EndTime = startTime;
            ComponentType = componentType;
            ComponentId = componentId;
            IssueType = issueType;
            Phase = phase;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            WatchDogIgnoreEvent other = (WatchDogIgnoreEvent)obj;

            return LocationIdentifier == other.LocationIdentifier &&
                    StartTime == other.StartTime &&
                    EndTime == other.EndTime &&
                    ComponentType == other.ComponentType &&
                    ComponentId == other.ComponentId &&
                    IssueType == other.IssueType;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(LocationIdentifier, EndTime, ComponentType, ComponentId, IssueType);
        }
    }
}
