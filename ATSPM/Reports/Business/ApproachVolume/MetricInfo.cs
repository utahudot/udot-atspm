using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.ApproachVolume
{
    public class MetricInfo
    {
        public string Direction2PeakHourString { get; set; }        
        public double Direction2PeakHourDFactor { get; set; }
        public double Direction2PeakHourKFactor { get; set; }
        public double Direction2PeakHourFactor { get; set; }
        public int Direction2PeakHourMaxValue { get; set; }
        public int Direction2PeakHourVolume { get; set; }
        public string Direction1PeakHourString { get; set; }
        public double Direction1PeakHourDFactor { get; set; }
        public double Direction1PeakHourKFactor { get; set; }
        public double Direction1PeakHourFactor { get; set; }
        public int Direction1PeakHourMaxValue { get; set; }
        public int Direction1PeakHourVolume { get; set; }
        public int CombinedVolume { get; set; }
        public string CombinedPeakHourString { get; set; }
        public double CombinedPeakHourKFactor { get; set; }
        public double CombinedPeakHourFactor { get; set; }
        public int CombinedPeakHourValue { get; set; }
        public string ImageLocation { get; set; }
        public string Direction1 { get; set; }
        public string Direction2 { get; set; }
        public int Direction2Volume { get; set; }
        public int Direction1Volume { get; set; }
        public int CombinedPeakHourVolume { get; set; }
    }
}