//using System;
//using System.Collections.Generic;
//using System.Runtime.Serialization;
//using System.Text.RegularExpressions;
//using Legacy.Common.Business.Bins;
//using Legacy.Common.Business.DataAggregation;

//namespace Legacy.Common.Business.WCFServiceLibrary
//{
//    [DataContract]
//    public class PreemptionAggregationOptions : SignalAggregationMetricOptions
//    {
//        public PreemptionAggregationOptions()
//        {
//            MetricTypeId = 22;
//            AggregatedDataTypes = new List<AggregatedDataType>();
//            AggregatedDataTypes.Add(new AggregatedDataType {Id = 0, DataName = "PreemptNumber"});
//            AggregatedDataTypes.Add(new AggregatedDataType {Id = 1, DataName = "PreemptRequests"});
//            AggregatedDataTypes.Add(new AggregatedDataType {Id = 2, DataName = "PreemptServices"});
//        }

//        public override string YAxisTitle => SelectedAggregationType + " of " + Regex.Replace(
//                                                 SelectedAggregatedDataType.DataName,
//                                                 @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $1") + " " +
//                                             TimeOptions.SelectedBinSize + " bins";

//        protected override List<BinsContainer> GetBinsContainersBySignal(Models.Signal signal)
//        {
//            var aggregationBySignal = new PreemptionAggregationBySignal(this, signal);
//            return aggregationBySignal.BinsContainers;
//        }

//        public override List<BinsContainer> GetBinsContainersByRoute(List<Models.Signal> signals)
//        {
//            var binsContainers = BinFactory.GetBins(TimeOptions);
//            foreach (var signal in signals)
//            {
//                var preemptionAggregationBySignal = new PreemptionAggregationBySignal(this, signal);
//                PopulateBinsForRoute(signals, binsContainers, preemptionAggregationBySignal);
//            }
//            return binsContainers;
//        }

//    }
//}