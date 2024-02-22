using System;

namespace ATSPM.Application.Business.Watchdog
{
    public class WatchDogOptions
    {
        public int? AreaId { get; set; }
        public int? RegionId { get; set; }
        public int? JurisdictionId { get; set; }
        public int? IssueType { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string? LocationIdentifier { get; set; }
    }
}
