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

        protected override void LoadBins(ApproachAggregationMetricOptions? approachAggregationMetricOptions, Location? signal, AggregationOptions options)
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