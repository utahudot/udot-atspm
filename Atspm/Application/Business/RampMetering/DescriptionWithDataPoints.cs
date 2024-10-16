using Utah.Udot.Atspm.Business.Common;

namespace Utah.Udot.Atspm.Business.RampMetering
{
    public class DescriptionWithDataPoints
    {
        public string Description { get; set; }
        public List<DataPointDateDouble> Value { get; set; }
    }
}
