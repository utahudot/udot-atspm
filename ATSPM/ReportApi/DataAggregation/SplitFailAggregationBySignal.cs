using ATSPM.Application.Business.Aggregation;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using MOE.Common.Business.WCFServiceLibrary;

namespace MOE.Common.Business.DataAggregation
{
    public class SplitFailAggregationBySignal : AggregationBySignal
    {
        private readonly IApproachSplitFailAggregationRepository approachSplitFailAggregationRepository;

        public SplitFailAggregationBySignal(
            ApproachSplitFailAggregationOptions approachSplitFailAggregationOptions,
            Location signal,
            IApproachSplitFailAggregationRepository approachSplitFailAggregationRepository,
            AggregationOptions options
            ) : base(approachSplitFailAggregationOptions, signal, options)
        {
            this.approachSplitFailAggregationRepository = approachSplitFailAggregationRepository;
            ApproachSplitFailures = new List<SplitFailAggregationByApproach>();
            GetApproachSplitFailAggregationContainersForAllApporaches(approachSplitFailAggregationOptions, signal, options);
            LoadBins(null, null, options);
        }

        public SplitFailAggregationBySignal(
            ApproachSplitFailAggregationOptions approachSplitFailAggregationOptions,
            Location signal,
            int phaseNumber,
            IApproachSplitFailAggregationRepository approachSplitFailAggregationRepository,
            AggregationOptions options
            ) : base(approachSplitFailAggregationOptions, signal, options)
        {
            this.approachSplitFailAggregationRepository = approachSplitFailAggregationRepository;
            ApproachSplitFailures = new List<SplitFailAggregationByApproach>();
            foreach (var approach in signal.Approaches)
                if (approach.ProtectedPhaseNumber == phaseNumber)
                {
                    ApproachSplitFailures.Add(
                        new SplitFailAggregationByApproach(
                            approach,
                            approachSplitFailAggregationOptions,
                            options.Start,
                            options.End,
                            true,
                            options.DataType,
                            approachSplitFailAggregationRepository,
                            options
                            ));
                    if (approach.PermissivePhaseNumber != null && approach.PermissivePhaseNumber == phaseNumber)
                        ApproachSplitFailures.Add(
                            new SplitFailAggregationByApproach(
                                approach,
                                approachSplitFailAggregationOptions,
                                options.Start,
                                options.End,
                                false,
                                options.DataType,
                                approachSplitFailAggregationRepository,
                                options
                                ));
                }
            LoadBins(null, null, options);
        }

        public SplitFailAggregationBySignal(
            ApproachSplitFailAggregationOptions approachSplitFailAggregationOptions,
            Location signal,
            DirectionTypes direction,
            AggregationOptions options
            ) : base(approachSplitFailAggregationOptions, signal, options)
        {
            ApproachSplitFailures = new List<SplitFailAggregationByApproach>();
            foreach (var approach in signal.Approaches)
                if (approach.DirectionType.Id == direction)
                {
                    ApproachSplitFailures.Add(
                        new SplitFailAggregationByApproach(
                            approach,
                            approachSplitFailAggregationOptions,
                            options.Start,
                            options.End,
                            true,
                            options.DataType,
                            approachSplitFailAggregationRepository,
                            options
                            ));
                    if (approach.PermissivePhaseNumber != null)
                        ApproachSplitFailures.Add(
                            new SplitFailAggregationByApproach(
                                approach,
                                approachSplitFailAggregationOptions,
                                options.Start,
                                options.End,
                                false,
                                options.DataType,
                                approachSplitFailAggregationRepository,
                                options
                                ));
                }
            LoadBins(null, null, options);
        }

        public List<SplitFailAggregationByApproach> ApproachSplitFailures { get; }

        protected override void LoadBins(SignalAggregationMetricOptions signalAggregationMetricOptions, Location signal, AggregationOptions options)
        {
            for (var i = 0; i < BinsContainers.Count; i++)
                for (var binIndex = 0; binIndex < BinsContainers[i].Bins.Count; binIndex++)
                {
                    var bin = BinsContainers[i].Bins[binIndex];
                    foreach (var approachSplitFailAggregationContainer in ApproachSplitFailures)
                    {
                        bin.Sum += approachSplitFailAggregationContainer.BinsContainers[i].Bins[binIndex].Sum;
                        bin.Average = ApproachSplitFailures.Count > 0 ? bin.Sum / ApproachSplitFailures.Count : 0;
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
                    foreach (var approachSplitFailAggregationContainer in ApproachSplitFailures)
                        if (approachSplitFailAggregationContainer.BinsContainers.Count > 0)
                            bin.Sum += approachSplitFailAggregationContainer.BinsContainers[i].Bins[binIndex].Sum;
                    bin.Average = ApproachSplitFailures.Count > 0 ? bin.Sum / ApproachSplitFailures.Count : 0;
                }
            }
        }

        private void GetApproachSplitFailAggregationContainersForAllApporaches(
            ApproachSplitFailAggregationOptions approachSplitFailAggregation, Location signal, AggregationOptions options)
        {
            foreach (var approach in signal.Approaches)
            {
                ApproachSplitFailures.Add(
                    new SplitFailAggregationByApproach(
                        approach,
                        approachSplitFailAggregation,
                        options.TimeOptions.Start,
                        options.TimeOptions.End,
                        true,
                        options.DataType,
                        approachSplitFailAggregationRepository,
                        options
                        ));
                if (approach.PermissivePhaseNumber != null)
                    ApproachSplitFailures.Add(
                        new SplitFailAggregationByApproach(
                            approach,
                            approachSplitFailAggregation,
                            options.Start,
                            options.End,
                            false,
                            options.DataType,
                            approachSplitFailAggregationRepository,
                            options
                            ));
            }
        }

        public double GetSplitFailsByDirection(DirectionTypes direction)
        {
            double splitFails = 0;
            if (ApproachSplitFailures != null)
                splitFails = ApproachSplitFailures
                    .Where(a => a.Approach.DirectionType.Id == direction)
                    .Sum(a => a.BinsContainers.FirstOrDefault().SumValue);
            return splitFails;
        }

        public int GetAverageSplitFailsByDirection(DirectionTypes direction)
        {
            var approachSplitFailuresByDirection = ApproachSplitFailures
                .Where(a => a.Approach.DirectionType.Id == direction);
            var splitFails = 0;
            if (approachSplitFailuresByDirection.Any())
                splitFails = Convert.ToInt32(Math.Round(approachSplitFailuresByDirection
                    .Average(a => a.BinsContainers.FirstOrDefault().SumValue)));
            return splitFails;
        }
    }
}