using ATSPM.Application.Business.Common;

namespace ATSPM.Application.Business.LeftTurnGapReport
{
    public class LeftTurnGapDataCheckOptions : OptionsBase
    {
        public int ApproachId { get; set; }
        public int VolumePerHourThreshold { get; set; }
        public double GapOutThreshold { get; set; }
        public double PedestrianThreshold { get; set; }
        public int[] DaysOfWeek { get; set; }
    }
}
