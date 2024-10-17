using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.Business.Watchdog
{
    public class WatchDogDetectionTypeGroup
    {
        public DetectionTypes DetectionType { get; set; }
        public List<WatchDogHardwareCount> Hardware { get; set; }
        public string Name { get; set; }

        public WatchDogDetectionTypeGroup()
        {
            Hardware = new List<WatchDogHardwareCount>();
        }
    }
}
