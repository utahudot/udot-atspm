using ATSPM.Domain.Common;
using System.Text.Json;

namespace ATSPM.Application.Analysis.Plans
{
    public abstract class Plan : StartEndRange, IPlan
    {
        public string SignalId { get; set; }
        public int PlanNumber { get; set; }

        public abstract bool TryAssignToPlan(IStartEndRange range);

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
