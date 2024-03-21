using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using MOE.Common.Business.WCFServiceLibrary;

namespace MOE.Common.Business.DataAggregation
{
    public class PcdAggregationBySignal : AggregationBySignal
    {
        private readonly IApproachPcdAggregationRepository approachPcdAggregationRepository;

        public PcdAggregationBySignal(ApproachPcdAggregationOptions options, Location signal) : base(
            options, signal)
        {
            ApproachPcds = new List<PcdAggregationByApproach>();
            GetApproachPcdAggregationContainersForAllApporaches(options, signal);
            LoadBins(null, null);
        }

        public PcdAggregationBySignal(
            ApproachPcdAggregationOptions options,
            Location signal,
            int phaseNumber,
            IApproachPcdAggregationRepository approachPcdAggregationRepository) : base(options, signal)
        {
            ApproachPcds = new List<PcdAggregationByApproach>();
            foreach (var approach in signal.Approaches)
                if (approach.ProtectedPhaseNumber == phaseNumber)
                {
                    ApproachPcds.Add(
                        new PcdAggregationByApproach(
                            approach,
                            options,
                            options.Start,
                            options.End,
                            true,
                            options.SelectedAggregatedDataType,
                            approachPcdAggregationRepository));
                    if (approach.PermissivePhaseNumber != null && approach.PermissivePhaseNumber == phaseNumber)
                        ApproachPcds.Add(
                            new PcdAggregationByApproach(
                                approach,
                                options,
                                options.Start,
                                options.End,
                                false,
                                options.SelectedAggregatedDataType,
                                approachPcdAggregationRepository));
                }
            LoadBins(null, null);
            this.approachPcdAggregationRepository = approachPcdAggregationRepository;
        }

        public PcdAggregationBySignal(ApproachPcdAggregationOptions options, Location signal,
            DirectionTypes direction) : base(options, signal)
        {
            ApproachPcds = new List<PcdAggregationByApproach>();
            foreach (var approach in signal.Approaches)
                if (approach.DirectionType.Id == direction)
                {
                    ApproachPcds.Add(
                        new PcdAggregationByApproach(
                            approach,
                            options,
                            options.Start,
                            options.End,
                            true,
                            options.SelectedAggregatedDataType,
                            approachPcdAggregationRepository));
                    if (approach.PermissivePhaseNumber != null)
                        ApproachPcds.Add(
                            new PcdAggregationByApproach(
                                approach,
                                options,
                                options.Start,
                                options.End,
                                false,
                                options.SelectedAggregatedDataType,
                                approachPcdAggregationRepository));
                }
            LoadBins(null, null);
        }

        public List<PcdAggregationByApproach> ApproachPcds { get; }

        protected override void LoadBins(SignalAggregationMetricOptions options, Location signal)
        {
            for (var i = 0; i < BinsContainers.Count; i++)
            {
                for (var binIndex = 0; binIndex < BinsContainers[i].Bins.Count; binIndex++)
                {
                    var bin = BinsContainers[i].Bins[binIndex];
                    foreach (var approachPcdAggregationContainer in ApproachPcds)
                        bin.Sum += approachPcdAggregationContainer.BinsContainers[i].Bins[binIndex].Sum;
                    bin.Average = ApproachPcds.Count > 0 ? bin.Sum / ApproachPcds.Count : 0;
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
                    foreach (var approachPcdAggregationContainer in ApproachPcds)
                        bin.Sum += approachPcdAggregationContainer.BinsContainers[i].Bins[binIndex].Sum;
                    bin.Average = ApproachPcds.Count > 0 ? bin.Sum / ApproachPcds.Count : 0;
                }
            }
        }


        private void GetApproachPcdAggregationContainersForAllApporaches(
            ApproachPcdAggregationOptions options, Location signal)
        {
            foreach (var approach in signal.Approaches)
            {
                ApproachPcds.Add(
                    new PcdAggregationByApproach(
                        approach,
                        options,
                        options.Start,
                        options.End,
                        true,
                        options.SelectedAggregatedDataType,
                        approachPcdAggregationRepository));
                if (approach.PermissivePhaseNumber != null)
                    ApproachPcds.Add(
                        new PcdAggregationByApproach(
                            approach,
                            options,
                            options.Start,
                            options.End,
                            false,
                            options.SelectedAggregatedDataType,
                            approachPcdAggregationRepository));
            }
        }


        public double GetPcdsByDirection(DirectionTypes direction)
        {
            double splitFails = 0;
            if (ApproachPcds != null)
                splitFails = ApproachPcds
                    .Where(a => a.Approach.DirectionType.Id == direction)
                    .Sum(a => a.BinsContainers.FirstOrDefault().SumValue);
            return splitFails;
        }

        public int GetAveragePcdsByDirection(DirectionTypes direction)
        {
            var approachPcduresByDirection = ApproachPcds
                .Where(a => a.Approach.DirectionType.Id == direction);
            var splitFails = 0;
            if (approachPcduresByDirection.Any())
                splitFails = Convert.ToInt32(Math.Round(approachPcduresByDirection
                    .Average(a => a.BinsContainers.FirstOrDefault().SumValue)));
            return splitFails;
        }
    }
}