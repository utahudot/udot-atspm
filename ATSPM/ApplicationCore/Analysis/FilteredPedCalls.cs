using ATSPM.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis
{
    public class FilteredPedCalls : FilterStepBase
    {
        public FilteredPedCalls(DataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.PedestrianCallRegistered);
            filteredList.Add((int)DataLoggerEnum.PedDetectorOn);
        }
    }
}
