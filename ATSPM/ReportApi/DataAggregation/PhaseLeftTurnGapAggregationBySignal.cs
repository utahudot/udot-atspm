using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using MOE.Common.Business.WCFServiceLibrary;

namespace MOE.Common.Business.DataAggregation
{
    public class PhaseLeftTurnGapAggregationBySignal : AggregationBySignal
    {
        private readonly IPhaseLeftTurnGapAggregationRepository phaseLeftTurnGapAggregationRepository;

        public List<PhaseLeftTurnGapAggregationByApproach> ApproachLeftTurnGaps { get; }

        public PhaseLeftTurnGapAggregationBySignal(
            PhaseLeftTurnGapAggregationOptions options,
            Location signal,
            IPhaseLeftTurnGapAggregationRepository phaseLeftTurnGapAggregationRepository
            ) : base(
            options, signal)
        {
            this.phaseLeftTurnGapAggregationRepository = phaseLeftTurnGapAggregationRepository;
            ApproachLeftTurnGaps = new List<PhaseLeftTurnGapAggregationByApproach>();
            GetApproachLeftTurnGapAggregationContainersForAllApporaches(options, signal);
            LoadBins(null, null);
        }


        public PhaseLeftTurnGapAggregationBySignal(PhaseLeftTurnGapAggregationOptions options, Location signal,
            int phaseNumber) : base(options, signal)
        {
            ApproachLeftTurnGaps = new List<PhaseLeftTurnGapAggregationByApproach>();
            foreach (var approach in signal.Approaches)
                if (approach.ProtectedPhaseNumber == phaseNumber)
                {
                    ApproachLeftTurnGaps.Add(
                        new PhaseLeftTurnGapAggregationByApproach(
                            approach,
                            options,
                            options.Start,
                            options.End,
                            true,
                            options.SelectedAggregatedDataType,
                            phaseLeftTurnGapAggregationRepository));
                    if (approach.PermissivePhaseNumber != null && approach.PermissivePhaseNumber == phaseNumber)
                        ApproachLeftTurnGaps.Add(
                            new PhaseLeftTurnGapAggregationByApproach(
                                approach,
                                options,
                                options.Start,
                                options.End,
                                false,
                                options.SelectedAggregatedDataType,
                                phaseLeftTurnGapAggregationRepository));
                }
            LoadBins(null, null);
        }

        public PhaseLeftTurnGapAggregationBySignal(PhaseLeftTurnGapAggregationOptions options, Location signal,
            DirectionTypes direction) : base(options, signal)
        {
            ApproachLeftTurnGaps = new List<PhaseLeftTurnGapAggregationByApproach>();
            foreach (var approach in signal.Approaches)
                if (approach.DirectionType.Id == direction)
                {
                    ApproachLeftTurnGaps.Add(
                        new PhaseLeftTurnGapAggregationByApproach(
                            approach,
                            options,
                            options.Start,
                            options.End,
                            true,
                            options.SelectedAggregatedDataType,
                            phaseLeftTurnGapAggregationRepository));
                    if (approach.PermissivePhaseNumber != null)
                        ApproachLeftTurnGaps.Add(
                            new PhaseLeftTurnGapAggregationByApproach(
                                approach,
                                options,
                                options.Start,
                                options.End,
                                false,
                                options.SelectedAggregatedDataType,
                                phaseLeftTurnGapAggregationRepository));
                }
            LoadBins(null, null);
        }

        protected override void LoadBins(SignalAggregationMetricOptions options, Location signal)
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

        protected override void LoadBins(ApproachAggregationMetricOptions options, Location signal)
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
            PhaseLeftTurnGapAggregationOptions options, Location signal)
        {
            foreach (var approach in signal.Approaches)
            {
                ApproachLeftTurnGaps.Add(
                    new PhaseLeftTurnGapAggregationByApproach(
                        approach,
                        options,
                        options.Start,
                        options.End,
                        true,
                        options.SelectedAggregatedDataType,
                        phaseLeftTurnGapAggregationRepository));
                if (approach.PermissivePhaseNumber != null)
                    ApproachLeftTurnGaps.Add(
                        new PhaseLeftTurnGapAggregationByApproach(
                            approach,
                            options,
                            options.Start,
                            options.End,
                            false,
                            options.SelectedAggregatedDataType,
                            phaseLeftTurnGapAggregationRepository));
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