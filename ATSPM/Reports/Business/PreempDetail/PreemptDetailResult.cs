using ATSPM.Application.Reports.Business.PreemptService;
using Reports.Business.Common;
using Reports.Business.PreempDetail;
using Reports.Business.PreemptService;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.PreempDetail
{
    /// <summary>
    /// Preempt Detail chart
    /// </summary>
    public class PreemptDetailResult
    {
        public PreemptDetailResult(
            ICollection<PreemptDetail> preemptDetails,
            PreemptRequestAndServices preemptSummary)
        {
            Details = preemptDetails;
            Summary = preemptSummary;
        }
        public ICollection<PreemptDetail> Details { get; set; }

        public PreemptRequestAndServices Summary { get; set; }
    }

    public class PreemptDetail : SignalResult
    {
        public PreemptDetail(
            string signalId,
            DateTime start,
            DateTime end,
            int preemptNumber,
            ICollection<PreemptCycleResult> preemptCycles) : base(signalId, start, end)
        {
            PreemptionNumber = preemptNumber;
            Cycles = preemptCycles;
        }
        public int PreemptionNumber { get; set; }
        public ICollection<PreemptCycleResult> Cycles { get; }
    }
}