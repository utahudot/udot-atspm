using System;

namespace Reports.Business.Common
{
    public class DataPointForDouble
    {
        public DataPointForDouble(DateTime startTime, double seconds)
        {
            this.startTime = DateTime.SpecifyKind(startTime, DateTimeKind.Unspecified);
            Value = seconds;
        }

        private DateTime startTime;
        public DateTime StartTime
        {
            get
            {
                return startTime;
            }
            set
            {
                startTime = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
            }
        }
        public double Value { get; set; }
    }
}
