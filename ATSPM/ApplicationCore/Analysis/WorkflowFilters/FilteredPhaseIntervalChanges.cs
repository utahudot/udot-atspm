using ATSPM.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    public class FilteredPhaseIntervalChanges : FilterStepBase
    {
        public FilteredPhaseIntervalChanges(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.PhaseBeginGreen);
            filteredList.Add((int)DataLoggerEnum.PhaseBeginYellowChange);
            filteredList.Add((int)DataLoggerEnum.PhaseEndYellowChange);
            filteredList.Add((int)DataLoggerEnum.PhaseEndRedClearance);
        }
    }
}
