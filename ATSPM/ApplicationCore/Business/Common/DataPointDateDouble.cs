using System;

namespace ATSPM.Application.Business.Common
{
    public class DataPointDateDouble : IAggregationDataPoint
    {

        public DataPointDateDouble()
        {
        }

        public DataPointDateDouble(DateTime start, double value)
        {
            this.Start = start;
            this.Value = value;
        }

        public DateTime Start { get; set; }

        public double Value { get; set; }
    }
}
