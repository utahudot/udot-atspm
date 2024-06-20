using ATSPM.Application.Business.Aggregation;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using MOE.Common.Business.WCFServiceLibrary;

namespace MOE.Common.Business.DataAggregation
{
    public class YellowRedActivationsAggregationBySignal : AggregationBySignal
    {
        private readonly IApproachYellowRedActivationAggregationRepository approachYellowRedActivationAggregationRepository;

        public YellowRedActivationsAggregationBySignal(
            ApproachYellowRedActivationsAggregationOptions yellowRedActivationsAggregationOptions,
            Location signal,
            IApproachYellowRedActivationAggregationRepository approachYellowRedActivationAggregationRepository,
            AggregationOptions options
            ) : base(
            yellowRedActivationsAggregationOptions, signal, options)
        {
            this.approachYellowRedActivationAggregationRepository = approachYellowRedActivationAggregationRepository;
            ApproachYellowRedActivationsures = new List<YellowRedActivationsAggregationByApproach>();
            GetApproachYellowRedActivationsAggregationContainersForAllApporaches(yellowRedActivationsAggregationOptions, signal, options);
            LoadBins(null, null, options);
        }

        public YellowRedActivationsAggregationBySignal(
            ApproachYellowRedActivationsAggregationOptions approachYellowRedActivationsAggregationOptions,
            Location signal,
            int phaseNumber,
            IApproachYellowRedActivationAggregationRepository approachYellowRedActivationAggregationRepository,
            AggregationOptions options
            ) : base(approachYellowRedActivationsAggregationOptions, signal, options)
        {
            this.approachYellowRedActivationAggregationRepository = approachYellowRedActivationAggregationRepository;
            ApproachYellowRedActivationsures = new List<YellowRedActivationsAggregationByApproach>();
            foreach (var approach in signal.Approaches)
                if (approach.ProtectedPhaseNumber == phaseNumber)
                {
                    ApproachYellowRedActivationsures.Add(
                        new YellowRedActivationsAggregationByApproach(
                            approach,
                            approachYellowRedActivationsAggregationOptions,
                            options.Start,
                            options.End,
                            true,
                            options.DataType,
                            approachYellowRedActivationAggregationRepository,
                            options
                            ));
                    if (approach.PermissivePhaseNumber != null && approach.PermissivePhaseNumber == phaseNumber)
                        ApproachYellowRedActivationsures.Add(
                            new YellowRedActivationsAggregationByApproach(
                                approach,
                                approachYellowRedActivationsAggregationOptions,
                                options.Start,
                                options.End,
                                false,
                                options.DataType,
                                approachYellowRedActivationAggregationRepository,
                                options
                                ));
                }
            LoadBins(null, null, options);
        }

        public YellowRedActivationsAggregationBySignal(ApproachYellowRedActivationsAggregationOptions approachYellowRedActivationsAggregationOptions,
            Location signal,
            DirectionTypes direction,
            AggregationOptions options
            ) : base(approachYellowRedActivationsAggregationOptions, signal, options)
        {
            ApproachYellowRedActivationsures = new List<YellowRedActivationsAggregationByApproach>();
            foreach (var approach in signal.Approaches)
                if (approach.DirectionType.Id == direction)
                {
                    ApproachYellowRedActivationsures.Add(
                        new YellowRedActivationsAggregationByApproach(
                            approach,
                            approachYellowRedActivationsAggregationOptions,
                            options.Start,
                            options.End,
                            true,
                            options.DataType,
                            approachYellowRedActivationAggregationRepository,
                            options
                            ));
                    if (approach.PermissivePhaseNumber != null)
                        ApproachYellowRedActivationsures.Add(
                            new YellowRedActivationsAggregationByApproach(
                                approach,
                                approachYellowRedActivationsAggregationOptions,
                                options.Start,
                                options.End,
                                false,
                                options.DataType,
                                approachYellowRedActivationAggregationRepository,
                                options
                                ));
                }
            LoadBins(null, null, options);
        }

        public List<YellowRedActivationsAggregationByApproach> ApproachYellowRedActivationsures { get; }

        protected override void LoadBins(SignalAggregationMetricOptions signalAggregationMetricOptions, Location signal, AggregationOptions options)
        {
            for (var i = 0; i < BinsContainers.Count; i++)
                for (var binIndex = 0; binIndex < BinsContainers[i].Bins.Count; binIndex++)
                {
                    var bin = BinsContainers[i].Bins[binIndex];
                    foreach (var approachYellowRedActivationsAggregationContainer in ApproachYellowRedActivationsures)
                        bin.Sum += approachYellowRedActivationsAggregationContainer.BinsContainers[i].Bins[binIndex].Sum;
                    bin.Average = ApproachYellowRedActivationsures.Count > 0
                        ? bin.Sum / ApproachYellowRedActivationsures.Count
                        : 0;
                }
        }

        protected override void LoadBins(ApproachAggregationMetricOptions? approachAggregationMetricOptions, Location? signal, AggregationOptions options)
        {
            for (var i = 0; i < BinsContainers.Count; i++)
                for (var binIndex = 0; binIndex < BinsContainers[i].Bins.Count; binIndex++)
                {
                    var bin = BinsContainers[i].Bins[binIndex];
                    foreach (var approachYellowRedActivationsAggregationContainer in ApproachYellowRedActivationsures)
                        bin.Sum += approachYellowRedActivationsAggregationContainer.BinsContainers[i].Bins[binIndex].Sum;
                    bin.Average = ApproachYellowRedActivationsures.Count > 0
                        ? bin.Sum / ApproachYellowRedActivationsures.Count
                        : 0;
                }
        }

        private void GetApproachYellowRedActivationsAggregationContainersForAllApporaches(
            ApproachYellowRedActivationsAggregationOptions yellowRedActivationsAggregationOptions, Location signal, AggregationOptions options)
        {
            foreach (var approach in signal.Approaches)
            {
                ApproachYellowRedActivationsures.Add(
                    new YellowRedActivationsAggregationByApproach(
                        approach,
                        yellowRedActivationsAggregationOptions,
                        options.Start,
                        options.End,
                        true,
                        options.DataType,
                        approachYellowRedActivationAggregationRepository,
                        options
                        ));
                if (approach.PermissivePhaseNumber != null)
                    ApproachYellowRedActivationsures.Add(
                        new YellowRedActivationsAggregationByApproach(
                            approach,
                            yellowRedActivationsAggregationOptions,
                            options.Start,
                            options.End,
                            false,
                            options.DataType,
                            approachYellowRedActivationAggregationRepository,
                            options
                            ));
            }
        }


        public double GetYellowRedActivationssByDirection(DirectionType direction)
        {
            double splitFails = 0;
            if (ApproachYellowRedActivationsures != null)
                splitFails = ApproachYellowRedActivationsures
                    .Where(a => a.Approach.DirectionType.Id == direction.Id)
                    .Sum(a => a.BinsContainers.FirstOrDefault().SumValue);
            return splitFails;
        }

        public int GetAverageYellowRedActivationssByDirection(DirectionType direction)
        {
            var approachYellowRedActivationsuresByDirection = ApproachYellowRedActivationsures
                .Where(a => a.Approach.DirectionType.Id == direction.Id);
            var splitFails = 0;
            if (approachYellowRedActivationsuresByDirection.Any())
                splitFails = Convert.ToInt32(Math.Round(approachYellowRedActivationsuresByDirection
                    .Average(a => a.BinsContainers.FirstOrDefault().SumValue)));
            return splitFails;
        }
    }
}