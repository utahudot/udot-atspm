using ATSPM.Application.Analysis.PreemptionDetails;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class CalculateDwellTime : PreemptiveProcessBase<DwellTimeValue>
    {
        public CalculateDwellTime(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            first = IndianaEnumerations.PreemptionBeginDwellService;
            second = IndianaEnumerations.PreemptionBeginExitInterval;
        }
    }
}
