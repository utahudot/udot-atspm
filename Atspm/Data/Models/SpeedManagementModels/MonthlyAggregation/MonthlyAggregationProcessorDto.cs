namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.MonthlyAggregation
{
    public class MonthlyAggregationProcessorDto
    {
        public List<HourlySpeed> hourlySpeeds { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public Guid SegmentId { get; set; }
        public MonthlyAggregation monthlyAggregation { get; set; }
    }
}