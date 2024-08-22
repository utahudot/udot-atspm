using ATSPM.Data.Models.SpeedManagement.Common;

namespace ATSPM.Data.Models.SpeedManagement.SpeedOverTime
{
    public class SpeedOverTimeOptions : OptionsBase
    {
        public TimeOptionsEnum TimeOptions {  get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string SegmentId { get; set; }
    }
}
