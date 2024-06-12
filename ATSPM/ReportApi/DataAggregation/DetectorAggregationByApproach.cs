#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - MOE.Common.Business.DataAggregation/DetectorAggregationByApproach.cs
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