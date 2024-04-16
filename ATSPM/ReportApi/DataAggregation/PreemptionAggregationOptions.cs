using ATSPM.Application.Business.Aggregation;
using ATSPM.Application.Business.Bins;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data.Models;
using MOE.Common.Business.DataAggregation;

namespace MOE.Common.Business.WCFServiceLibrary
{
    public enum PreemptionDataTypes
    {
        PreemptNumber,
        PreemptRequests,
        PreemptServices
    }
    //AggregatedDataTypes = new List<AggregatedDataType>
    //        {
    //            new AggregatedDataType { Id = 0, DataName = "PreemptNumber" },
    //            new AggregatedDataType { Id = 1, DataName = "PreemptRequests" },
    //            new AggregatedDataType { Id = 2, DataName = "PreemptServices" }
    //        };

    public class PreemptionAggregationOptions : SignalAggregationMetricOptions
    {
        private readonly IPreemptionAggregationRepository preemptionAggregationRepository;

        public PreemptionAggregationOptions(
            IPreemptionAggregationRepository preemptionAggregationRepository,
            ILocationRepository locationRepository,
            ILogger<PhaseTerminationAggregationOptions> logger) : base(locationRepository, logger)
        {

            this.preemptionAggregationRepository = preemptionAggregationRepository;
        }

        //public override string YAxisTitle => SelectedAggregationType + " of " + Regex.Replace(
        //                                         SelectedAggregatedDataType.DataName,
        //                                         @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $1") + " " +
        //                                     TimePeriodOptions.SelectedBinSize + " bins";

        protected override List<BinsContainer> GetBinsContainersBySignal(Location signal, AggregationOptions options)
        {
            var aggregationBySignal = new PreemptionAggregationBySignal(this, signal, preemptionAggregationRepository, options);
            return aggregationBySignal.BinsContainers;
        }

        public override List<BinsContainer> GetBinsContainersByRoute(List<Location> signals, AggregationOptions options)
        {
            var binsContainers = BinFactory.GetBins(options.TimeOptions);
            foreach (var signal in signals)
            {
                var preemptionAggregationBySignal = new PreemptionAggregationBySignal(this, signal, preemptionAggregationRepository, options);
                PopulateBinsForRoute(signals, binsContainers, preemptionAggregationBySignal);
            }
            return binsContainers;
        }

    }
}