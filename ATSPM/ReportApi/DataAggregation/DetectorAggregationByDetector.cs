#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - MOE.Common.Business.DataAggregation/DetectorAggregationByDetector.cs
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
using System.Collections.Concurrent;

namespace MOE.Common.Business.DataAggregation
{
    public class DetectorAggregationByDetector : AggregationByDetector
    {
        public DetectorAggregationByDetector(
            Detector detector,
            DetectorVolumeAggregationOptions detectorVolumeAggregationOptions,
            IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository,
            AggregationOptions options
            ) : base(
            detector, detectorVolumeAggregationOptions, detectorEventCountAggregationRepository, options)
        {
            LoadBins(detector, detectorVolumeAggregationOptions, options);
        }

        public override void LoadBins(
            Detector detector,
            DetectorAggregationMetricOptions detectorAggregationMetricOptions,
            AggregationOptions options)
        {
            var detectorAggregations = detectorAggregationMetricOptions.detectorEventCountAggregation.GetAggregationsBetweenDates(detector.Approach.Location.LocationIdentifier, options.Start, options.End).Where(d => d.DetectorPrimaryId == detector.Id).ToList();
            BinsContainers = GetBinsContainers(detectorAggregationMetricOptions, detectorAggregations, BinsContainers, options);
        }

        public static List<BinsContainer> GetBinsContainers(
            DetectorAggregationMetricOptions detectorAggregationMetricOptions,
            List<DetectorEventCountAggregation> detectorAggregations,
            List<BinsContainer> binsContainers,
            AggregationOptions options
            )
        {
            var dataTypeEnum = (DetectorVolumeDataTypes)options.DataType;
            var concurrentBinContainers = new ConcurrentBag<BinsContainer>();
            if (detectorAggregations != null)
            {
                //foreach (var binsContainer in binsContainers)
                Parallel.ForEach(binsContainers, binsContainer =>
                {
                    var tempBinsContainer =
                        new BinsContainer(binsContainer.Start, binsContainer.End);
                    var concurrentBins = new ConcurrentBag<Bin>();
                    //foreach (var bin in binsContainer.Bins)
                    Parallel.ForEach(binsContainer.Bins, bin =>
                    {
                        if (detectorAggregations.Any(s => s.Start >= bin.Start && s.Start < bin.End))
                        {
                            var volume = 0;
                            switch (dataTypeEnum)
                            {
                                case DetectorVolumeDataTypes.DetectorActivationCount:
                                    volume =
                                        detectorAggregations.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.EventCount);
                                    break;
                                default:
                                    throw new Exception("Unknown Aggregate Data Type for Split Failure");
                            }

                            concurrentBins.Add(new Bin
                            {
                                Start = bin.Start,
                                End = bin.End,
                                Sum = volume,
                                Average = volume
                            });
                        }
                        else
                        {
                            concurrentBins.Add(new Bin
                            {
                                Start = bin.Start,
                                End = bin.End,
                                Sum = 0,
                                Average = 0
                            });
                        }
                    });
                    tempBinsContainer.Bins = concurrentBins.OrderBy(c => c.Start).ToList();
                    concurrentBinContainers.Add(tempBinsContainer);
                });
            }
            return concurrentBinContainers.OrderBy(b => b.Start).ToList();
        }
    }
}