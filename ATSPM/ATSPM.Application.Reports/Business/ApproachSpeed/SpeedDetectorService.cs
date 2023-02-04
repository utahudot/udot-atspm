using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using Legacy.Common.Business;
using System;
using System.Linq;

namespace ATSPM.Application.Reports.Business.ApproachSpeed
{
    public class SpeedDetectorService
    {
        private readonly CycleService cycleService;
        private readonly ISpeedEventRepository speedEventRepository;
        private readonly PlanService planService;

        public SpeedDetectorService(
            CycleService cycleService,
            ISpeedEventRepository speedEventRepository,
            PlanService planService)
        {
            this.cycleService = cycleService;
            this.speedEventRepository = speedEventRepository;
            this.planService = planService;
        }

        public SpeedDetector GetSpeedDetector(Data.Models.Detector detector, DateTime start, DateTime end, int binSize,
            bool getPermissivePhase)
        {
            var cycles = cycleService.GetSpeedCycles(start, end, getPermissivePhase, detector);
            var speedEvents = speedEventRepository.GetSpeedEventsByDetector(
                detector,
                start,
                end,
                detector.MinSpeedFilter ?? 5).ToList();
            if (cycles.Any())
            {
                foreach (var cycle in cycles)
                    cycle.FindSpeedEventsForCycle(speedEvents);
            }

            var totalDetectorHits = cycles.Sum(c => c.SpeedEvents.Count);
            var plans = planService.GetSpeedPlans(cycles, start, end, detector.Approach);
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