using ATSPM.Application.Business.Aggregation;
using ATSPM.Application.Business.Bins;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using MOE.Common.Business.DataAggregation;
using System.Collections.Concurrent;

namespace MOE.Common.Business.WCFServiceLibrary
{
    public enum DetectorVolumeDataTypes { DetectorActivationCount }

    //AggregatedDataTypes = new List<AggregatedDataType>
    //{
    //    new AggregatedDataType { Id = 0, DataName = "DetectorActivationCount" }
    //};

    public class DetectorVolumeAggregationOptions : DetectorAggregationMetricOptions
    {
        private readonly IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository;

        public DetectorVolumeAggregationOptions(
            IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository,
            ILocationRepository locationRepository,
            ILogger<DetectorVolumeAggregationOptions> logger) : base(locationRepository, logger, detectorEventCountAggregationRepository)
        {
            this.detectorEventCountAggregationRepository = detectorEventCountAggregationRepository;
        }

        protected override int GetAverageByPhaseNumber(Location signal, int phaseNumber, AggregationOptions options)
        {
            var detectorAggregationBySignal =
                new DetectorAggregationBySignal(this, signal, detectorEventCountAggregationRepository, options);
            return detectorAggregationBySignal.Average;
        }

        protected override double GetSumByPhaseNumber(Location signal, int phaseNumber, AggregationOptions options)
        {
            var detectorAggregationBySignal =
                new DetectorAggregationBySignal(this, signal, detectorEventCountAggregationRepository, options);
            return detectorAggregationBySignal.Total;
        }

        protected override int GetAverageByDirection(Location signal, DirectionTypes direction, AggregationOptions options)
        {
            var detectorAggregationBySignal =
                new DetectorAggregationBySignal(this, signal, direction, detectorEventCountAggregationRepository, options);
            return detectorAggregationBySignal.Average;
        }

        protected override double GetSumByDirection(Location signal, DirectionTypes direction, AggregationOptions options)
        {
            var detectorAggregationByDetector =
                new DetectorAggregationBySignal(this, signal, direction, detectorEventCountAggregationRepository, options);
            return detectorAggregationByDetector.Total;
        }

        protected override List<BinsContainer> GetBinsContainersBySignal(Location signal, AggregationOptions options)
        {
            var detectorAggregationByDetector = new DetectorAggregationBySignal(this, signal, detectorEventCountAggregationRepository, options);
            return detectorAggregationByDetector.BinsContainers;
        }

        protected override List<BinsContainer> GetBinsContainersByDirection(DirectionTypes directionType,
            Location signal, AggregationOptions options)
        {
            var detectorAggregationBySignal =
                new DetectorAggregationBySignal(this, signal, directionType, detectorEventCountAggregationRepository, options);
            return detectorAggregationBySignal.BinsContainers;
        }

        protected override List<BinsContainer> GetBinsContainersByPhaseNumber(Location signal, int phaseNumber, AggregationOptions options)
        {
            var detectorAggregationBySignal =
                new DetectorAggregationBySignal(this, signal, phaseNumber, detectorEventCountAggregationRepository, options);
            return detectorAggregationBySignal.BinsContainers;
        }

        public override List<BinsContainer> GetBinsContainersByRoute(List<Location> signals, AggregationOptions options)
        {
            var aggregations = new ConcurrentBag<DetectorAggregationBySignal>();
            Parallel.ForEach(signals, signal => { aggregations.Add(new DetectorAggregationBySignal(this, signal, detectorEventCountAggregationRepository, options)); });
            var binsContainers = BinFactory.GetBins(options.TimeOptions);
            foreach (var splitFailAggregationBySignal in aggregations)
                for (var i = 0; i < binsContainers.Count; i++)
                    for (var binIndex = 0; binIndex < binsContainers[i].Bins.Count; binIndex++)
                    {
                        var bin = binsContainers[i].Bins[binIndex];
                        bin.Sum += splitFailAggregationBySignal.BinsContainers[i].Bins[binIndex].Sum;
                        bin.Average = Convert.ToInt32(Math.Round((double)(bin.Sum / signals.Count)));
                    }
            return binsContainers;
        }

        protected override List<BinsContainer> GetBinsContainersByDetector(Detector detector, AggregationOptions options, IDetectorEventCountAggregationRepository repository)
        {
            var detectorAggregationByDetector = new DetectorAggregationByDetector(detector, this, repository, options);
            return detectorAggregationByDetector.BinsContainers;
        }


        protected override List<BinsContainer> GetBinsContainersByApproach(Approach approach, bool getprotectedPhase, AggregationOptions options)
        {
            var detectorAggregationByDetector = new DetectorAggregationByApproach(approach, this, getprotectedPhase, detectorEventCountAggregationRepository, options);
            return detectorAggregationByDetector.BinsContainers;
        }
    }
}