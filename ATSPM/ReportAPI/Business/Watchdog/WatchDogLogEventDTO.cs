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
            string? regionDescription,
            int? jurisdictionId,
            string? jurisdictionName,
            IEnumerable<AreaDTO> areas) : base(locationId, locationIdentifier, timestamp, componentType, componentId, issueType, details, phase)
        {
            RegionId = regionId;
            RegionDescription = regionDescription;
            JurisdictionId = jurisdictionId;
            JurisdictionName = jurisdictionName;
            Areas = areas;
        }

        public int? RegionId { get; set; }
        public string? RegionDescription { get; set; }
        public int? JurisdictionId { get; set; }
        public string? JurisdictionName { get; set; }
        public IEnumerable<AreaDTO> Areas  { get; set; }
        
    } 
}
