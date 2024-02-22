using System;

namespace ATSPM.ReportApi.Business.Common
{
    public class DataPointForInt : DataPointBase
    {
        public DataPointForInt(DateTime start, int value) : base(start)
        {
            Timestamp = start;
            Value = value;
        }
        public int Value { get; set; }
    }
}
