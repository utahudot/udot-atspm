using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.TempModels;

namespace ATSPM.ReportApi.Business.PreempDetail
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
            string locationId,
            DateTime start,
            DateTime end,
            int preemptNumber,
            ICollection<PreemptCycleResult> preemptCycles) : base(locationId, start, end)
        {
            PreemptionNumber = preemptNumber;
            Cycles = preemptCycles;
        }
        public int PreemptionNumber { get; set; }
        public ICollection<PreemptCycleResult> Cycles { get; }
    }
}