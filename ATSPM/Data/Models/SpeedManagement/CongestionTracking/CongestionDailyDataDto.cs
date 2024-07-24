
namespace ATSPM.Data.Models.SpeedManagement.CongestionTracking
{
    public class CongestionDailyDataDto
    {
        public DateTime Date { get; set; }
        public CongestionSeriesData Series {  get; set; }
    }
}
