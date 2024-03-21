using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using MOE.Common.Business.WCFServiceLibrary;

namespace MOE.Common.Business.DataAggregation
{
    public class DetectorAggregationBySignal : AggregationBySignal
    {
        private readonly IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository;

        public DetectorAggregationBySignal(DetectorVolumeAggregationOptions options, Location signal, IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository) : base(
            options, signal)
        {
            ApproachDetectorVolumes = new List<DetectorAggregationByApproach>();
            GetApproachDetectorVolumeAggregationContainersForAllApporaches(options, signal);
            LoadBins(null, null);
            this.detectorEventCountAggregationRepository = detectorEventCountAggregationRepository;
        }

        public DetectorAggregationBySignal(DetectorVolumeAggregationOptions options, Location signal,
            int phaseNumber, IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository) : base(options, signal)
        {
            this.detectorEventCountAggregationRepository = detectorEventCountAggregationRepository;
            ApproachDetectorVolumes = new List<DetectorAggregationByApproach>();
            foreach (var approach in signal.Approaches)
                if (approach.ProtectedPhaseNumber == phaseNumber)
                {
                    ApproachDetectorVolumes.Add(
                        new DetectorAggregationByApproach(approach, options, true, detectorEventCountAggregationRepository));
                    if (approach.PermissivePhaseNumber != null && approach.PermissivePhaseNumber == phaseNumber)
                        ApproachDetectorVolumes.Add(new DetectorAggregationByApproach(approach, options, false, detectorEventCountAggregationRepository));
                }
            LoadBins(null, null);
        }

        public DetectorAggregationBySignal(DetectorVolumeAggregationOptions options, Location signal,
            DirectionTypes direction, IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository) : base(options, signal)
        {
            this.detectorEventCountAggregationRepository = detectorEventCountAggregationRepository;
            ApproachDetectorVolumes = new List<DetectorAggregationByApproach>();
            foreach (var approach in signal.Approaches)
                if (approach.DirectionType.Id == direction)
                {
                    ApproachDetectorVolumes.Add(
                        new DetectorAggregationByApproach(approach, options, true, detectorEventCountAggregationRepository));
                    if (approach.PermissivePhaseNumber != null)
                        ApproachDetectorVolumes.Add(
                            new DetectorAggregationByApproach(approach, options, false, detectorEventCountAggregationRepository));
                }
            LoadBins(null, null);
        }

        public List<DetectorAggregationByApproach> ApproachDetectorVolumes { get; }

        protected override void LoadBins(SignalAggregationMetricOptions options, Location signal)
        {
            for (var i = 0; i < BinsContainers.Count; i++)
                for (var binIndex = 0; binIndex < BinsContainers[i].Bins.Count; binIndex++)
                {
                    var bin = BinsContainers[i].Bins[binIndex];
                    foreach (var detectorEventAggregationContainer in ApproachDetectorVolumes)
                    {
                        bin.Sum += detectorEventAggregationContainer.BinsContainers[i].Bins[binIndex].Sum;
                    }
                    bin.Average = ApproachDetectorVolumes.Count > 0 ? bin.Sum / ApproachDetectorVolumes.Count : 0;
                }
        }

        protected override void LoadBins(ApproachAggregationMetricOptions options, Location signal)
        {
            for (var i = 0; i < BinsContainers.Count; i++)
                for (var binIndex = 0; binIndex < BinsContainers[i].Bins.Count; binIndex++)
                {
                    var bin = BinsContainers[i].Bins[binIndex];
                    foreach (var detectorAggregationContainer in ApproachDetectorVolumes)
                    {
                        if (detectorAggregationContainer.BinsContainers.Count > 0)
                        {
                            bin.Sum += detectorAggregationContainer.BinsContainers[i].Bins[binIndex].Sum;
                        }
                    }
                    bin.Average = ApproachDetectorVolumes.Count > 0 ? bin.Sum / ApproachDetectorVolumes.Count : 0;
                }
        }

        private void GetApproachDetectorVolumeAggregationContainersForAllApporaches(
            DetectorVolumeAggregationOptions options, Location signal)
        {
            foreach (var approach in signal.Approaches)
            {
                ApproachDetectorVolumes.Add(
                    new DetectorAggregationByApproach(approach, options, true, detectorEventCountAggregationRepository));
                if (approach.PermissivePhaseNumber != null)
                    ApproachDetectorVolumes.Add(
                        new DetectorAggregationByApproach(approach, options, false, detectorEventCountAggregationRepository));
            }
        }
    }
}