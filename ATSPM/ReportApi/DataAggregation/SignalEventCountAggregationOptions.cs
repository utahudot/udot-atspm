using ATSPM.Application.Business.Aggregation;
using ATSPM.Application.Business.Bins;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data.Models;
using MOE.Common.Business.DataAggregation;

namespace MOE.Common.Business.WCFServiceLibrary
{
    public enum SignalEventCountDataTypes
    {
        EventCount
    }
    //AggregatedDataTypes = new List<AggregatedDataType>
    //{
    //    new AggregatedDataType { Id = 0, DataName = "EventCount" }
    //};

    public class SignalEventCountAggregationOptions : SignalAggregationMetricOptions
    {
        private readonly ISignalEventCountAggregationRepository signalEventCountAggregationRepository;

        public SignalEventCountAggregationOptions(
            ISignalEventCountAggregationRepository signalEventCountAggregationRepository,
            ILocationRepository locationRepository,
            ILogger<PhaseTerminationAggregationOptions> logger) : base(locationRepository, logger)
        {
            this.signalEventCountAggregationRepository = signalEventCountAggregationRepository;
        }

        //public SignalEventCountAggregationOptions(SignalAggregationMetricOptions options)
        //{
        //    AggregatedDataTypes = new List<AggregatedDataType>
        //    {
        //        new AggregatedDataType { Id = 0, DataName = "EventCount" }
        //    };
        //    CopySignalAggregationBaseValues(options);
        //}

        //public override string YAxisTitle => SelectedAggregationType + " of " + Regex.Replace(
        //                                         SelectedAggregatedDataType.DataName,
        //                                         @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $1") + " " +
        //                                     TimePeriodOptions.SelectedBinSize + " bins";





        protected override List<BinsContainer> GetBinsContainersBySignal(Location signal, AggregationOptions options)
        {
            var aggregationBySignal = new SignalEventCountAggregationBySignal(this, signal, signalEventCountAggregationRepository, options);
            return aggregationBySignal.BinsContainers;
        }

        public override List<BinsContainer> GetBinsContainersByRoute(List<Location> signals, AggregationOptions options)
        {
            var binsContainers = BinFactory.GetBins(options.TimeOptions);
            foreach (var signal in signals)
            {
                var eventCountAggregationBySignal = new SignalEventCountAggregationBySignal(this, signal, signalEventCountAggregationRepository, options);
                PopulateBinsForRoute(signals, binsContainers, eventCountAggregationBySignal);
            }
            return binsContainers;
        }

    }
}