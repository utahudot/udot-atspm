using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="IndianaEnumerations.PedestrianCallRegistered"/></item>
    /// <item><see cref="IndianaEnumerations.PedDetectorOn"/></item>
    /// </list>
    /// </summary>
    public class FilteredPedCalls : FilterEventCodeBase
    {
        /// <inheritdoc/>
        public FilteredPedCalls(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)IndianaEnumerations.PedestrianCallRegistered);
            filteredList.Add((int)IndianaEnumerations.PedDetectorOn);
        }
    }
}
