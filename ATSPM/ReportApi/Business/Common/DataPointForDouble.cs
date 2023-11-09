using System;

namespace ATSPM.ReportApi.Business.Common
{
    public class DataPointForDouble : DataPointBase
    {
        public DataPointForDouble(DateTime start, DateTime end) : base(start)
        {
            Value = (end - start).TotalSeconds;
        }

        public DataPointForDouble(DateTime start, double value) : base(start)
        {
            Value = value;
        }

        public double Value { get; set; }
    }
}
