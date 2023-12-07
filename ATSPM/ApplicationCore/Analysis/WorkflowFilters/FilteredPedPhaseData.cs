using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="DataLoggerEnum.PedestrianBeginWalk"/></item>
    /// <item><see cref="DataLoggerEnum.PedestrianBeginChangeInterval"/></item>
    /// <item><see cref="DataLoggerEnum.PedestrianOverlapBeginWalk"/></item>
    /// <item><see cref="DataLoggerEnum.PedestrianOverlapBeginClearance"/></item>
    /// </list>
    /// </summary>
    public class FilteredPedPhaseData : FilterEventCodeBase
    {
        /// <inheritdoc/>
        public FilteredPedPhaseData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.PedestrianBeginWalk);
            filteredList.Add((int)DataLoggerEnum.PedestrianBeginChangeInterval);
            filteredList.Add((int)DataLoggerEnum.PedestrianOverlapBeginWalk);
            filteredList.Add((int)DataLoggerEnum.PedestrianOverlapBeginClearance);
        }
    }
}
