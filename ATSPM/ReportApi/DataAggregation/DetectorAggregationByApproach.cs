using ATSPM.Application.Business.Aggregation;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data.Models;
using MOE.Common.Business.WCFServiceLibrary;

namespace MOE.Common.Business.DataAggregation
{
    public class DetectorAggregationByApproach : AggregationByApproach
    {
        protected readonly IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository;

        public DetectorAggregationByApproach(
            Approach approach,
            DetectorVolumeAggregationOptions detectorVolumeAggregationOptions,
            bool getProtectedPhase,
            IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository,
            AggregationOptions options) : base(approach, detectorVolumeAggregationOptions, options.Start, options.End,
            getProtectedPhase, options.DataType, options)
        {
            this.detectorEventCountAggregationRepository = detectorEventCountAggregationRepository;
            GetApproachDetectorVolumeAggregationContainersForAllDetectors(detectorVolumeAggregationOptions, approach, options);
            LoadBins(
                approach,
                detectorVolumeAggregationOptions,
                getProtectedPhase,
                options.DataType,
                options);
        }

        public List<DetectorAggregationByDetector> DetectorAggregationByDetectors { get; set; } =
            new List<DetectorAggregationByDetector>();


        private void GetApproachDetectorVolumeAggregationContainersForAllDetectors(
            DetectorVolumeAggregationOptions detectorVolumeAggregationOptions, Approach approach, AggregationOptions options)
        {
            foreach (var detector in approach.Detectors)
                DetectorAggregationByDetectors.Add(new DetectorAggregationByDetector(detector, detectorVolumeAggregationOptions, detectorEventCountAggregationRepository, options));
        }

        protected override void LoadBins(
            Approach approach,
            ApproachAggregationMetricOptions approachAggregationMetricOptions,
            bool getProtectedPhase,
            int dataType,
            AggregationOptions options)
        {
            //var bins = DetectorAggregationByDetectors.SelectMany(b => b.BinsContainers.SelectMany(bc => bc.Bins)).ToList().GroupBy(g => g.Start, g => g.Sum, (key, a)=> new(Start = key, Sum = a.Sum().ToList();
            //var sum = bins.Sum(b => b.Sum);
            for (var i = 0; i < BinsContainers.Count; i++)
            {
                for (var binIndex = 0; binIndex < BinsContainers[i].Bins.Count; binIndex++)
                {
                    var bin = BinsContainers[i].Bins[binIndex];
                    foreach (var detectorAggregationByDetector in DetectorAggregationByDetectors)
                        bin.Sum += detectorAggregationByDetector.BinsContainers[i].Bins[binIndex].Sum;
                    bin.Average = DetectorAggregationByDetectors.Count > 0
                        ? bin.Sum / DetectorAggregationByDetectors.Count
                        : 0;
                }
            }
        }
    }
}