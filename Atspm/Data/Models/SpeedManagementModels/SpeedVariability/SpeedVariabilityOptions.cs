using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Common;

namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SpeedVariability
{
    public class SpeedVariabilityOptions : OptionsBase
    {
        public string SegmentId { get; set; }
        public List<DayOfWeek> DaysOfWeek { get; set; }
        public bool? IsHolidaysFiltered { get; set; }
    }
}
