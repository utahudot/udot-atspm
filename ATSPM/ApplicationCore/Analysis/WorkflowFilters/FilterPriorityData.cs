using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="IndianaEnumerations.TSPCheckIn"/></item>
    /// <item><see cref="IndianaEnumerations.TSPAdjustmenttoEarlyGreen"/></item>
    /// <item><see cref="IndianaEnumerations.TSPAdjustmenttoExtendGreen"/></item>
    /// </list>
    /// </summary>
    public class FilterPriorityData : FilterEventCodeLocationBase
    {
        /// <inheritdoc/>
        public FilterPriorityData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)IndianaEnumerations.TSPCheckIn);
            filteredList.Add((int)IndianaEnumerations.TSPAdjustmenttoEarlyGreen);
            filteredList.Add((int)IndianaEnumerations.TSPAdjustmenttoExtendGreen);
        }
    }
}
