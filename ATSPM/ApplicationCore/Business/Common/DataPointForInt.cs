using System;

namespace ATSPM.Application.Business.Common
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
