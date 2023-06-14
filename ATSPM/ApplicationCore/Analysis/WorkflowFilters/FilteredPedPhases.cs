using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="DataLoggerEnum.PedestrianBeginWalk"/></item>
    /// <item><see cref="DataLoggerEnum.PedestrianBeginSolidDontWalk"/></item>
    /// </list>
    /// </summary>
    public class FilteredPedPhases : FilterStepBase
    {
        /// <inheritdoc/>
        public FilteredPedPhases(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.PedestrianBeginWalk);
            filteredList.Add((int)DataLoggerEnum.PedestrianBeginSolidDontWalk);
        }
    }
}
