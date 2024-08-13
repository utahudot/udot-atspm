﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - ATSPM.ReportApi.DataAggregation/PhaseTerminationAggregationByPhase.cs
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
    public class PhaseTerminationAggregationByPhase : AggregationByPhase
    {
        private readonly IPhaseTerminationAggregationRepository phaseTerminationAggregationRepository;

        public PhaseTerminationAggregationByPhase(
            Location signal,
            int phaseNumber,
            PhaseTerminationAggregationOptions phaseTerminationAggregationOptions,
            int dataType,
            IPhaseTerminationAggregationRepository phaseTerminationAggregationRepository,
            AggregationOptions options
            )
            : base(signal, phaseNumber, phaseTerminationAggregationOptions, dataType, options)
        {
            LoadBins(signal, phaseNumber, phaseTerminationAggregationOptions, dataType, options);
            this.phaseTerminationAggregationRepository = phaseTerminationAggregationRepository;
        }

        protected override void LoadBins(
            Location signal,
            int phaseNumber,
            PhaseAggregationMetricOptions phaseAggregationMetricOptions,
            int dataType,
            AggregationOptions options
            )
        {
            var dataTypeEnum = (PhaseTerminationDataType)dataType;
            var splitFails = phaseTerminationAggregationRepository.GetAggregationsBetweenDates(signal.LocationIdentifier, options.Start, options.End).ToList();
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
                                case PhaseTerminationDataType.GapOuts:
                                    terminationCount =
                                        splitFails.Where(s => s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.GapOuts);
                                    break;
                                case PhaseTerminationDataType.ForceOffs:
                                    terminationCount =
                                        splitFails.Where(s => s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.ForceOffs);
                                    break;
                                case PhaseTerminationDataType.MaxOuts:
                                    terminationCount =
                                        splitFails.Where(s => s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.MaxOuts);
                                    break;
                                case PhaseTerminationDataType.Unknown:
                                    terminationCount =
                                        splitFails.Where(s => s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.Unknown);
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