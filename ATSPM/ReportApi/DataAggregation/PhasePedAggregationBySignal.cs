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

        protected override void LoadBins(ApproachAggregationMetricOptions? approachAggregationMetricOptions, Location? signal, AggregationOptions options)
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