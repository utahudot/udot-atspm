using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;

namespace ATSPM.ReportApi.Business.ApproachSpeed
{
    public class AvgSpeedBucketCollection
    {
        public List<AvgSpeedBucket> AvgSpeedBuckets = new List<AvgSpeedBucket>();

        public AvgSpeedBucketCollection(DateTime startTime, DateTime endTime, int binSize, int movementdelay,
            List<CycleSpeed> cycles)
        {
            var speedEvents = cycles.SelectMany(c => c.SpeedEvents).ToList();
            var dt = startTime;
            while (dt.AddMinutes(binSize) <= endTime)
            {
                var avg = new AvgSpeedBucket(dt, dt.AddMinutes(binSize), binSize, movementdelay, speedEvents);
                AvgSpeedBuckets.Add(avg);
                dt = dt.AddMinutes(binSize);
            }
        }


        public int GetAverageSpeed(List<SpeedEvent> speedEvents)
        {
            var TotalSpeed = 0;
            var AvgSpeed = 0;
            foreach (var speed in speedEvents)
                TotalSpeed = TotalSpeed + speed.Mph;
            double RawAvgSpeed = TotalSpeed / speedEvents.Count;
            AvgSpeed = Convert.ToInt32(Math.Round(RawAvgSpeed));
            return AvgSpeed;
        }
    }
}