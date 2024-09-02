using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.Data.Models.WatchDogModels
{
    public class WatchDogDetectionTypeGroup
    {
        public DetectionTypes DetectionType { get; set; }
        public List<HardwareEvent> Hardware { get; set; }
        public string Name { get; set; }

        public WatchDogDetectionTypeGroup()
        {
            Hardware = new List<HardwareEvent>();
        }
    }

    public class HardwareEvent
    {
        public string Name { get; set; }
        public int Counts { get; set; }
    }
}
