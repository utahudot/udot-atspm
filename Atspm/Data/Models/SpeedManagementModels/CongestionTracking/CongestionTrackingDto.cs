﻿using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Common;

namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.CongestionTracking
{
    public class CongestionTrackingDto
    {
        public Guid SegmentId { get; set; }
        public string SegmentName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double StartingMilePoint { get; set; }
        public double EndingMilePoint { get; set; }
        public long SpeedLimit { get; set; }
        public List<SpeedDataDto> Data { get; set; }
    }
}