#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - MOE.Common.Business.DataAggregation/PhaseSplitMonitorAggregationBySignal.cs
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
    public class PhaseSplitMonitorAggregationBySignal : AggregationBySignal
    {
        private readonly IPhaseSplitMonitorAggregationRepository phaseSplitMonitorAggregationRepository;

        public List<PhaseSplitMonitorAggregationByPhase> SplitMonitorAggregations { get; }
        public PhaseSplitMonitorAggregationBySignal(
            PhaseSplitMonitorAggregationOptions phaseSplitMonitorAggregationOptions,
            Location signal,
            IPhaseSplitMonitorAggregationRepository phaseSplitMonitorAggregationRepository,
            AggregationOptions options
            ) : base(
            phaseSplitMonitorAggregationOptions, signal, options)
        {
            this.phaseSplitMonitorAggregationRepository = phaseSplitMonitorAggregationRepository;
            SplitMonitorAggregations = new List<PhaseSplitMonitorAggregationByPhase>();
            GetSplitMonitorAggregationContainersForAllPhases(phaseSplitMonitorAggregationOptions, signal, options);
            LoadBins(null, null, options);
        }

        public PhaseSplitMonitorAggregationBySignal(
            PhaseSplitMonitorAggregationOptions phaseSplitMonitorAggregationOptions,
            Location signal,
            int phaseNumber,
            IPhaseSplitMonitorAggregationRepository phaseSplitMonitorAggregationRepository,
            AggregationOptions options
            ) : base(phaseSplitMonitorAggregationOptions, signal, options)
        {
            this.phaseSplitMonitorAggregationRepository = phaseSplitMonitorAggregationRepository;
            SplitMonitorAggregations = new List<PhaseSplitMonitorAggregationByPhase>
            {
                new PhaseSplitMonitorAggregationByPhase(signal, phaseNumber, phaseSplitMonitorAggregationOptions, options.DataType, phaseSplitMonitorAggregationRepository, options)
            };
            LoadBins(null, null, options);
        }

        protected override void LoadBins(SignalAggregationMetricOptions phaseSplitMonitorAggregationOptions, Location signal, AggregationOptions options)
        {
            for (var i = 0; i < BinsContainers.Count; i++)
                for (var binIndex = 0; binIndex < BinsContainers[i].Bins.Count; binIndex++)
                {
                    var bin = BinsContainers[i].Bins[binIndex];
                    foreach (var approachSplitFailAggregationContainer in SplitMonitorAggregations)
                    {
                        bin.Sum += approachSplitFailAggregationContainer.BinsContainers[i].Bins[binIndex].Sum;
                        bin.Average = SplitMonitorAggregations.Count > 0 ? bin.Sum / SplitMonitorAggregations.Count : 0;
                    }
                }
        }

        protected override void LoadBins(ApproachAggregationMetricOptions phaseSplitMonitorAggregationOptions, Location signal, AggregationOptions options)
        {
            for (var i = 0; i < BinsContainers.Count; i++)
            {
                for (var binIndex = 0; binIndex < BinsContainers[i].Bins.Count; binIndex++)
                {
                    var bin = BinsContainers[i].Bins[binIndex];
                    foreach (var approachSplitFailAggregationContainer in SplitMonitorAggregations)
                        bin.Sum += approachSplitFailAggregationContainer.BinsContainers[i].Bins[binIndex].Sum;
                    bin.Average = SplitMonitorAggregations.Count > 0 ? bin.Sum / SplitMonitorAggregations.Count : 0;
                }
            }
        }

        private void GetSplitMonitorAggregationContainersForAllPhases(
            PhaseSplitMonitorAggregationOptions phaseSplitMonitorAggregationOptions, Location signal, AggregationOptions options)
        {
            List<int> availablePhases = GetAvailablePhasesForSignal(phaseSplitMonitorAggregationOptions, signal, options);
            foreach (var phaseNumber in availablePhases)
            {
                SplitMonitorAggregations.Add(
                    new PhaseSplitMonitorAggregationByPhase(
                        signal,
                        phaseNumber,
                        phaseSplitMonitorAggregationOptions,
                        options.DataType,
                        phaseSplitMonitorAggregationRepository,
                        options));
            }
        }

        private List<int> GetAvailablePhasesForSignal(
            PhaseSplitMonitorAggregationOptions phaseSplitMonitorAggregationOptions,
            Location signal, AggregationOptions options
            )
        {
            var availablePhases = phaseSplitMonitorAggregationRepository.GetAggregationsBetweenDates(signal.LocationIdentifier, options.Start, options.End).Select(x => x.PhaseNumber).Distinct().ToList();
            return availablePhases;
        }

    }
}