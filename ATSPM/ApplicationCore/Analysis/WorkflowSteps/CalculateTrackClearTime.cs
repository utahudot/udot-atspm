using ATSPM.Application.Analysis.PreemptionDetails;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    public class CalculateTrackClearTime : PreemptiveProcessBase<TrackClearTimeValue>
    {
        public CalculateTrackClearTime(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            first = DataLoggerEnum.PreemptionBeginTrackClearance;
            second = DataLoggerEnum.PreemptionBeginDwellService;
        }
    }
}
