using ATSPM.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    public class FilteredPreemptionData : FilterStepBase
    {
        public FilteredPreemptionData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.PreemptCallInputOn);
            filteredList.Add((int)DataLoggerEnum.PreemptGateDownInputReceived);
            filteredList.Add((int)DataLoggerEnum.PreemptCallInputOff);
            filteredList.Add((int)DataLoggerEnum.PreemptEntryStarted);
            filteredList.Add((int)DataLoggerEnum.PreemptionBeginDwellService);
            filteredList.Add((int)DataLoggerEnum.PreemptionMaxPresenceExceeded);
            filteredList.Add((int)DataLoggerEnum.PreemptionBeginExitInterval);
        }
    }
}
