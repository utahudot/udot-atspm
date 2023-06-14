using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="DataLoggerEnum.PhaseBeginGreen"/></item>
    /// <item><see cref="DataLoggerEnum.PhaseMinComplete"/></item>
    /// <item><see cref="DataLoggerEnum.PhaseMaxOut"/></item>
    /// <item><see cref="DataLoggerEnum.PhaseEndYellowChange"/></item>
    /// <item><see cref="DataLoggerEnum.PhaseEndRedClearance"/></item>
    /// <item><see cref="DataLoggerEnum.PedestrianBeginWalk"/></item>
    /// <item><see cref="DataLoggerEnum.PedestrianBeginChangeInterval"/></item>
    /// <item><see cref="DataLoggerEnum.PedestrianBeginSolidDontWalk"/></item>
    /// <item><see cref="DataLoggerEnum.OverlapBeginGreen"/></item>
    /// <item><see cref="DataLoggerEnum.OverlapBeginTrailingGreenExtension"/></item>
    /// <item><see cref="DataLoggerEnum.OverlapBeginYellow"/></item>
    /// <item><see cref="DataLoggerEnum.OverlapBeginRedClearance"/></item>
    /// <item><see cref="DataLoggerEnum.OverlapOffInactivewithredindication"/></item>
    /// <item><see cref="DataLoggerEnum.PedestrianOverlapBeginWalk"/></item>
    /// <item><see cref="DataLoggerEnum.PedestrianOverlapBeginClearance"/></item>
    /// <item><see cref="DataLoggerEnum.PedestrianOverlapBeginSolidDontWalk"/></item>
    /// <item><see cref="DataLoggerEnum.DetectorOff"/></item>
    /// <item><see cref="DataLoggerEnum.DetectorOn"/></item>
    /// <item><see cref="DataLoggerEnum.PedDetectorOff"/></item>
    /// <item><see cref="DataLoggerEnum.PedDetectorOn"/></item>
    /// </list>
    /// </summary>
    public class FilteredTimingActuationData : FilterStepBase
    {
        /// <inheritdoc/>
        public FilteredTimingActuationData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
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
