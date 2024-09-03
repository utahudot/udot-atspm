using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Interfaces;

namespace Utah.Udot.Atspm.Data.Models
{
    public class WatchDogIgnoreEvents : ILocationLayer
    {
        /// <summary>
        /// Location id
        /// </summary>
        public int LocationId { get; set; }

        /// <summary>
        /// Location identifier
        /// </summary>
        public string LocationIdentifier { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Component type
        /// </summary>
        public WatchDogComponentTypes ComponentType { get; set; }

        /// <summary>
        /// Component id
        /// </summary>
        public int ComponentId { get; set; }

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
        public WatchDogIgnoreEvents(int locationId, string locationIdentifier, DateTime timestamp, WatchDogComponentTypes componentType, int componentId, WatchDogIssueTypes issueType, string details, int? phase)
        {
            this.LocationId = locationId;
            this.LocationIdentifier = locationIdentifier;
            EndTime = timestamp;
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

            WatchDogIgnoreEvents other = (WatchDogIgnoreEvents)obj;

            return LocationIdentifier == other.LocationIdentifier &&
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
