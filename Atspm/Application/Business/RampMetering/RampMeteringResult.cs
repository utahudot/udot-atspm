using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Business.TimeSpaceDiagram;

namespace Utah.Udot.Atspm.Business.RampMetering
{
    public class RampMeteringResult : LocationResult
    {
        public RampMeteringResult(string locationId, DateTime start, DateTime end): base(locationId, start, end)
        { }

        public List<DataPointForDouble> MainlineAvgFlow { get; set; }
        public List<DataPointForDouble> MainlineAvgOcc { get; set; }
        public List<DataPointForDouble> MainlineAvgSpeed { get; set; }
        public List<TimeSpaceEventBase> StartUpWarning { get; set; }
        public List<TimeSpaceEventBase> ShutdownWarning { get; set; }
        public List<DescriptionWithDataPoints> LanesActiveRate { get; set; }
        public List<DescriptionWithDataPoints> LanesBaseRate { get; set; }
        public List<DescriptionWithDataPoints> LanesQueueOnEvents { get; set; }
        public List<DescriptionWithDataPoints> LanesQueueOffEvents { get; set; }
    }
}
