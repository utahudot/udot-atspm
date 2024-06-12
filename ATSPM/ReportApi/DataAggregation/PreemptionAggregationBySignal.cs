#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - MOE.Common.Business.DataAggregation/PreemptionAggregationBySignal.cs
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
using MOE.Common.Business.WCFServiceLibrary;
using System.Collections.Concurrent;

namespace MOE.Common.Business.DataAggregation
{
    public class PreemptionAggregationBySignal : AggregationBySignal
    {
        private readonly IPreemptionAggregationRepository preemptionAggregationRepository;

        public PreemptionAggregationBySignal(
            PreemptionAggregationOptions preemptionAggregationOptions,
            Location signal,
            IPreemptionAggregationRepository preemptionAggregationRepository,
            AggregationOptions options
            ) : base(
            preemptionAggregationOptions, signal, options)
        {
            this.preemptionAggregationRepository = preemptionAggregationRepository;
            LoadBins(preemptionAggregationOptions, signal, options);
        }

        protected override void LoadBins(
            SignalAggregationMetricOptions signalAggregationMetricOption,
            Location signal,
            AggregationOptions options
            )
        {
            var dataTypeEnum = (PreemptionDataTypes)options.DataType;
            var selectionEndDate = BinsContainers.Max(b => b.End);
            //Add a day so that it gets all the data for the entire end day instead of stoping at 12:00AM
            if (options.TimeOptions.SelectedBinSize == TimeOptions.BinSize.Day)
            {
                selectionEndDate = selectionEndDate.AddDays(1);
            }

            var preemptions =
                preemptionAggregationRepository.GetAggregationsBetweenDates(signal.LocationIdentifier, BinsContainers.Min(b => b.Start), selectionEndDate).ToList();

            if (preemptions != null)
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
                        if (preemptions.Any(s => s.Start >= bin.Start && s.Start < bin.End))
                        {
                            var preemptionSum = 0;

                            switch (dataTypeEnum)
                            {
                                case PreemptionDataTypes.PreemptNumber:
                                    preemptionSum = preemptions.Where(s =>
                                            s.Start >= bin.Start && s.Start < bin.End)
                                        .Sum(s => s.PreemptNumber);
                                    break;
                                case PreemptionDataTypes.PreemptRequests:
                                    preemptionSum = preemptions.Where(s =>
                                            s.Start >= bin.Start && s.Start < bin.End)
                                        .Sum(s => s.PreemptRequests);
                                    break;
                                case PreemptionDataTypes.PreemptServices:
                                    preemptionSum = preemptions.Where(s =>
                                            s.Start >= bin.Start && s.Start < bin.End)
                                        .Sum(s => s.PreemptServices);
                                    break;
                            }
                            Bin newBin = new Bin
                            {
                                Start = bin.Start,
                                End = bin.End,
                                Sum = preemptionSum,
                                Average = preemptionSum
                            };
                            concurrentBins.Add(newBin);
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

        protected override void LoadBins(ApproachAggregationMetricOptions approachAggregationMetricOptions, Location signal, AggregationOptions options)
        {
            throw new System.NotImplementedException();
        }
    }
}