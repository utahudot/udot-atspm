using System;

namespace ATSPM.Application.Reports.ViewModels.PreemptServiceRequest
{
    public class PreemptRequest
    {
        public DateTime StartTime { get; internal set; }
        public int EventParam { get; internal set; }
    }
}