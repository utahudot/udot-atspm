using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="DataLoggerEnum.TSPCheckIn"/></item>
    /// <item><see cref="DataLoggerEnum.TSPAdjustmenttoEarlyGreen"/></item>
    /// <item><see cref="DataLoggerEnum.TSPAdjustmenttoExtendGreen"/></item>
    /// <item><see cref="DataLoggerEnum.TSPCheckOut"/></item>
    /// </list>
    /// </summary>
    public class FilterTspPriorityData : FilterEventCodeSignalBase
    {
        /// <inheritdoc/>
        public FilterTspPriorityData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.TSPCheckIn);
            filteredList.Add((int)DataLoggerEnum.TSPAdjustmenttoEarlyGreen);
            filteredList.Add((int)DataLoggerEnum.TSPAdjustmenttoExtendGreen);
            filteredList.Add((int)DataLoggerEnum.TSPCheckOut);
        }
    }
}
