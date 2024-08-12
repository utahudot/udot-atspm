﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - ATSPM.ReportApi.DataAggregation/YellowRedActivationsAggregationByApproach.cs
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

using System.Collections.Concurrent;

namespace Utah.Udot.Atspm.ReportApi.DataAggregation
{
    public class YellowRedActivationsAggregationByApproach : AggregationByApproach
    {
        private readonly IApproachYellowRedActivationAggregationRepository approachYellowRedActivationAggregationRepository;

        public YellowRedActivationsAggregationByApproach(
            Approach approach,
            ApproachYellowRedActivationsAggregationOptions approachYellowRedActivationsAggregationOptions,
            DateTime startDate,
            DateTime endDate,
            bool getProtectedPhase,
            int dataType,
            IApproachYellowRedActivationAggregationRepository approachYellowRedActivationAggregationRepository,
            AggregationOptions options
            ) : base(approach, approachYellowRedActivationsAggregationOptions, startDate, endDate,
            getProtectedPhase, dataType, options)
        {
            LoadBins(approach, approachYellowRedActivationsAggregationOptions, getProtectedPhase, dataType, options);
            this.approachYellowRedActivationAggregationRepository = approachYellowRedActivationAggregationRepository;
        }

        protected override void LoadBins(
            Approach approach,
            ApproachAggregationMetricOptions approachAggregationMetricOptions,
            bool getProtectedPhase,
            int dataType,
            AggregationOptions options
            )
        {
            var dataTypeEnum = (YellowRedActivationsDataTypes)dataType;
            var yellowRedActivations = approachYellowRedActivationAggregationRepository
                 .GetAggregationsBetweenDates(approach.Location.LocationIdentifier, options.Start, options.End)
                .Where(a => a.ApproachId == approach.Id && a.IsProtectedPhase == getProtectedPhase)
                .ToList();
            if (yellowRedActivations != null)
            {
                var concurrentBinContainers = new ConcurrentBag<BinsContainer>();
                //foreach (var binsContainer in binsContainers)
                Parallel.ForEach(BinsContainers, binsContainer =>
                {
                    var tempBinsContainer =
                        new BinsContainer(binsContainer.Start, binsContainer.End);
                    var concurrentBins = new ConcurrentBag<Bin>();
                    //foreach (var bin in binsContainer.Bins)
                    Parallel.ForEach(binsContainer.Bins, bin =>
                    {
                        if (yellowRedActivations.Any(s => s.Start >= bin.Start && s.Start < bin.End))
                        {
                            var yellowRedActivationCount = 0;
                            switch (dataTypeEnum)
                            {
                                case YellowRedActivationsDataTypes.SevereRedLightViolations:
                                    yellowRedActivationCount =
                                        yellowRedActivations.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.SevereRedLightViolations);
                                    break;
                                case YellowRedActivationsDataTypes.TotalRedLightViolations:
                                    yellowRedActivationCount =
                                        yellowRedActivations.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.TotalRedLightViolations);
                                    break;
                                case YellowRedActivationsDataTypes.YellowActivations:
                                    yellowRedActivationCount =
                                        yellowRedActivations.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.YellowActivations);
                                    break;
                                case YellowRedActivationsDataTypes.ViolationTime:
                                    yellowRedActivationCount =
                                        yellowRedActivations.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.ViolationTime);
                                    break;
                                case YellowRedActivationsDataTypes.Cycles:
                                    yellowRedActivationCount =
                                        yellowRedActivations.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.Cycles);
                                    break;
                                default:

                                    throw new Exception("Unknown Aggregate Data Type for Yellow Red Activation");
                            }

                            concurrentBins.Add(new Bin
                            {
                                Start = bin.Start,
                                End = bin.End,
                                Sum = yellowRedActivationCount,
                                Average = yellowRedActivationCount
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
                BinsContainers = concurrentBinContainers.OrderBy(b => b.Start).ToList();
            }
        }

    }
}