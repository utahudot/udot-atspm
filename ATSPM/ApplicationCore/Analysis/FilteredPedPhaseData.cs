using ATSPM.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis
{
    public class FilteredPedPhaseData : FilterStepBase
    {
        public FilteredPedPhaseData(DataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.PedestrianBeginWalk);
            filteredList.Add((int)DataLoggerEnum.PedestrianBeginChangeInterval);
            filteredList.Add((int)DataLoggerEnum.PedestrianOverlapBeginWalk);
            filteredList.Add((int)DataLoggerEnum.PedestrianOverlapBeginClearance);
        }
    }
}
