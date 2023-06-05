using ATSPM.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    public class FilteredPlanData : FilterStepBase
    {
        public FilteredPlanData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.CoordPatternChange);
        }
    }
}
