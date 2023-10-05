using System;

namespace Reports.Business.Common
{
    public class DataPointForInt : DataPointBase
    {
        public DataPointForInt(DateTime start, int value) : base(start)
        {
            TimeStamp = start;
            Value = value;
        }
        public int Value { get; set; }
    }
}
