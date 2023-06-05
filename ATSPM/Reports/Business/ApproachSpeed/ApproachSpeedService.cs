using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using ATSPM.Application.Extensions;
using System.Collections.Generic;
using static Legacy.Common.Business.Bins.BinFactoryOptions;
using System;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Infrastructure.Repositories;
using System.Linq;

namespace ATSPM.Application.Reports.Business.ApproachSpeed
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
            List<ControllerEventLog> cycleEvents,
            List<ControllerEventLog> planEvents,
            List<SpeedEvent> speedEvents,
            Detector detector)        
        {
            var speedDetector = GetSpeedDetector(
                detector,
                options.Start,
                options.End,
                options.SelectedBinSize,
                false,
                planEvents,
                cycleEvents,
                speedEvents);
            var averageSpeeds = new List<AverageSpeeds>();
            var eightyFifthSpeeds = new List<EightyFifthSpeeds>();
            var fifteenthSpeeds = new List<FifteenthSpeeds>();
            foreach (var bucket in speedDetector.AvgSpeedBucketCollection.AvgSpeedBuckets)
            {
                averageSpeeds.Add(new AverageSpeeds(bucket.StartTime, bucket.AvgSpeed));
                eightyFifthSpeeds.Add(new EightyFifthSpeeds(bucket.StartTime, bucket.EightyFifth));
                fifteenthSpeeds.Add(new FifteenthSpeeds(bucket.StartTime, bucket.FifteenthPercentile));
            }
            return new ApproachSpeedResult(
                    detector.Approach.SignalId,
                    detector.ApproachId,
                    detector.Approach.ProtectedPhaseNumber,
                    detector.Approach.Description,
                    options.Start, 
                    options.End,
                    detector.DetectionTypes.ToString(),
                    detector.DistanceFromStopBar.Value,
                    detector.Approach.Mph.Value,
                    speedDetector.Plans,
                    averageSpeeds,
                    eightyFifthSpeeds,
                    fifteenthSpeeds
                );
        }

        public SpeedDetector GetSpeedDetector(Detector detector, DateTime start, DateTime end, int binSize,
           bool getPermissivePhase, List<ControllerEventLog> planEvents, List<ControllerEventLog> cycleEvents, List<SpeedEvent> speedEvents)
        {
            var cycles = cycleService.GetSpeedCycles(start, end, cycleEvents);
            if (cycles.Any())
            {
                foreach (var cycle in cycles)
                    cycle.FindSpeedEventsForCycle(speedEvents);
            }

            var totalDetectorHits = cycles.Sum(c => c.SpeedEvents.Count);
            var plans = planService.GetSpeedPlans(cycles, start, end, detector.Approach, planEvents);
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
                speedEvents,
                avgSpeedBucketCollection
                );
        }
    }
}
