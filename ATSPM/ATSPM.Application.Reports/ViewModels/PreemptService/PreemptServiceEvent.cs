using System;

namespace ATSPM.Application.Reports.ViewModels.PreemptService
{
    public class PreemptServiceEvent
    {
        public DateTime StartTime { get; internal set; }
        public int EventParam { get; internal set; }
    }
}