using ATSPM.ReportApi.Business.Common;

namespace ATSPM.ReportApi.Business.Watchdog
{
    public class WatchDogOptions:OptionsBase
    {
        public int? AreaId { get; set; }
        public int? RegionId { get; set; }
        public int? JurisdictionId { get; set; }
        public int? LocationId { get; set; }
        public int? IssueType { get; set; }
    }
}
