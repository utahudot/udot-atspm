using ATSPM.Data.Models.AggregationModels;
using MOE.Common.Business.WCFServiceLibrary;

namespace MOE.Common.Business.DataAggregation
{
    class SplitFailAggregationByRoute
    {
        public SplitFailAggregationByRoute(ApproachAggregationMetricOptions options)
        {
            Container = new List<SplitFailAggregationBySignal>();

            //foreach (var sig in options.Signals)
            //{
            //    options.SignalID = sig.SignalID;
            //    var locationRepository = MOE.Common.Models.Repositories.SignalsRepositoryFactory.Create();
            //    Location signal = locationRepository.GetVersionOfSignalByDate(sig.SignalID, options.StartDate);

            //    SpliFailAggregationBySignal signalAggregation = new SpliFailAggregationBySignal(options);

            //    Container.Add(signalAggregation);
            //}

        }

        public List<SplitFailAggregationBySignal> Container { get; }


    }


    public class RouteSplitFailAggregationContainer
    {
        public Route Route { get; set; }
        public List<ApproachSplitFailAggregation> SplitFails { get; }

        public RouteSplitFailAggregationContainer(ApproachAggregationMetricOptions options)
        {
            //Approach = approach;
            //var splitFailAggregationRepository =
            //    MOE.Common.Models.Repositories.ApproachSplitFailAggregationRepositoryFactory.Create();
            //SplitFails = splitFailAggregationRepository.GetApproachSplitFailAggregationByApproachIdAndDateRange(
            //    approach.ApproachID, start, end);
        }
    }
}
