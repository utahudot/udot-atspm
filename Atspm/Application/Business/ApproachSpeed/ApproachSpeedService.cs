#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.ApproachSpeed/ApproachSpeedService.cs
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
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Extensions;

namespace Utah.Udot.Atspm.Business.ApproachSpeed
{
    public class ApproachSpeedService
    {
        private readonly CycleService cycleService;
        private readonly PlanService planService;
        const string DetectionNotFoundMessage = "Detection Type Not Found";

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
            Detector detector,
            ILogger logger)
        {
            var speedEventsForDetector = speedEvents.Where(d => d.DetectorId == detector.DectectorIdentifier && d.Timestamp >= options.Start && d.Timestamp < options.End).ToList();
            var speedDetector = GetSpeedDetector(
                options,
                detector,
                planEvents,
                cycleEvents,
                speedEventsForDetector,
                logger);
            if (speedEvents.IsNullOrEmpty())
            {
                return new ApproachSpeedResult(
                   detector.Approach.Location.LocationIdentifier,
                   detector.ApproachId,
                   detector.Approach.ProtectedPhaseNumber,
                   detector.Approach.Description,
                   options.Start,
                   options.End,
                   detector.DetectionTypes.IsNullOrEmpty()
                    ? DetectionNotFoundMessage
                    : detector.DetectionTypes.FirstOrDefault(d => d.MeasureTypes.Any(m => m.Id == options.MetricTypeId))?.Description ?? DetectionNotFoundMessage,
                   detector.DistanceFromStopBar.Value,
                detector.Approach.Mph.Value)
                {
                    Plans = speedDetector.Plans
                };
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
                    detector.DetectionTypes.IsNullOrEmpty()
                    ? DetectionNotFoundMessage
                    : detector.DetectionTypes.FirstOrDefault(d => d.MeasureTypes.Any(m => m.Id == options.MetricTypeId))?.Description ?? DetectionNotFoundMessage,
                    detector.DistanceFromStopBar.Value,
                    detector.Approach.Mph.Value)
            {
                Plans = speedDetector.Plans,
                AverageSpeeds = averageSpeeds,
                EightyFifthSpeeds = eightyFifthSpeeds,
                FifteenthSpeeds = fifteenthSpeeds
            };
        }

        public SpeedDetector GetSpeedDetector(
            ApproachSpeedOptions options,
            Detector detector,
            List<IndianaEvent> planEvents,
            List<IndianaEvent> cycleEvents,
            List<SpeedEvent> speedEventsForDetector,
            ILogger logger)
        {
            var cycles = cycleService.GetSpeedCycles(options.Start, options.End, cycleEvents);
            if (cycles.Any())
            {
                foreach (var cycle in cycles)
                    cycle.FindSpeedEventsForCycle(speedEventsForDetector);
            }
            var plans = planService.GetSpeedPlans(cycles, options.Start, options.End, detector.Approach, planEvents);
            if (speedEventsForDetector.IsNullOrEmpty())
            {
                return new SpeedDetector(
                    plans,
                    0,
                    options.Start,
                    options.End,
                    cycles,
                    null,
                    null
                );
            }

            var totalDetectorHits = cycles.Sum(c => c.SpeedEvents.Count);
            var movementDelay = 0;
            if (detector.MovementDelay != null)
                movementDelay = detector.MovementDelay.Value;
            var avgSpeedBucketCollection = new AvgSpeedBucketCollection(options.Start, options.End, options.BinSize, movementDelay, cycles, logger);
            return new SpeedDetector(
                plans,
                totalDetectorHits,
                options.Start,
                options.End,
                cycles,
                speedEventsForDetector,
                avgSpeedBucketCollection
                );
        }
    }
}
