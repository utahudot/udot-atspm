
namespace ATSPM.Data.Models.SpeedManagement.CongestionTracking
{
    public class CongestionSeriesData
    {
        public List<DataPoint<double>> Average {  get; set; }
        public List<DataPoint<int>> EightyFifth {  get; set; }
    }
}
