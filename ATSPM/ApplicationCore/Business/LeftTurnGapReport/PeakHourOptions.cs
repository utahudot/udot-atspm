using ATSPM.Application.Business.Common;

namespace ATSPM.Application.Business.LeftTurnGapReport
{
    public class PeakHourOptions : OptionsBase
    {
        public int ApproachId { get; set; }
        public int[] DaysOfWeek { get; set; }
    }
}
