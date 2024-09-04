namespace Utah.Udot.Atspm.Data.Models.WatchDogModels
{
    public class WatchDogControllerTypeGroup
    {
        public String Name { get; set; }
        public List<WatchDogModel<WatchDogFirmwareWithIssueType>> Model { get; set; }
    }
}
