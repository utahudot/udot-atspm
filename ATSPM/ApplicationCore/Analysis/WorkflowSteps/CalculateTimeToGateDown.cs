using ATSPM.Application.Analysis.PreemptionDetails;
using ATSPM.Data.Enums;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class CalculateTimeToGateDown : PreemptiveProcessBase<TimeToGateDownValue>
    {
        public CalculateTimeToGateDown(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            first = DataLoggerEnum.PreemptCallInputOn;
            second = DataLoggerEnum.PreemptGateDownInputReceived;
        }
    }
}
