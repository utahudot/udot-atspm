using System;

namespace ATSPM.Application.Business.Common
{
    public class AggregationDataPoint
    {
        public string Identifier { get; set; }

        public DateTime Start { get; set; }

        public double Value { get; set; }
    }
}
