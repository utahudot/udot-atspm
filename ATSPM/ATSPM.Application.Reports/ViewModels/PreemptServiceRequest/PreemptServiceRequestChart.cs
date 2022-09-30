using ATSPM.Application.Reports.ViewModels;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.ViewModels.PreemptServiceRequest
{
    /// <summary>
    /// Preempt Service Request chart
    /// </summary>
    public class PreemptServiceRequest
    {
        public string ChartName { get; internal set; }
        public string SignalId { get; internal set; }
        public string SignalLocation { get; internal set; }
        public DateTime Start { get; internal set; }
        public DateTime End { get; internal set; }
        public ICollection<Plan> Plans { get; internal set; }
        public ICollection<PreemptRequest> PreemptRequests { get; internal set; }
    }
}