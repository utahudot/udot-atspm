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
            PhaseSplitMonitorAggregationOptions options,
            Location signal,
            IPhaseSplitMonitorAggregationRepository phaseSplitMonitorAggregationRepository
            ) : base(
            options, signal)
        {
            this.phaseSplitMonitorAggregationRepository = phaseSplitMonitorAggregationRepository;
            SplitMonitorAggregations = new List<PhaseSplitMonitorAggregationByPhase>();
            GetSplitMonitorAggregationContainersForAllPhases(options, signal);
            LoadBins(null, null);
        }

        public PhaseSplitMonitorAggregationBySignal(
            PhaseSplitMonitorAggregationOptions options,
            Location signal,
            int phaseNumber,
            IPhaseSplitMonitorAggregationRepository phaseSplitMonitorAggregationRepository
            ) : base(options, signal)
        {
            this.phaseSplitMonitorAggregationRepository = phaseSplitMonitorAggregationRepository;
            SplitMonitorAggregations = new List<PhaseSplitMonitorAggregationByPhase>
            {
                new PhaseSplitMonitorAggregationByPhase(signal, phaseNumber, options, options.SelectedAggregatedDataType, phaseSplitMonitorAggregationRepository)
            };
            LoadBins(null, null);
        }

        protected override void LoadBins(SignalAggregationMetricOptions options, Location signal)
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

        protected override void LoadBins(ApproachAggregationMetricOptions options, Location signal)
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
            PhaseSplitMonitorAggregationOptions options, Location signal)
        {
            List<int> availablePhases = GetAvailablePhasesForSignal(options, signal);
            foreach (var phaseNumber in availablePhases)
            {
                SplitMonitorAggregations.Add(
                    new PhaseSplitMonitorAggregationByPhase(
                        signal,
                        phaseNumber,
                        options,
                        options.SelectedAggregatedDataType,
                        phaseSplitMonitorAggregationRepository));
            }
        }

        private List<int> GetAvailablePhasesForSignal(
            PhaseSplitMonitorAggregationOptions options,
            Location signal
            )
        {
            var availablePhases = phaseSplitMonitorAggregationRepository.GetAggregationsBetweenDates(signal.LocationIdentifier, options.Start, options.End).Select(x => x.PhaseNumber).Distinct().ToList();
            return availablePhases;
        }

    }
}