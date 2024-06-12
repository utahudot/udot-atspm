#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.ApproachSpeed/ApproachSpeedService.cs
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
using ATSPM.Application.Business.Common;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.ApproachSpeed
{
    public class ApproachSpeedService
    {
        private readonly CycleService cycleService;
        private readonly PlanService planService;

        public ApproachSpeedService(
            CycleService cycleService,
            PlanService planService
            )
        {
            this.cycleService = cycleService;
            this.planService = planService;
        }

        public ApproachSpeedResult GetChartData(
            ApproachSpeedOptions options,
            List<IndianaEvent> cycleEvents,
            List<IndianaEvent> planEvents,
            List<SpeedEvent> speedEvents,
            Detector detector)
        {
            var speedEventsForDetector = speedEvents.Where(d => d.DetectorId == detector.DectectorIdentifier && d.Timestamp >= options.Start && d.Timestamp < options.End).ToList();
            var speedDetector = GetSpeedDetector(
                detector,
                options.Start,
                options.End,
                options.BinSize,
                planEvents,
                cycleEvents,
                speedEventsForDetector);
            if (speedEvents.IsNullOrEmpty())
            {
                new ApproachSpeedResult(
                   detector.Approach.Location.LocationIdentifier,
                   detector.ApproachId,
                   detector.Approach.ProtectedPhaseNumber,
                   detector.Approach.Description,
                   options.Start,
                   options.End,
                   detector.DetectionTypes.FirstOrDefault(d => d.MeasureTypes.Any(m => m.Id == options.MetricTypeId)).Description,
                   detector.DistanceFromStopBar.Value,
                   detector.Approach.Mph.Value,
                   speedDetector.Plans,
                   null,
                   null,
                   null
                   );
            }
            var averageSpeeds = new List<DataPointForInt>();
            var eightyFifthSpeeds = new List<DataPointForInt>();
            var fifteenthSpeeds = new List<DataPointForInt>();
            if (speedDetector.AvgSpeedBucketCollection != null)
            {
                foreach (var bucket in speedDetector.AvgSpeedBucketCollection.AvgSpeedBuckets)
                {
                    averageSpeeds.Add(new DataPointForInt(bucket.StartTime, bucket.AvgSpeed));
                    eightyFifthSpeeds.Add(new DataPointForInt(bucket.StartTime, bucket.EightyFifth));
                    fifteenthSpeeds.Add(new DataPointForInt(bucket.StartTime, bucket.FifteenthPercentile));
                }
            }
            return new ApproachSpeedResult(
                    detector.Approach.Location.LocationIdentifier,
                    detector.ApproachId,
                    detector.Approach.ProtectedPhaseNumber,
                    detector.Approach.Description,
                    options.Start,
                    options.End,
                    detector.DetectionTypes.FirstOrDefault(d => d.MeasureTypes.Any(m => m.Id == options.MetricTypeId)).Description,
                    detector.DistanceFromStopBar.Value,
                    detector.Approach.Mph.Value,
                    speedDetector.Plans,
                    averageSpeeds,
                    eightyFifthSpeeds,
                    fifteenthSpeeds
                );
        }

        public SpeedDetector GetSpeedDetector(
            Detector detector,
            DateTime start,
            DateTime end,
            int binSize,
            List<IndianaEvent> planEvents,
            List<IndianaEvent> cycleEvents,
            List<SpeedEvent> speedEventsForDetector)
        {
            var cycles = cycleService.GetSpeedCycles(start, end, cycleEvents);
            if (cycles.Any())
            {
                foreach (var cycle in cycles)
                    cycle.FindSpeedEventsForCycle(speedEventsForDetector);
            }
            var plans = planService.GetSpeedPlans(cycles, start, end, detector.Approach, planEvents);
            if (speedEventsForDetector.IsNullOrEmpty())
            {
                return new SpeedDetector(
                    plans,
                    0,
                    start,
                    end,
                    cycles,
                    null,
                    null
                );
            }

            var totalDetectorHits = cycles.Sum(c => c.SpeedEvents.Count);
            var movementDelay = 0;
            if (detector.MovementDelay != null)
                movementDelay = detector.MovementDelay.Value;
            var avgSpeedBucketCollection = new AvgSpeedBucketCollection(start, end, binSize, movementDelay, cycles);
            return new SpeedDetector(
                plans,
                totalDetectorHits,
                start,
                end,
                cycles,
                speedEventsForDetector,
                avgSpeedBucketCollection
                );
        }
    }
}
