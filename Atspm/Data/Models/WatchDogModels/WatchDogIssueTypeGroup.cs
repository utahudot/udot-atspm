using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.Data.Models.WatchDogModels
{
    public class WatchDogIssueTypeGroup
    {
        public WatchDogIssueTypes IssueType { get; set; }
        public List<ProductEvent> Products { get; set; }
        public string Name { get; set; }

        public WatchDogIssueTypeGroup()
        {
            Products = new List<ProductEvent>();
        }
    }

    public class ProductEvent
    {
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public List<FirmwareEvent> Firmware { get; set; }
    }

    public class FirmwareEvent
    {
        public string Firmware { get; set; }
        public int Counts { get; set; }
    }
}
