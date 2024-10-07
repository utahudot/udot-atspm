namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SegmentSpeed
{
    public class HistoricalSpeedOptions
    {
        public Guid SegmentId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
    }
}
