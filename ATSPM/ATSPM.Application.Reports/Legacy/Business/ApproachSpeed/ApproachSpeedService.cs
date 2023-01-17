using ATSPM.Application.Reports.ViewModels.ApproachSpeed;
using ATSPM.Data.Models;
using ATSPM.Application.Extensions;
using System.Collections.Generic;
using static Legacy.Common.Business.Bins.BinFactoryOptions;
using System;
using Legacy.Common.Business.WCFServiceLibrary;

namespace ATSPM.Application.Reports.Legacy.Business.ApproachSpeed
{
    public class ApproachSpeedService
    {
        private readonly SpeedDetectorService speedDetectorService;

        public ApproachSpeedService(SpeedDetectorService speedDetectorService)
        {
            this.speedDetectorService = speedDetectorService;
        }

        public ApproachSpeedResult GetChartData(DateTime start, DateTime end, int binSize, Data.Models.Detector detector, ApproachSpeedOptions options)
        {
            var speedDetector = speedDetectorService.GetSpeedDetector(detector, start, end, binSize, false);
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
                    start,
                    end,
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
