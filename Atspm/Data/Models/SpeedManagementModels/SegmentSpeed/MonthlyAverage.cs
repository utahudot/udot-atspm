﻿namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SegmentSpeed
{
    public class MonthlyAverage
    {
        public DateTime Month { get; set; }
        public double Average { get; set; }
        public double FifteenthSpeed { get; set; }
        public double EightyFifthSpeed { get; set; }
        public double NinetyFifthSpeed { get; set; }
        public double NinetyNinthSpeed { get; set; }
        public double Violation { get; set; }
        public double ExtremeViolation { get; set; }
        public double Flow { get; set; }
        public double MaxSpeed { get; set; }
        public double MinSpeed { get; set; }
    }
}
