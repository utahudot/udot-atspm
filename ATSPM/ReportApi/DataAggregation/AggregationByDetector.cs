#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - MOE.Common.Business.DataAggregation/AggregationByDetector.cs
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