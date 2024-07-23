using ATSPM.Data.Models.EventLogModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.ApproachSpeed
{
    public class AvgSpeedBucketCollection
    {
        private readonly ILogger logger;

        public List<AvgSpeedBucket> AvgSpeedBuckets { get; set; }

        public AvgSpeedBucketCollection(DateTime startTime, DateTime endTime, int binSize, int movementdelay,
            List<CycleSpeed> cycles, ILogger logger)
        {
            AvgSpeedBuckets = new List<AvgSpeedBucket>();
            var speedEvents = cycles.SelectMany(c => c.SpeedEvents).ToList();
            var dt = startTime;
            while (dt.AddMinutes(binSize) <= endTime)
            {
                var avg = new AvgSpeedBucket(dt, dt.AddMinutes(binSize), binSize, movementdelay, speedEvents, logger);
                AvgSpeedBuckets.Add(avg);
                dt = dt.AddMinutes(binSize);
            }

            this.logger = logger;
        }

        public static int GetAverageSpeed(List<SpeedEvent> speedEvents)
        {
            var totalSpeed = 0;
            var avgSpeed = 0;
            foreach (var speed in speedEvents)
                totalSpeed = totalSpeed + speed.Mph;
            double RawAvgSpeed = (double)totalSpeed / (double)speedEvents.Count;
            avgSpeed = Convert.ToInt32(Math.Round(RawAvgSpeed));
            return avgSpeed;
        }
    }
}