using ATSPM.Application.Analysis.PreemptionDetails;
using ATSPM.Data.Enums;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class CalculateDelay : PreemptiveProcessBase<DelayTimeValue>
    {
        public CalculateDelay(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            first = DataLoggerEnum.PreemptCallInputOn;
            second = DataLoggerEnum.PreemptEntryStarted;
        }
    }
}
