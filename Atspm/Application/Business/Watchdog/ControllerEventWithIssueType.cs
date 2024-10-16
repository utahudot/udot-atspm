using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.Business.Watchdog
{
    public class ControllerEventWithIssueType
    {
        public WatchDogIssueTypes IssueType { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Firmware { get; set; }
    }
}
