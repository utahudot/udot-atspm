#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - MOE.Common.Business.DataAggregation/PhaseSplitMonitorAggregationByPhase.cs
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
    public class PhaseSplitMonitorAggregationByPhase : AggregationByPhase
    {
        private readonly IPhaseSplitMonitorAggregationRepository phaseSplitMonitorAggregationRepository;

        public PhaseSplitMonitorAggregationByPhase(
            Location signal,
            int phaseNumber,
            PhaseSplitMonitorAggregationOptions phaseSplitMonitorAggregation,
            int dataType,
            IPhaseSplitMonitorAggregationRepository phaseSplitMonitorAggregationRepository,
            AggregationOptions options
            )
            : base(signal, phaseNumber, phaseSplitMonitorAggregation, dataType, options)
        {
            this.phaseSplitMonitorAggregationRepository = phaseSplitMonitorAggregationRepository;
            LoadBins(signal, phaseNumber, phaseSplitMonitorAggregation, dataType, options);
        }


        protected override void LoadBins(
            Location signal,
            int phaseNumber,
            PhaseAggregationMetricOptions phaseAggregationMetricOptions,
            int dataType,
            AggregationOptions options
            )
        {
            var dataTypeEnum = (SplitMonitorDataTypes)dataType;
            var splitFails = phaseSplitMonitorAggregationRepository.GetAggregationsBetweenDates(signal.LocationIdentifier, options.Start, options.End).Where(a => a.PhaseNumber == phaseNumber).ToList();
            if (splitFails != null)
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
                        if (splitFails.Any(s => s.Start >= bin.Start && s.Start < bin.End))
                        {
                            var terminationCount = 0;
                            switch (dataTypeEnum)
                            {
                                case SplitMonitorDataTypes.EightyFifthPercentileSplit:
                                    terminationCount =
                                        splitFails.Where(s => s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.EightyFifthPercentileSplit);
                                    break;
                                case SplitMonitorDataTypes.SkippedCount:
                                    terminationCount =
                                        splitFails.Where(s => s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.SkippedCount);
                                    break;
                                default:

                                    throw new Exception("Unknown Aggregate Data Type for Split Failure");
                            }

                            concurrentBins.Add(new Bin
                            {
                                Start = bin.Start,
                                End = bin.End,
                                Sum = terminationCount,
                                Average = terminationCount
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