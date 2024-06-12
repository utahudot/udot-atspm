#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - MOE.Common.Business.DataAggregation/PhasePedAggregationBySignal.cs
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
    public class PhasePedAggregationBySignal : AggregationBySignal
    {
        private readonly IPhasePedAggregationRepository phasePedAggregationRepository;

        public List<PhasePedAggregationByPhase> PedAggregations { get; }
        public PhasePedAggregationBySignal(
            PhasePedAggregationOptions phasePedAggregationOptions,
            Location signal,
            IPhasePedAggregationRepository phasePedAggregationRepository,
            AggregationOptions options
            ) : base(
            phasePedAggregationOptions, signal, options)
        {
            this.phasePedAggregationRepository = phasePedAggregationRepository;
            PedAggregations = new List<PhasePedAggregationByPhase>();
            GetPhasePedAggregationContainersForAllPhases(phasePedAggregationOptions, signal, options);
            LoadBins(null, null, options);
        }

        public PhasePedAggregationBySignal(
            PhasePedAggregationOptions phasePedAggregationOptions,
            Location signal,
            int phaseNumber,
            IPhasePedAggregationRepository phasePedAggregationRepository,
            AggregationOptions options
            ) : base(phasePedAggregationOptions, signal, options)
        {
            this.phasePedAggregationRepository = phasePedAggregationRepository;
            PedAggregations = new List<PhasePedAggregationByPhase>
            {
                new PhasePedAggregationByPhase(signal, phaseNumber, phasePedAggregationOptions, options.DataType, phasePedAggregationRepository, options)
            };
            LoadBins(null, null, options);
        }

        protected override void LoadBins(SignalAggregationMetricOptions signalAggregationMetricOptions, Location signal, AggregationOptions options)
        {
            for (var i = 0; i < BinsContainers.Count; i++)
                for (var binIndex = 0; binIndex < BinsContainers[i].Bins.Count; binIndex++)
                {
                    var bin = BinsContainers[i].Bins[binIndex];
                    foreach (var approachSplitFailAggregationContainer in PedAggregations)
                    {
                        bin.Sum += approachSplitFailAggregationContainer.BinsContainers[i].Bins[binIndex].Sum;
                        bin.Average = PedAggregations.Count > 0 ? bin.Sum / PedAggregations.Count : 0;
                    }
                }
        }

        protected override void LoadBins(ApproachAggregationMetricOptions approachAggregationMetricOptions, Location signal, AggregationOptions options)
        {
            for (var i = 0; i < BinsContainers.Count; i++)
            {
                for (var binIndex = 0; binIndex < BinsContainers[i].Bins.Count; binIndex++)
                {
                    var bin = BinsContainers[i].Bins[binIndex];
                    foreach (var approachSplitFailAggregationContainer in PedAggregations)
                        bin.Sum += approachSplitFailAggregationContainer.BinsContainers[i].Bins[binIndex].Sum;
                    bin.Average = PedAggregations.Count > 0 ? bin.Sum / PedAggregations.Count : 0;
                }
            }
        }

        private void GetPhasePedAggregationContainersForAllPhases(
            PhasePedAggregationOptions phasePedAggregationOptions, Location signal, AggregationOptions options)
        {
            List<int> availablePhases = GetAvailablePhasesForSignal(phasePedAggregationOptions, signal, options);
            foreach (var phaseNumber in availablePhases)
            {
                PedAggregations.Add(
                    new PhasePedAggregationByPhase(
                        signal,
                        phaseNumber,
                        phasePedAggregationOptions,
                        options.DataType,
                        phasePedAggregationRepository,
                        options
                        ));
            }
        }

        private List<int> GetAvailablePhasesForSignal(PhasePedAggregationOptions phasePedAggregationOptions, Location signal, AggregationOptions options)
        {
            var availablePhases = phasePedAggregationRepository.GetAggregationsBetweenDates(signal.LocationIdentifier, options.Start, options.End).Select(x => x.PhaseNumber).Distinct().ToList();
            return availablePhases;
        }

    }
}