﻿using ATSPM.Application.Business.Aggregation;
using ATSPM.Application.Business.Bins;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data.Models;
using ATSPM.Data.Models.AggregationModels;
using MOE.Common.Business.WCFServiceLibrary;

namespace MOE.Common.Business.DataAggregation
{
    public abstract class AggregationByDetector
    {
        protected readonly IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository;

        protected List<DetectorEventCountAggregation> DetectorEventCountAggregations { get; set; }
        protected AggregationByDetector(
            Detector detector,
            DetectorAggregationMetricOptions detectorAggregationMetricOptions,
            IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository,
            AggregationOptions options)
        {
            Detector = detector;
            this.detectorEventCountAggregationRepository = detectorEventCountAggregationRepository;
            BinsContainers = BinFactory.GetBins(options.TimeOptions);
            LoadBins(detector, detectorAggregationMetricOptions, options);
        }


        public Detector Detector { get; }
        public List<BinsContainer> BinsContainers { get; set; }

        protected List<DetectorEventCountAggregation> GetdetectorEventCountAggregations(
            DetectorAggregationMetricOptions detectorAggregationMetricOptions,
            Detector detector,
            AggregationOptions options)
        {
            return detectorEventCountAggregationRepository.GetAggregationsBetweenDates(
                detector.Approach.Location.LocationIdentifier,
                options.Start,
                options.End).Where(d => d.DetectorPrimaryId == detector.Id).ToList();
        }

        public abstract void LoadBins(Detector detector, DetectorAggregationMetricOptions detectorAggregationMetricOptions, AggregationOptions options);
    }
}