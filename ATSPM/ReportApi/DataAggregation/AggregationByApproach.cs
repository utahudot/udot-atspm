using ATSPM.Application.Business.Aggregation;
using ATSPM.Application.Business.Bins;
using ATSPM.Data.Models;
using MOE.Common.Business.WCFServiceLibrary;

namespace MOE.Common.Business.DataAggregation
{
    public abstract class AggregationByApproach
    {
        public double Total
        {
            get { return BinsContainers.Sum(c => c.SumValue); }
        }


        public Approach Approach { get; }
        public List<BinsContainer> BinsContainers { get; set; } = new List<BinsContainer>();

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

        public AggregationByApproach(Approach approach, ApproachAggregationMetricOptions approachAggregationMetricOptions, DateTime startDate,
            DateTime endDate, bool getProtectedPhase, int dataType, AggregationOptions options)
        {
            Approach = approach;
            BinsContainers = BinFactory.GetBins(options.TimeOptions);
        }




        protected abstract void LoadBins(Approach approach, ApproachAggregationMetricOptions approachAggregationMetricOptions,
            bool getProtectedPhase, int dataType, AggregationOptions options);


    }
}