using ATSPM.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis
{
    public class FilteredTimingActuationData : FilterStepBase
    {
        public FilteredTimingActuationData(DataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.PhaseBeginGreen);
            filteredList.Add((int)DataLoggerEnum.PhaseMinComplete);
            filteredList.Add((int)DataLoggerEnum.PhaseMaxOut);
            filteredList.Add((int)DataLoggerEnum.PhaseEndYellowChange);
            filteredList.Add((int)DataLoggerEnum.PhaseEndRedClearance);
            filteredList.Add((int)DataLoggerEnum.PedestrianBeginWalk);
            filteredList.Add((int)DataLoggerEnum.PedestrianBeginChangeInterval);
            filteredList.Add((int)DataLoggerEnum.PedestrianBeginSolidDontWalk);
            filteredList.Add((int)DataLoggerEnum.OverlapBeginGreen);
            filteredList.Add((int)DataLoggerEnum.OverlapBeginTrailingGreenExtension);
            filteredList.Add((int)DataLoggerEnum.OverlapBeginYellow);
            filteredList.Add((int)DataLoggerEnum.OverlapBeginRedClearance);
            filteredList.Add((int)DataLoggerEnum.OverlapOffInactivewithredindication);
            filteredList.Add((int)DataLoggerEnum.PedestrianOverlapBeginWalk);
            filteredList.Add((int)DataLoggerEnum.PedestrianOverlapBeginClearance);
            filteredList.Add((int)DataLoggerEnum.PedestrianOverlapBeginSolidDontWalk);
            filteredList.Add((int)DataLoggerEnum.DetectorOff);
            filteredList.Add((int)DataLoggerEnum.DetectorOn);
            filteredList.Add((int)DataLoggerEnum.PedDetectorOff);
            filteredList.Add((int)DataLoggerEnum.PedDetectorOn);
        }
    }
}
