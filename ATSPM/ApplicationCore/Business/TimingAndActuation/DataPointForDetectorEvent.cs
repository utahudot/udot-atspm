using System;

namespace ATSPM.ReportApi.Business.TimingAndActuation
{
    public class DataPointForDetectorEvent : DetectorEventBase
    {
        public DataPointForDetectorEvent(int value, DateTime start, DateTime stop) : base(start, stop)
        {
            Value = value;
        }

        public int Value { get; set; }
    }
}
