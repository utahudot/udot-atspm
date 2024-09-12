namespace Utah.Udot.Atspm.Business.Watchdog
{
    public class WatchDogControllerTypeGroup
    {
        public String Name { get; set; }
        public List<WatchDogModel<WatchDogFirmwareWithIssueType>> Model { get; set; }
    }
}
