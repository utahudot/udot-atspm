using ATSPM.Domain.Common;

namespace ATSPM.Application.Analysis.Plans
{
    public interface IPlan : IStartEndRange
    {
        string SignalId { get; set; }
        int PlanNumber { get; set; }
        bool TryAssignToPlan(IStartEndRange range);
    }
}
