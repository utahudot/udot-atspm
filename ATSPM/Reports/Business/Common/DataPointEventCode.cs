using System;

namespace Reports.Business.Common
{
    public class DataPointEventCode
    {
        public DataPointEventCode(DateTime startTime, int eventCode)
        {
            StartTime = startTime;
            EventCode = eventCode;
        }

        public DateTime StartTime { get; set; }
        public int EventCode { get; set; }
    }
}
