namespace Utah.Udot.Atspm.Data.Models.WatchDogModels
{
    public class WatchDogDashboardGroup
    {
        public List<WatchDogIssueTypeGroup> IssueTypeGroup { get; set; }
        public List<WatchDogDetectionTypeGroup> DetectionTypeGroup { get; set; }
        public List<WatchDogControllerTypeGroup> ControllerTypeGroup { get; set; }

        public WatchDogDashboardGroup()
        {
            IssueTypeGroup = new List<WatchDogIssueTypeGroup>();
            DetectionTypeGroup = new List<WatchDogDetectionTypeGroup>();
            ControllerTypeGroup = new List<WatchDogControllerTypeGroup>();
        }
    }
}
