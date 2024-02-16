using ATSPM.Data.Enums;
using ATSPM.Data.Models.EventLogModels;
using ATSPM.ReportApi.Business.Common;

namespace ATSPM.ReportApi.Business.PreemptService
{
    public class PreemptServiceService
    {
        private readonly PlanService planService;
        public PreemptServiceService(PlanService planService)
        {
            this.planService = planService;
        }

        public PreemptServiceResult GetChartData(
            PreemptServiceOptions options,
            IReadOnlyList<IndianaEvent> planEvents,
            IReadOnlyList<IndianaEvent> preemptEvents)
        {
            IReadOnlyList<Plan> plans = planService.GetBasicPlans(options.Start, options.End, options.locationIdentifier, planEvents);
            var preemptPlans = plans.Select(pl => new PreemptPlan(
                pl.PlanNumber.ToString(),
                pl.Start,
                pl.End,
                preemptEvents.Count(p => p.EventCode == DataLoggerEnum.PreemptEntryStarted && p.Timestamp >= pl.Start && p.Timestamp < pl.End))).ToList();

            return new PreemptServiceResult(
                options.locationIdentifier,
                options.Start,
                options.End,
                preemptPlans,
                preemptEvents.Select(p => new DataPointForInt(p.Timestamp, p.EventParam)).ToList()
                );
        }
    }
}