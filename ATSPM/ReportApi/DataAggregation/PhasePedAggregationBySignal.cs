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
            PhasePedAggregationOptions options,
            Location signal,
            IPhasePedAggregationRepository phasePedAggregationRepository) : base(
            options, signal)
        {
            this.phasePedAggregationRepository = phasePedAggregationRepository;
            PedAggregations = new List<PhasePedAggregationByPhase>();
            GetPhasePedAggregationContainersForAllPhases(options, signal);
            LoadBins(null, null);
        }

        public PhasePedAggregationBySignal(
            PhasePedAggregationOptions options,
            Location signal,
            int phaseNumber,
            IPhasePedAggregationRepository phasePedAggregationRepository) : base(options, signal)
        {
            this.phasePedAggregationRepository = phasePedAggregationRepository;
            PedAggregations = new List<PhasePedAggregationByPhase>
            {
                new PhasePedAggregationByPhase(signal, phaseNumber, options, options.SelectedAggregatedDataType, phasePedAggregationRepository)
            };
            LoadBins(null, null);
        }

        protected override void LoadBins(SignalAggregationMetricOptions options, Location signal)
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

        protected override void LoadBins(ApproachAggregationMetricOptions options, Location signal)
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
            PhasePedAggregationOptions options, Location signal)
        {
            List<int> availablePhases = GetAvailablePhasesForSignal(options, signal);
            foreach (var phaseNumber in availablePhases)
            {
                PedAggregations.Add(
                    new PhasePedAggregationByPhase(
                        signal,
                        phaseNumber,
                        options,
                        options.SelectedAggregatedDataType,
                        phasePedAggregationRepository));
            }
        }

        private List<int> GetAvailablePhasesForSignal(PhasePedAggregationOptions options, Location signal)
        {
            var availablePhases = phasePedAggregationRepository.GetAggregationsBetweenDates(signal.LocationIdentifier, options.Start, options.End).Select(x => x.PhaseNumber).Distinct().ToList();
            return availablePhases;
        }

    }
}