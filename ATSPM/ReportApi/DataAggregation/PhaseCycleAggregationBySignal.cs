using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using MOE.Common.Business.WCFServiceLibrary;

namespace MOE.Common.Business.DataAggregation
{
    public class PhaseCycleAggregationBySignal : AggregationBySignal
    {
        private readonly IPhaseCycleAggregationRepository phaseCycleAggregationRepository;

        public List<PhaseCycleAggregationByApproach> ApproachCycles { get; }

        public PhaseCycleAggregationBySignal(
            PhaseCycleAggregationOptions options,
            Location signal,
            IPhaseCycleAggregationRepository phaseCycleAggregationRepository) : base(
            options, signal)
        {
            this.phaseCycleAggregationRepository = phaseCycleAggregationRepository;
            ApproachCycles = new List<PhaseCycleAggregationByApproach>();
            GetApproachCycleAggregationContainersForAllApporaches(options, signal);
            LoadBins(null, null);
        }


        public PhaseCycleAggregationBySignal(
            PhaseCycleAggregationOptions options,
            Location signal,
            int phaseNumber,
            IPhaseCycleAggregationRepository phaseCycleAggregationRepository) : base(options, signal)
        {
            this.phaseCycleAggregationRepository = phaseCycleAggregationRepository;
            ApproachCycles = new List<PhaseCycleAggregationByApproach>();
            foreach (var approach in signal.Approaches)
                if (approach.ProtectedPhaseNumber == phaseNumber)
                {
                    ApproachCycles.Add(
                        new PhaseCycleAggregationByApproach(
                            approach,
                            options,
                            options.Start,
                            options.End,
                            true,
                            options.SelectedAggregatedDataType,
                            phaseCycleAggregationRepository));
                    if (approach.PermissivePhaseNumber != null && approach.PermissivePhaseNumber == phaseNumber)
                        ApproachCycles.Add(
                            new PhaseCycleAggregationByApproach(
                                approach,
                                options,
                                options.Start,
                                options.End,
                                false,
                                options.SelectedAggregatedDataType,
                                phaseCycleAggregationRepository));
                }
            LoadBins(null, null);
        }

        public PhaseCycleAggregationBySignal(PhaseCycleAggregationOptions options, Location signal,
            DirectionTypes direction) : base(options, signal)
        {
            ApproachCycles = new List<PhaseCycleAggregationByApproach>();
            foreach (var approach in signal.Approaches)
                if (approach.DirectionType.Id == direction)
                {
                    ApproachCycles.Add(
                        new PhaseCycleAggregationByApproach(
                            approach,
                            options,
                            options.Start,
                            options.End,
                            true,
                            options.SelectedAggregatedDataType,
                            phaseCycleAggregationRepository));
                    if (approach.PermissivePhaseNumber != null)
                        ApproachCycles.Add(
                            new PhaseCycleAggregationByApproach(
                                approach,
                                options,
                                options.Start,
                                options.End,
                                false,
                                options.SelectedAggregatedDataType,
                                phaseCycleAggregationRepository));
                }
            LoadBins(null, null);
        }

        protected override void LoadBins(SignalAggregationMetricOptions options, Location signal)
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

        protected override void LoadBins(ApproachAggregationMetricOptions options, Location signal)
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
            PhaseCycleAggregationOptions options, Location signal)
        {
            foreach (var approach in signal.Approaches)
            {
                ApproachCycles.Add(
                    new PhaseCycleAggregationByApproach(
                        approach,
                        options,
                        options.Start,
                        options.End,
                        true,
                        options.SelectedAggregatedDataType,
                        phaseCycleAggregationRepository));
                if (approach.PermissivePhaseNumber != null)
                    ApproachCycles.Add(
                        new PhaseCycleAggregationByApproach(
                            approach,
                            options,
                            options.Start,
                            options.End,
                            false,
                            options.SelectedAggregatedDataType,
                            phaseCycleAggregationRepository));
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