using ATSPM.Application.Business.Aggregation;
using ATSPM.Application.Business.Bins;
using ATSPM.Data.Models;
using MOE.Common.Business.WCFServiceLibrary;

namespace MOE.Common.Business.DataAggregation
{
    public abstract class AggregationByPhase
    {
        //protected List<ApproachEventCountAggregation> ApproachEventCountAggregations { get; set; }
        public double Total
        {
            get { return BinsContainers.Sum(c => c.SumValue); }
        }


        public int PhaseNumber { get; }
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

        public AggregationByPhase(Location signal, int phaseNumber, PhaseAggregationMetricOptions phaseAggregationMetricOptions, int dataType, AggregationOptions options)
        {
            BinsContainers = BinFactory.GetBins(options.TimeOptions);
            PhaseNumber = phaseNumber;
            LoadBins(signal, phaseNumber, phaseAggregationMetricOptions, dataType, options);
        }


        protected abstract void LoadBins(Location signal, int phaseNumber, PhaseAggregationMetricOptions phaseAggregationMetricOptions, int dataType, AggregationOptions options);


    }
}