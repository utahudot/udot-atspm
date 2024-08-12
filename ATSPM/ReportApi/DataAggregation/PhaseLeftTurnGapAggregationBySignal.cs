﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - ATSPM.ReportApi.DataAggregation/PhaseLeftTurnGapAggregationBySignal.cs
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
    public class PhaseLeftTurnGapAggregationBySignal : AggregationBySignal
    {
        private readonly IPhaseLeftTurnGapAggregationRepository phaseLeftTurnGapAggregationRepository;

        public List<PhaseLeftTurnGapAggregationByApproach> ApproachLeftTurnGaps { get; }

        public PhaseLeftTurnGapAggregationBySignal(
            PhaseLeftTurnGapAggregationOptions phaseLeftTurnGapAggregationOptions,
            Location signal,
            IPhaseLeftTurnGapAggregationRepository phaseLeftTurnGapAggregationRepository,
            AggregationOptions options
            ) : base(
            phaseLeftTurnGapAggregationOptions, signal, options)
        {
            this.phaseLeftTurnGapAggregationRepository = phaseLeftTurnGapAggregationRepository;
            ApproachLeftTurnGaps = new List<PhaseLeftTurnGapAggregationByApproach>();
            GetApproachLeftTurnGapAggregationContainersForAllApporaches(phaseLeftTurnGapAggregationOptions, signal, options);
            LoadBins(null, null, options);
        }


        public PhaseLeftTurnGapAggregationBySignal(
            PhaseLeftTurnGapAggregationOptions phaseLeftTurnGapAggregationOptions,
            Location signal,
            int phaseNumber,
            AggregationOptions options
            ) : base(phaseLeftTurnGapAggregationOptions, signal, options)
        {
            ApproachLeftTurnGaps = new List<PhaseLeftTurnGapAggregationByApproach>();
            foreach (var approach in signal.Approaches)
                if (approach.ProtectedPhaseNumber == phaseNumber)
                {
                    ApproachLeftTurnGaps.Add(
                        new PhaseLeftTurnGapAggregationByApproach(
                            approach,
                            phaseLeftTurnGapAggregationOptions,
                            options.Start,
                            options.End,
                            true,
                            options.DataType,
                            phaseLeftTurnGapAggregationRepository,
                            options
                            ));
                    if (approach.PermissivePhaseNumber != null && approach.PermissivePhaseNumber == phaseNumber)
                        ApproachLeftTurnGaps.Add(
                            new PhaseLeftTurnGapAggregationByApproach(
                                approach,
                                phaseLeftTurnGapAggregationOptions,
                                options.Start,
                                options.End,
                                false,
                                options.DataType,
                                phaseLeftTurnGapAggregationRepository, options
                                ));
                }
            LoadBins(null, null, options);
        }

        public PhaseLeftTurnGapAggregationBySignal(
            PhaseLeftTurnGapAggregationOptions phaseLeftTurnGapAggregationOptions,
            Location signal,
            DirectionTypes direction,
            AggregationOptions options
            ) : base(phaseLeftTurnGapAggregationOptions, signal, options)
        {
            ApproachLeftTurnGaps = new List<PhaseLeftTurnGapAggregationByApproach>();
            foreach (var approach in signal.Approaches)
                if (approach.DirectionType.Id == direction)
                {
                    ApproachLeftTurnGaps.Add(
                        new PhaseLeftTurnGapAggregationByApproach(
                            approach,
                            phaseLeftTurnGapAggregationOptions,
                            options.Start,
                            options.End,
                            true,
                            options.DataType,
                            phaseLeftTurnGapAggregationRepository,
                            options
                            ));
                    if (approach.PermissivePhaseNumber != null)
                        ApproachLeftTurnGaps.Add(
                            new PhaseLeftTurnGapAggregationByApproach(
                                approach,
                                phaseLeftTurnGapAggregationOptions,
                                options.Start,
                                options.End,
                                false,
                                options.DataType,
                                phaseLeftTurnGapAggregationRepository,
                                options
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
                    foreach (var approachLeftTurnGapAggregationContainer in ApproachLeftTurnGaps)
                    {
                        bin.Sum += approachLeftTurnGapAggregationContainer.BinsContainers[i].Bins[binIndex].Sum;
                        bin.Average = ApproachLeftTurnGaps.Count > 0 ? bin.Sum / ApproachLeftTurnGaps.Count : 0;
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
                    foreach (var approachLeftTurnGapAggregationContainer in ApproachLeftTurnGaps)
                        bin.Sum += approachLeftTurnGapAggregationContainer.BinsContainers[i].Bins[binIndex].Sum;
                    bin.Average = ApproachLeftTurnGaps.Count > 0 ? bin.Sum / ApproachLeftTurnGaps.Count : 0;
                }
            }
        }


        private void GetApproachLeftTurnGapAggregationContainersForAllApporaches(
            PhaseLeftTurnGapAggregationOptions phaseLeftTurnGapAggregationOptions,
            Location signal,
            AggregationOptions options)
        {
            foreach (var approach in signal.Approaches)
            {
                ApproachLeftTurnGaps.Add(
                    new PhaseLeftTurnGapAggregationByApproach(
                        approach,
                        phaseLeftTurnGapAggregationOptions,
                        options.Start,
                        options.End,
                        true,
                        options.DataType,
                        phaseLeftTurnGapAggregationRepository,
                        options
                        ));
                if (approach.PermissivePhaseNumber != null)
                    ApproachLeftTurnGaps.Add(
                        new PhaseLeftTurnGapAggregationByApproach(
                            approach,
                            phaseLeftTurnGapAggregationOptions,
                            options.Start,
                            options.End,
                            false,
                            options.DataType,
                            phaseLeftTurnGapAggregationRepository,
                            options
                            ));
            }
        }


        public double GetLeftTurnGapByDirection(DirectionTypes direction)
        {
            double splitFails = 0;
            if (ApproachLeftTurnGaps != null)
                splitFails = ApproachLeftTurnGaps
                    .Where(a => a.Approach.DirectionType.Id == direction)
                    .Sum(a => a.BinsContainers.FirstOrDefault().SumValue);
            return splitFails;
        }

        public int GetAverageGapByDirection(DirectionTypes direction)
        {
            var approachLeftTurnGapByDirection = ApproachLeftTurnGaps
                .Where(a => a.Approach.DirectionType.Id == direction);
            var splitFails = 0;
            if (approachLeftTurnGapByDirection.Any())
                splitFails = Convert.ToInt32(Math.Round(approachLeftTurnGapByDirection
                    .Average(a => a.BinsContainers.FirstOrDefault().SumValue)));
            return splitFails;
        }
    }
}