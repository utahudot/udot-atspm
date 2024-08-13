﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - ATSPM.ReportApi.DataAggregation/PhaseCycleAggregationBySignal.cs
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

using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.ReportApi.DataAggregation
{
    public class PhaseCycleAggregationBySignal : AggregationBySignal
    {
        private readonly IPhaseCycleAggregationRepository phaseCycleAggregationRepository;

        public List<PhaseCycleAggregationByApproach> ApproachCycles { get; }

        public PhaseCycleAggregationBySignal(
            PhaseCycleAggregationOptions phaseCycleAggregationOptions,
            Location signal,
            IPhaseCycleAggregationRepository phaseCycleAggregationRepository,
            AggregationOptions options
            ) : base(
            phaseCycleAggregationOptions, signal, options)
        {
            this.phaseCycleAggregationRepository = phaseCycleAggregationRepository;
            ApproachCycles = new List<PhaseCycleAggregationByApproach>();
            GetApproachCycleAggregationContainersForAllApporaches(phaseCycleAggregationOptions, signal, options);
            LoadBins(null, null, options);
        }


        public PhaseCycleAggregationBySignal(
            PhaseCycleAggregationOptions phaseCycleAggregationOptions,
            Location signal,
            int phaseNumber,
            IPhaseCycleAggregationRepository phaseCycleAggregationRepository,
            AggregationOptions options
            ) : base(phaseCycleAggregationOptions, signal, options)
        {
            this.phaseCycleAggregationRepository = phaseCycleAggregationRepository;
            ApproachCycles = new List<PhaseCycleAggregationByApproach>();
            foreach (var approach in signal.Approaches)
                if (approach.ProtectedPhaseNumber == phaseNumber)
                {
                    ApproachCycles.Add(
                        new PhaseCycleAggregationByApproach(
                            approach,
                            phaseCycleAggregationOptions,
                            options.Start,
                            options.End,
                            true,
                            options.DataType,
                            phaseCycleAggregationRepository,
                            options
                            ));
                    if (approach.PermissivePhaseNumber != null && approach.PermissivePhaseNumber == phaseNumber)
                        ApproachCycles.Add(
                            new PhaseCycleAggregationByApproach(
                                approach,
                                phaseCycleAggregationOptions,
                                options.Start,
                                options.End,
                                false,
                                options.DataType,
                                phaseCycleAggregationRepository,
                                options
                                ));
                }
            LoadBins(null, null, options);
        }

        public PhaseCycleAggregationBySignal(
            PhaseCycleAggregationOptions phaseCycleAggregationOptions,
            Location signal,
            DirectionTypes direction,
            AggregationOptions options,
            IPhaseCycleAggregationRepository phaseCycleAggregationRepository
            ) : base(phaseCycleAggregationOptions, signal, options)
        {
            this.phaseCycleAggregationRepository = phaseCycleAggregationRepository;
            ApproachCycles = new List<PhaseCycleAggregationByApproach>();
            foreach (var approach in signal.Approaches)
                if (approach.DirectionTypeId == direction)
                {
                    ApproachCycles.Add(
                        new PhaseCycleAggregationByApproach(
                            approach,
                            phaseCycleAggregationOptions,
                            options.Start,
                            options.End,
                            true,
                            options.DataType,
                            phaseCycleAggregationRepository,
                            options
                            ));
                    if (approach.PermissivePhaseNumber != null)
                        ApproachCycles.Add(
                            new PhaseCycleAggregationByApproach(
                                approach,
                                phaseCycleAggregationOptions,
                                options.Start,
                                options.End,
                                false,
                                options.DataType,
                                phaseCycleAggregationRepository, options
                                ));
                }
            LoadBins(null, null, options);
        }

        protected override void LoadBins(SignalAggregationMetricOptions signalAggregationMetricOptions, Location signal, AggregationOptions options)
        {

            for (var i = 0; i < BinsContainers.Count; i++)
                for (var binIndex = 0; binIndex < BinsContainers[i].Bins.Count; binIndex++)
                {
                    var bin = BinsContainers[i].Bins[binIndex];
                    foreach (var approachCycleAggregationContainer in ApproachCycles)
                    {
                        bin.Sum += approachCycleAggregationContainer.BinsContainers[i].Bins[binIndex].Sum;
                        bin.Average = ApproachCycles.Count > 0 ? bin.Sum / ApproachCycles.Count : 0;
                    }

                }
        }

        protected override void LoadBins(ApproachAggregationMetricOptions? approachAggregationMetricOptions, Location? signal, AggregationOptions options)
        {
            for (var i = 0; i < BinsContainers.Count; i++)
            {
                for (var binIndex = 0; binIndex < BinsContainers[i].Bins.Count; binIndex++)
                {
                    var bin = BinsContainers[i].Bins[binIndex];
                    foreach (var approachCycleAggregationContainer in ApproachCycles)
                        bin.Sum += approachCycleAggregationContainer.BinsContainers[i].Bins[binIndex].Sum;
                    bin.Average = ApproachCycles.Count > 0 ? bin.Sum / ApproachCycles.Count : 0;
                }
            }
        }


        private void GetApproachCycleAggregationContainersForAllApporaches(
            PhaseCycleAggregationOptions phaseCycleAggregationOptions,
            Location signal,
            AggregationOptions options)
        {
            foreach (var approach in signal.Approaches)
            {
                ApproachCycles.Add(
                    new PhaseCycleAggregationByApproach(
                        approach,
                        phaseCycleAggregationOptions,
                        options.Start,
                        options.End,
                        true,
                        options.DataType,
                        phaseCycleAggregationRepository,
                        options
                        ));
                if (approach.PermissivePhaseNumber != null)
                    ApproachCycles.Add(
                        new PhaseCycleAggregationByApproach(
                            approach,
                            phaseCycleAggregationOptions,
                            options.Start,
                            options.End,
                            false,
                            options.DataType,
                            phaseCycleAggregationRepository,
                            options
                            ));
            }
        }


        public double GetCyclesByDirection(DirectionTypes direction)
        {
            double splitFails = 0;
            if (ApproachCycles != null)
                splitFails = ApproachCycles
                    .Where(a => a.Approach.DirectionType.Id == direction)
                    .Sum(a => a.BinsContainers.FirstOrDefault().SumValue);
            return splitFails;
        }

        public int GetAverageCyclesByDirection(DirectionTypes direction)
        {
            var approachCyclesByDirection = ApproachCycles
                .Where(a => a.Approach.DirectionType.Id == direction);
            var splitFails = 0;
            if (approachCyclesByDirection.Any())
                splitFails = Convert.ToInt32(Math.Round(approachCyclesByDirection
                    .Average(a => a.BinsContainers.FirstOrDefault().SumValue)));
            return splitFails;
        }
    }
}