namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Importer
{
    public class HourlyStatistics
    {
        public int Hour { get; set; }
        public int? Violations { get; set; }
        public int? ExtremeViolations { get; set; }
        public int Flow { get; set; }
        public List<Double> WeightedSpeeds { get; set; }
        public int SpeedFlowMismatches { get; set; }
        public int TotalBins { get; set; }
        public bool SourceDataAnalyzed { get; set; }
        //public double TotalFlowSpeedProduct { get; set; }
    }
    public class DayStatistics
    {
        public DateTime Date { get; set; }
        public long SpeedLimit { get; set; }
        public List<HourlyStatistics> HourlyStatistics { get; set; } = new List<HourlyStatistics>();

    }
}