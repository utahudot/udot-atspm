using ATSPM.Data.Models.EventLogModels;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.ApproachSpeed
{
    public class AvgSpeedBucket
    {
        private readonly ILogger logger;

        public AvgSpeedBucket(DateTime startTime, DateTime endTime, int binSize, int movementdelay,
            List<SpeedEvent> speedEvents, ILogger logger)
        {
            StartTime = startTime;
            EndTime = endTime;
            XAxis = startTime;
            MovementDelay = movementdelay;
            this.logger = logger;
            var speedsForBucket = new List<int>();
            speedsForBucket.AddRange(speedEvents.Where(s => s.Timestamp >= startTime && s.Timestamp < endTime).Select(s => s.Mph));

            if (!speedsForBucket.IsNullOrEmpty())
            {
                speedsForBucket.Sort();
                SpeedVolume = speedsForBucket.Count;
                SummedSpeed = speedsForBucket.Sum();
                AvgSpeed = Convert.ToInt32(Math.Round(speedsForBucket.Average()));
                EightyFifth = GetPercentile(speedsForBucket, .85);
                FifteenthPercentile = GetPercentile(speedsForBucket, .15);
            }
            else
            {
                SpeedVolume = 0;
                SummedSpeed = 0;
                AvgSpeed = 0;
                EightyFifth = 0;
                FifteenthPercentile = 0;
            }
        }

        public DateTime XAxis { get; set; }

        public DateTime TotalMph { get; set; }

        public DateTime StartTime { get; }

        public DateTime EndTime { get; }

        public int AvgSpeed { get; }

        public int EightyFifth { get; }

        public int FifteenthPercentile { get; }


        public int MovementDelay { get; }

        public int SummedSpeed { get; }
        public int SpeedVolume { get; }

        private int GetPercentile(List<int> speeds, double percentile)
        {
            var percentileValue = 0;
            try
            {
                var tempPercentileIndex = SpeedVolume * percentile - 1;
                var percentileIndex = 0;
                if (SpeedVolume > 3)
                {

                    percentileIndex = Convert.ToInt32(Math.Round(tempPercentileIndex + .5));
                    percentileValue = speeds[percentileIndex];
                }
                else
                {
                    percentileIndex = Convert.ToInt32(tempPercentileIndex);
                    var speed1 = (double)speeds[percentileIndex];
                    var speed2 = (double)speeds[percentileIndex + 1];
                    double rawPercentile = (speed1 + speed2) / (double)2;
                    percentileValue = Convert.ToInt32(Math.Round(rawPercentile));
                }
            }
            catch
            {
                logger.LogError("Error creating Percentile");
            }
            return percentileValue;
        }
    }
}