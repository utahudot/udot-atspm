﻿using ATSPM.Application.Business.Common;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using Microsoft.Extensions.Logging;
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
            Detector detector,
            ILogger logger)
        {
            var speedEventsForDetector = speedEvents.Where(d => d.DetectorId == detector.DectectorIdentifier && d.Timestamp >= options.Start && d.Timestamp < options.End).ToList();
            var speedDetector = GetSpeedDetector(
                detector,
                options.Start,
                options.End,
                options.BinSize,
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
                    ? "Detection Type Not Found"
                    : detector.DetectionTypes.FirstOrDefault(d => d.MeasureTypes.Any(m => m.Id == options.MetricTypeId))?.Description ?? "Detection Type Not Found",
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
                    detector.DetectionTypes.IsNullOrEmpty()
                    ? "Detection Type Not Found"
                    : detector.DetectionTypes.FirstOrDefault(d => d.MeasureTypes.Any(m => m.Id == options.MetricTypeId))?.Description ?? "Detection Type Not Found",
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
            List<SpeedEvent> speedEventsForDetector,
            ILogger logger)
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
            var avgSpeedBucketCollection = new AvgSpeedBucketCollection(start, end, binSize, movementDelay, cycles, logger);
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
