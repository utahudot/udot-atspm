using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Common;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.MonthlyAggregation;

namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SpeedOverTime
{
    public class SpeedOverTimeOptions : OptionsBase
    {
        public TimeOptionsEnum TimeOptions { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string SegmentId { get; set; }
        public FilteringTimePeriod timePeriod { get; set; }
        public MonthAggClassification monthAggClassification { get; set; }
    }
}
