using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="IndianaEnumerations.PhaseBeginGreen"/></item>
    /// <item><see cref="IndianaEnumerations.PhaseMinComplete"/></item>
    /// <item><see cref="IndianaEnumerations.PhaseMaxOut"/></item>
    /// <item><see cref="IndianaEnumerations.PhaseEndYellowChange"/></item>
    /// <item><see cref="IndianaEnumerations.PhaseEndRedClearance"/></item>
    /// <item><see cref="IndianaEnumerations.PedestrianBeginWalk"/></item>
    /// <item><see cref="IndianaEnumerations.PedestrianBeginChangeInterval"/></item>
    /// <item><see cref="IndianaEnumerations.PedestrianBeginSolidDontWalk"/></item>
    /// <item><see cref="IndianaEnumerations.OverlapBeginGreen"/></item>
    /// <item><see cref="IndianaEnumerations.OverlapBeginTrailingGreenExtension"/></item>
    /// <item><see cref="IndianaEnumerations.OverlapBeginYellow"/></item>
    /// <item><see cref="IndianaEnumerations.OverlapBeginRedClearance"/></item>
    /// <item><see cref="IndianaEnumerations.OverlapOffInactivewithredindication"/></item>
    /// <item><see cref="IndianaEnumerations.PedestrianOverlapBeginWalk"/></item>
    /// <item><see cref="IndianaEnumerations.PedestrianOverlapBeginClearance"/></item>
    /// <item><see cref="IndianaEnumerations.PedestrianOverlapBeginSolidDontWalk"/></item>
    /// <item><see cref="IndianaEnumerations.DetectorOff"/></item>
    /// <item><see cref="IndianaEnumerations.DetectorOn"/></item>
    /// <item><see cref="IndianaEnumerations.PedDetectorOff"/></item>
    /// <item><see cref="IndianaEnumerations.PedDetectorOn"/></item>
    /// </list>
    /// </summary>
    public class FilteredTimingActuationData : FilterEventCodeBase
    {
        /// <inheritdoc/>
        public FilteredTimingActuationData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)IndianaEnumerations.PhaseBeginGreen);
            filteredList.Add((int)IndianaEnumerations.PhaseMinComplete);
            filteredList.Add((int)IndianaEnumerations.PhaseMaxOut);
            filteredList.Add((int)IndianaEnumerations.PhaseEndYellowChange);
            filteredList.Add((int)IndianaEnumerations.PhaseEndRedClearance);
            filteredList.Add((int)IndianaEnumerations.PedestrianBeginWalk);
            filteredList.Add((int)IndianaEnumerations.PedestrianBeginChangeInterval);
            filteredList.Add((int)IndianaEnumerations.PedestrianBeginSolidDontWalk);
            filteredList.Add((int)IndianaEnumerations.OverlapBeginGreen);
            filteredList.Add((int)IndianaEnumerations.OverlapBeginTrailingGreenExtension);
            filteredList.Add((int)IndianaEnumerations.OverlapBeginYellow);
            filteredList.Add((int)IndianaEnumerations.OverlapBeginRedClearance);
            filteredList.Add((int)IndianaEnumerations.OverlapOffInactivewithredindication);
            filteredList.Add((int)IndianaEnumerations.PedestrianOverlapBeginWalk);
            filteredList.Add((int)IndianaEnumerations.PedestrianOverlapBeginClearance);
            filteredList.Add((int)IndianaEnumerations.PedestrianOverlapBeginSolidDontWalk);
            filteredList.Add((int)IndianaEnumerations.DetectorOff);
            filteredList.Add((int)IndianaEnumerations.DetectorOn);
            filteredList.Add((int)IndianaEnumerations.PedDetectorOff);
            filteredList.Add((int)IndianaEnumerations.PedDetectorOn);
        }
    }
}
