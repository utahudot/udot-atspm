using ATSPM.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    public class FilteredDetectorData : FilterStepBase
    {
        public FilteredDetectorData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.DetectorOff);
            filteredList.Add((int)DataLoggerEnum.DetectorOn);
        }
    }
}
