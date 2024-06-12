#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - MOE.Common.Business.DataAggregation/PhaseTerminationAggregationBySignal.cs
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
    public class PhaseTerminationAggregationBySignal : AggregationBySignal
    {
        private readonly IPhaseTerminationAggregationRepository phaseTerminationAggregationRepository;

        public List<PhaseTerminationAggregationByPhase> PhaseTerminations { get; }
        public PhaseTerminationAggregationBySignal(
            PhaseTerminationAggregationOptions phaseTerminationAggregationOptions,
            Location signal,
            IPhaseTerminationAggregationRepository phaseTerminationAggregationRepository,
            AggregationOptions options
            ) : base(
            phaseTerminationAggregationOptions, signal, options)
        {
            this.phaseTerminationAggregationRepository = phaseTerminationAggregationRepository;
            PhaseTerminations = new List<PhaseTerminationAggregationByPhase>();
            GetPhaseTerminationAggregationContainersForAllPhases(phaseTerminationAggregationOptions, signal, options);
            LoadBins(null, null, options);
        }

        public PhaseTerminationAggregationBySignal(
            PhaseTerminationAggregationOptions phaseTerminationAggregationOptions,
            Location signal,
            int phaseNumber,
            IPhaseTerminationAggregationRepository phaseTerminationAggregationRepository,
            AggregationOptions options
            ) : base(phaseTerminationAggregationOptions, signal, options)
        {
            this.phaseTerminationAggregationRepository = phaseTerminationAggregationRepository;
            PhaseTerminations = new List<PhaseTerminationAggregationByPhase>
            {
                new PhaseTerminationAggregationByPhase(signal, phaseNumber, phaseTerminationAggregationOptions, options.DataType, phaseTerminationAggregationRepository, options)
            };
            LoadBins(null, null, options);
        }

        protected override void LoadBins(SignalAggregationMetricOptions signalAggregationMetricOptions, Location signal, AggregationOptions options)
        {
            for (var i = 0; i < BinsContainers.Count; i++)
                for (var binIndex = 0; binIndex < BinsContainers[i].Bins.Count; binIndex++)
                {
                    var bin = BinsContainers[i].Bins[binIndex];
                    foreach (var approachSplitFailAggregationContainer in PhaseTerminations)
                    {
                        bin.Sum += approachSplitFailAggregationContainer.BinsContainers[i].Bins[binIndex].Sum;
                        bin.Average = PhaseTerminations.Count > 0 ? bin.Sum / PhaseTerminations.Count : 0;
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
                    foreach (var approachSplitFailAggregationContainer in PhaseTerminations)
                        bin.Sum += approachSplitFailAggregationContainer.BinsContainers[i].Bins[binIndex].Sum;
                    bin.Average = PhaseTerminations.Count > 0 ? bin.Sum / PhaseTerminations.Count : 0;
                }
            }
        }

        private void GetPhaseTerminationAggregationContainersForAllPhases(
            PhaseTerminationAggregationOptions phaseTerminationAggregationOptions, Location signal, AggregationOptions options)
        {
            List<int> availablePhases = GetAvailablePhasesForSignal(signal, options);
            foreach (var phaseNumber in availablePhases)
            {
                PhaseTerminations.Add(
                    new PhaseTerminationAggregationByPhase(
                        signal,
                        phaseNumber,
                        phaseTerminationAggregationOptions,
                        options.DataType,
                        phaseTerminationAggregationRepository,
                        options
                        ));
            }
        }

        private List<int> GetAvailablePhasesForSignal(Location signal, AggregationOptions options)
        {
            var availablePhases =
                phaseTerminationAggregationRepository.GetAggregationsBetweenDates(signal.LocationIdentifier, options.Start, options.End).Select(s => s.PhaseNumber).Distinct().ToList();

            return availablePhases;
        }

    }
}