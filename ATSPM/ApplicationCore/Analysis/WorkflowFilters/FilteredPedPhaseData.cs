using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="IndianaEnumerations.PedestrianBeginWalk"/></item>
    /// <item><see cref="IndianaEnumerations.PedestrianBeginChangeInterval"/></item>
    /// <item><see cref="IndianaEnumerations.PedestrianOverlapBeginWalk"/></item>
    /// <item><see cref="IndianaEnumerations.PedestrianOverlapBeginClearance"/></item>
    /// </list>
    /// </summary>
    public class FilteredPedPhaseData : FilterEventCodeBase
    {
        /// <inheritdoc/>
        public FilteredPedPhaseData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)IndianaEnumerations.PedestrianBeginWalk);
            filteredList.Add((int)IndianaEnumerations.PedestrianBeginChangeInterval);
            filteredList.Add((int)IndianaEnumerations.PedestrianOverlapBeginWalk);
            filteredList.Add((int)IndianaEnumerations.PedestrianOverlapBeginClearance);
        }
    }
}
