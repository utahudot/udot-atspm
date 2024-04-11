using ATSPM.Application.Analysis.PreemptionDetails;
using ATSPM.Data.Enums;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class CalculateTimeToCallMaxOut : PreemptiveProcessBase<TimeToCallMaxOutValue>
    {
        public CalculateTimeToCallMaxOut(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            first = IndianaEnumerations.PreemptCallInputOn;
            second = IndianaEnumerations.PreemptionMaxPresenceExceeded;
        }
    }
}
