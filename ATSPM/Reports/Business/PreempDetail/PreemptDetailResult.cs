using ATSPM.Application.Reports.Business.PreemptService;
using Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.PreempDetail
{
    /// <summary>
    /// Preempt Detail chart
    /// </summary>
    public class PreemptDetailResult : SignalResult
    {
        public PreemptDetailResult(
            string signalId,
            DateTime start,
            DateTime end,
            ICollection<PreemptDetail> preemptDetails) : base(signalId, start, end)
        {
            PreemptDetails = preemptDetails;
        }
        public ICollection<PreemptDetail> PreemptDetails { get; set; }
    }

    public class PreemptDetail : SignalResult
    {
        public PreemptDetail(
            string signalId,
            DateTime start,
            DateTime end,
            int preemptNumber,
            ICollection<PreemptCycle> preemptCycles) : base(signalId, start, end)
        {
            PreemptNumber = preemptNumber;
            PreemptCycles = preemptCycles;
        }
        public int PreemptNumber { get; set; }
        public ICollection<PreemptCycle> PreemptCycles { get; }
    }
}