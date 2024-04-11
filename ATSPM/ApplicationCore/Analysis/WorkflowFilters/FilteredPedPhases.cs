using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="IndianaEnumerations.PedestrianBeginWalk"/></item>
    /// <item><see cref="IndianaEnumerations.PedestrianBeginSolidDontWalk"/></item>
    /// </list>
    /// </summary>
    public class FilteredPedPhases : FilterEventCodeBase
    {
        /// <inheritdoc/>
        public FilteredPedPhases(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)IndianaEnumerations.PedestrianBeginWalk);
            filteredList.Add((int)IndianaEnumerations.PedestrianBeginSolidDontWalk);
        }
    }
}
