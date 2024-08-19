﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - ATSPM.ReportApi.DataAggregation/PcdAggregationByApproach.cs
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
    //change to enum
    //public const string VOLUME = "Volume";
    //public const string TOTAL_DELAY = "TotalDelay";
    //public const string ARRIVALS_ON_YELLOW = "ArrivalsOnYellow";
    //public const string ARRIVALS_ON_RED = "ArrivalsOnRed";
    //public const string ARRIVALS_ON_GREEN = "ArrivalsOnGreen";

    public enum PcdAggregatedDataType
    {
        Volume,
        TotalDelay,
        ArrivalsOnYellow,
        ArrivalsOnRed,
        ArrivalsOnGreen
    }

    public class PcdAggregationByApproach : AggregationByApproach
    {

        private readonly IApproachPcdAggregationRepository approachPcdAggregationRepository;

        public PcdAggregationByApproach(
            Approach approach,
            ApproachPcdAggregationOptions approachAggregationOptions,
            DateTime startDate,
            DateTime endDate,
            bool getProtectedPhase,
            int dataType,
            IApproachPcdAggregationRepository approachPcdAggregationRepository,
            AggregationOptions options) : base(approach, approachAggregationOptions, startDate, endDate,
            getProtectedPhase, dataType, options)
        {
            this.approachPcdAggregationRepository = approachPcdAggregationRepository;
            LoadBins(approach, approachAggregationOptions, getProtectedPhase, dataType, options);
        }

        protected override void LoadBins(Approach approach, ApproachAggregationMetricOptions approachAggregationMetricOptions,
            bool getProtectedPhase,
            int dataType,
            AggregationOptions options)
        {
            var dataTypeEnum = (PcdAggregatedDataType)dataType;
            var pcdAggregations =
                approachPcdAggregationRepository.GetAggregationsBetweenDates(approach.Location.LocationIdentifier, options.Start, options.End)
                .Where(a => a.ApproachId == approach.Id && a.IsProtectedPhase == getProtectedPhase)
                .ToList();
            if (pcdAggregations != null)
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
                        if (pcdAggregations.Any(s => s.Start >= bin.Start && s.Start < bin.End))
                        {
                            double pcdCount = 0;
                            switch (dataTypeEnum)
                            {
                                case PcdAggregatedDataType.ArrivalsOnGreen:
                                    pcdCount =
                                        pcdAggregations.Where(s => s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.ArrivalsOnGreen);
                                    break;
                                case PcdAggregatedDataType.ArrivalsOnRed:
                                    pcdCount =
                                        pcdAggregations.Where(s => s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.ArrivalsOnRed);
                                    break;
                                case PcdAggregatedDataType.ArrivalsOnYellow:
                                    pcdCount =
                                        pcdAggregations.Where(s => s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.ArrivalsOnYellow);
                                    break;
                                case PcdAggregatedDataType.TotalDelay:
                                    pcdCount = pcdAggregations.Where(s => s.Start >= bin.Start && s.Start < bin.End).Sum(s => s.TotalDelay);
                                    break;
                                case PcdAggregatedDataType.Volume:
                                    pcdCount = pcdAggregations.Where(s => s.Start >= bin.Start && s.Start < bin.End).Sum(s => s.Volume);
                                    break;
                                default:
                                    throw new Exception("Unknown Aggregate Data Type for Split Failure");
                            }

                            concurrentBins.Add(new Bin
                            {
                                Start = bin.Start,
                                End = bin.End,
                                Sum = pcdCount,
                                Average = pcdCount
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

        protected double GetPercentArrivalOnGreen(Bin bin, List<ApproachPcdAggregation> pcdAggregations)
        {
            double percentArrivalOnGreen = 0;
            double arrivalsOnGreen = pcdAggregations.Where(s => s.Start >= bin.Start && s.Start < bin.End).Sum(s => s.ArrivalsOnGreen);
            int totalArrivals = pcdAggregations.Where(s => s.Start >= bin.Start && s.Start < bin.End).Sum(s => s.ArrivalsOnGreen) +
                                pcdAggregations.Where(s => s.Start >= bin.Start && s.Start < bin.End).Sum(s => s.ArrivalsOnYellow) +
                                pcdAggregations.Where(s => s.Start >= bin.Start && s.Start < bin.End).Sum(s => s.ArrivalsOnRed);
            if (totalArrivals > 0)
            {
                percentArrivalOnGreen = arrivalsOnGreen / totalArrivals;
            }
            else
            {
                percentArrivalOnGreen = 0;
            }
            return percentArrivalOnGreen;
        }

    }
}