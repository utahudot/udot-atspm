using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="21"/></item>
    /// <item><see cref="22"/></item>
    /// <item><see cref="67"/></item>
    /// <item><see cref="68"/></item>
    /// </list>
    /// </summary>
    public class FilteredPedPhaseData : FilterEventCodeBase
    {
        /// <inheritdoc/>
        public FilteredPedPhaseData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)21);
            filteredList.Add((int)22);
            filteredList.Add((int)67);
            filteredList.Add((int)68);
        }
    }
}
