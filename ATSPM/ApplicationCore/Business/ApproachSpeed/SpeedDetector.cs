using System;
using System.Collections.Generic;
using ATSPM.Data.Models;

namespace ATSPM.ReportApi.Business.ApproachSpeed
{
    public class SpeedDetector
    {

        public SpeedDetector(
            List<SpeedPlan> plans,
            int totalDetectorHits,
            DateTime startDate,
            DateTime endDate,
            List<CycleSpeed> cycles,
            List<SpeedEvent> speedEvents,
            AvgSpeedBucketCollection avgSpeedBucketCollection)
        {
            Plans = plans;
            TotalDetectorHits = totalDetectorHits;
            StartDate = startDate;
            EndDate = endDate;
            Cycles = cycles;
            SpeedEvents = speedEvents;
            AvgSpeedBucketCollection = avgSpeedBucketCollection;
        }

        public List<SpeedPlan> Plans { get; set; }
        public int TotalDetectorHits { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<CycleSpeed> Cycles { get; set; }
        public List<SpeedEvent> SpeedEvents { get; set; }
        public AvgSpeedBucketCollection AvgSpeedBucketCollection { get; set; }
    }
}