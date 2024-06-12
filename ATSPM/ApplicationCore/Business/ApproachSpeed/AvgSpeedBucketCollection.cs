#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.ApproachSpeed/AvgSpeedBucketCollection.cs
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
using ATSPM.Data.Models.EventLogModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.ApproachSpeed
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