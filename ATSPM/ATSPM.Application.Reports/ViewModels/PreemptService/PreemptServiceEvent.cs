using System;

namespace ATSPM.Application.Reports.ViewModels.PreemptService
{
    public class PreemptServiceEvent
    {
        public PreemptServiceEvent(DateTime startTime, int eventParam)
        {
            StartTime = startTime;
            EventParam = eventParam;
        }

        public DateTime StartTime { get; set; }
        public int EventParam { get; set; }
    }
}