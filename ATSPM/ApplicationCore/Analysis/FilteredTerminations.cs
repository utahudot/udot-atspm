using ATSPM.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis
{
    public class FilteredTerminations : FilterStepBase
    {
        public FilteredTerminations(DataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.PhaseGapOut);
            filteredList.Add((int)DataLoggerEnum.PhaseMaxOut);
            filteredList.Add((int)DataLoggerEnum.PhaseForceOff);
        }
    }
}
