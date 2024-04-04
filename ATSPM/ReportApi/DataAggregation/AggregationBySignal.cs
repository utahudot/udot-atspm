using ATSPM.Application.Business.Aggregation;
using ATSPM.Application.Business.Bins;
using ATSPM.Data.Models;
using ATSPM.Data.Models.AggregationModels;
using MOE.Common.Business.WCFServiceLibrary;

namespace MOE.Common.Business.DataAggregation
{
    public abstract class AggregationBySignal
    {

        public Location Signal { get; }

        public double Total
        {
            get { return BinsContainers.Sum(c => c.SumValue); }
        }

        public List<BinsContainer> BinsContainers { get; protected set; }
        public List<SignalEventCountAggregation> SignalEventCountAggregations { get; set; }

        public int Average
        {
            get
            {
                if (BinsContainers.Count > 1)
                    return Convert.ToInt32(Math.Round(BinsContainers.Average(b => b.SumValue)));
                double numberOfBins = 0;
                foreach (var binsContainer in BinsContainers)
                    numberOfBins += binsContainer.Bins.Count;
                return numberOfBins > 0 ? Convert.ToInt32(Math.Round(Total / numberOfBins)) : 0;
            }
        }

        public AggregationBySignal(SignalAggregationMetricOptions signalAggregationMetricOptions, Location signal, AggregationOptions options)
        {
            BinsContainers = BinFactory.GetBins(options.TimeOptions);
            Signal = signal;
        }

        protected abstract void LoadBins(SignalAggregationMetricOptions signalAggregationMetricOptions, Location signal, AggregationOptions options);

        protected abstract void LoadBins(ApproachAggregationMetricOptions approachAggregationMetricOptions, Location signal, AggregationOptions options);
    }
}