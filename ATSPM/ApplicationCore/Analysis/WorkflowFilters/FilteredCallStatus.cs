using ATSPM.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    public class FilteredCallStatus : FilterStepBase
    {
        public FilteredCallStatus(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.PhaseCallRegistered);
            filteredList.Add((int)DataLoggerEnum.PhaseCallDropped);
        }
    }
}
