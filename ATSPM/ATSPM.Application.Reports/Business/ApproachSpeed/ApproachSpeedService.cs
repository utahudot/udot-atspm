using ATSPM.Application.Reports.ViewModels.ApproachSpeed;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using ATSPM.Application.Extensions;
using System.Collections.Generic;
using static Legacy.Common.Business.Bins.BinFactoryOptions;
using System;
using ATSPM.Application.Reports.Business.Common;

namespace ATSPM.Application.Reports.Business.ApproachSpeed
{
    public class ApproachSpeedService
    {
        private readonly SpeedDetectorService speedDetectorService;
        private readonly IDetectorRepository detectorRepository;

        public ApproachSpeedService(
            SpeedDetectorService speedDetectorService,
            IDetectorRepository detectorRepository
            )
        {
            this.speedDetectorService = speedDetectorService;
            this.detectorRepository = detectorRepository;
        }

        public ApproachSpeedResult GetChartData(ApproachSpeedOptions options)
        {
            var detector = detectorRepository.Lookup(options.DetectorId);
            var speedDetector = speedDetectorService.GetSpeedDetector(detector, options.StartDate, options.EndDate, options.SelectedBinSize, false);
            var averageSpeeds = new List<AverageSpeeds>();
            var plans = new List<SpeedPlan>();
            var eightyFifthSpeeds = new List<EightyFifthSpeeds>();
            var fifteenthSpeeds = new List<FifteenthSpeeds>();
            foreach (var bucket in speedDetector.AvgSpeedBucketCollection.AvgSpeedBuckets)
            {
                if (options.ShowAverageSpeed)
                    averageSpeeds.Add(new AverageSpeeds(bucket.StartTime, bucket.AvgSpeed));
                if (options.Show85Percentile)
                    eightyFifthSpeeds.Add(new EightyFifthSpeeds(bucket.StartTime, bucket.EightyFifth));
                if (options.Show15Percentile)
                    fifteenthSpeeds.Add(new FifteenthSpeeds(bucket.StartTime, bucket.FifteenthPercentile));
            }
            if (options.ShowPlanStatistics)
                plans = speedDetector.Plans;
            return new ApproachSpeedResult(
                    "Approach Speed",
                    detector.Approach.SignalId,
                    detector.Approach.Signal.SignalDescription(),
                    detector.Approach.ProtectedPhaseNumber,
                    detector.Approach.Description,
                    options.StartDate, 
                    options.EndDate,
                    detector.DetectionTypes.ToString(),
                    detector.DistanceFromStopBar.Value,
                    detector.Approach.Mph.Value,
                    speedDetector.Plans,
                    averageSpeeds,
                    eightyFifthSpeeds,
                    fifteenthSpeeds
                );
        }
    }
}
