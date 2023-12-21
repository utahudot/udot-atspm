using ATSPM.Data.Enums;
using ATSPM.Data.Models;

namespace ATSPM.ReportApi.Business.Watchdog
{
    public class WatchDogLogEventDTO:WatchDogLogEvent
    {
        public WatchDogLogEventDTO(
            int locationId,
            string locationIdentifier,
            DateTime timestamp,
            WatchDogComponentType componentType,
            int componentId,
            WatchDogIssueType issueType,
            string details,
            int? phase,
            int? regionId,
            int? jurisdictionId) : base(locationId, locationIdentifier, timestamp, componentType, componentId, issueType, details, phase)
        {
            RegionId = regionId;
            JurisdictionId = jurisdictionId;
        }

        public int? RegionId { get; set; } 
        public int? JurisdictionId { get; set; }
    }
}
