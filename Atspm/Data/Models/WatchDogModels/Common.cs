namespace Utah.Udot.Atspm.Data.Models.WatchDogModels
{
    public class WatchDogProductInfo
    {
        public string Name { get; set; }
        public List<WatchDogModel<WatchDogFirmwareCount>> Model { get; set; }
    }

    public class WatchDogModel<T>
    {
        public string Name { get; set; }
        public List<T> Firmware { get; set; }
    }

    public class WatchDogFirmwareCount
    {
        public string Name { get; set; }
        public int Counts { get; set; }
    }
    public class WatchDogHardwareCount
    {
        public string Name { get; set; }
        public int Counts { get; set; }
    }

    public class WatchDogIssueTypeCount
    {
        public string Name { get; set; }
        public int Counts { get; set; }
    }

    public class WatchDogFirmwareWithIssueType
    {
        public string Name { get; set; }
        public List<WatchDogIssueTypeCount> IssueType { get; set; }
    }

    //public class WatchDogControllerInfo
    //{
    //    public string Name { get; set; }
    //    public List<WatchDogModel<WatchDogFirmwareWithIssueType>> Model { get; set; }
    //}
}
