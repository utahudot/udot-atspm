using System;

namespace ATSPM.Application.Reports.Business.PreemptServiceRequest
{
    public class PreemptRequest
    {
        public PreemptRequest(DateTime startTime, int eventParam)
        {
            StartTime = startTime;
            EventParam = eventParam;
        }

        public DateTime StartTime { get; internal set; }
        public int EventParam { get; internal set; }
    }
}