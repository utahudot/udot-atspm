#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - MOE.Common.Business.DataAggregation/DetectorAggregationBySignal.cs
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
using ATSPM.Application.Business.Aggregation;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using MOE.Common.Business.WCFServiceLibrary;

namespace MOE.Common.Business.DataAggregation
{
    public class DetectorAggregationBySignal : AggregationBySignal
    {
        private readonly IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository;

        public DetectorAggregationBySignal(
            DetectorVolumeAggregationOptions detectorVolumeAggregationOptions,
            Location signal,
            IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository,
            AggregationOptions options
            ) : base(
            detectorVolumeAggregationOptions, signal, options)
        {
            this.detectorEventCountAggregationRepository = detectorEventCountAggregationRepository;
            ApproachDetectorVolumes = new List<DetectorAggregationByApproach>();
            GetApproachDetectorVolumeAggregationContainersForAllApporaches(detectorVolumeAggregationOptions, signal, options);
            LoadBins(null, null, options);
        }

        public DetectorAggregationBySignal(
            DetectorVolumeAggregationOptions detectorVolumeAggregationOptions,
            Location signal,
            int phaseNumber,
            IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository,
            AggregationOptions options
            ) : base(detectorVolumeAggregationOptions, signal, options)
        {
            this.detectorEventCountAggregationRepository = detectorEventCountAggregationRepository;
            ApproachDetectorVolumes = new List<DetectorAggregationByApproach>();
            foreach (var approach in signal.Approaches)
                if (approach.ProtectedPhaseNumber == phaseNumber)
                {
                    ApproachDetectorVolumes.Add(
                        new DetectorAggregationByApproach(
                            approach,
                            detectorVolumeAggregationOptions,
                            true,
                            detectorEventCountAggregationRepository,
                            options
                            ));
                    if (approach.PermissivePhaseNumber != null && approach.PermissivePhaseNumber == phaseNumber)
                        ApproachDetectorVolumes.Add(new DetectorAggregationByApproach(
                            approach,
                            detectorVolumeAggregationOptions,
                            false,
                            detectorEventCountAggregationRepository,
                            options
                            ));
                }
            LoadBins(null, null, options);
        }

        public DetectorAggregationBySignal(
            DetectorVolumeAggregationOptions detectorVolumeAggregationOptions,
            Location signal,
            DirectionTypes direction,
            IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository,
            AggregationOptions options
            ) : base(detectorVolumeAggregationOptions, signal, options)
        {
            this.detectorEventCountAggregationRepository = detectorEventCountAggregationRepository;
            ApproachDetectorVolumes = new List<DetectorAggregationByApproach>();
            foreach (var approach in signal.Approaches)
                if (approach.DirectionType.Id == direction)
                {
                    ApproachDetectorVolumes.Add(
                        new DetectorAggregationByApproach(
                            approach,
                            detectorVolumeAggregationOptions,
                            true,
                            detectorEventCountAggregationRepository,
                            options
                            ));
                    if (approach.PermissivePhaseNumber != null)
                        ApproachDetectorVolumes.Add(
                            new DetectorAggregationByApproach(
                                approach,
                                detectorVolumeAggregationOptions,
                                false,
                                detectorEventCountAggregationRepository,
                                options
                                ));
                }
            LoadBins(null, null, options);
        }

        public List<DetectorAggregationByApproach> ApproachDetectorVolumes { get; }

        protected override void LoadBins(SignalAggregationMetricOptions signalAggregationMetricOptions, Location signal, AggregationOptions options)
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

        protected override void LoadBins(ApproachAggregationMetricOptions approachAggregationMetricOptions, Location signal, AggregationOptions options)
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
            DetectorVolumeAggregationOptions detectorVolumeAggregationOptions, Location signal, AggregationOptions options)
        {
            foreach (var approach in signal.Approaches)
            {
                ApproachDetectorVolumes.Add(
                    new DetectorAggregationByApproach(
                        approach,
                        detectorVolumeAggregationOptions,
                        true,
                        detectorEventCountAggregationRepository,
                        options
                        ));
                if (approach.PermissivePhaseNumber != null)
                    ApproachDetectorVolumes.Add(
                        new DetectorAggregationByApproach(
                            approach,
                            detectorVolumeAggregationOptions,
                            false,
                            detectorEventCountAggregationRepository,
                            options
                            ));
            }
        }
    }
}