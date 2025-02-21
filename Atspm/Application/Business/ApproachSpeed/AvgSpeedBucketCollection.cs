#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.ApproachSpeed/AvgSpeedBucketCollection.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.Extensions.Logging;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Business.ApproachSpeed
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
            double RawAvgSpeed = totalSpeed / (double)speedEvents.Count;
            avgSpeed = Convert.ToInt32(Math.Round(RawAvgSpeed));
            return avgSpeed;
        }
    }
}