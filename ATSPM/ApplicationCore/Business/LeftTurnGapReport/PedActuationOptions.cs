using ATSPM.Application.Business.Common;

namespace ATSPM.Application.Business.LeftTurnGapReport
{
    public class PedActuationOptions : OptionsBase
    {
        public int ApproachId { get; set; }
        public int StartHour { get; set; }
        public int StartMinute { get; set; }
        public int EndHour { get; set; }
        public int EndMinute { get; set; }
        public int[] DaysOfWeek { get; set; }
    }
}
