using System;

namespace Reports.Business.Common
{
    public class DataPointForDouble : DataPointBase
    {
        public DataPointForDouble(DateTime start, double value) : base(start)
        {
            Value = value;
        }

        public Double Value { get; set; }
    }
}
