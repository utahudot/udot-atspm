using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="DataLoggerEnum.PreemptCallInputOn"/></item>
    /// <item><see cref="DataLoggerEnum.PreemptGateDownInputReceived"/></item>
    /// <item><see cref="DataLoggerEnum.PreemptCallInputOff"/></item>
    /// <item><see cref="DataLoggerEnum.PreemptEntryStarted"/></item>
    /// <item><see cref="DataLoggerEnum.PreemptionBeginDwellService"/></item>
    /// <item><see cref="DataLoggerEnum.PreemptionMaxPresenceExceeded"/></item>
    /// <item><see cref="DataLoggerEnum.PreemptionBeginExitInterval"/></item>
    /// </list>
    /// </summary>
    public class FilteredPreemptionData : FilterStepBase
    {
        /// <inheritdoc/>
        public FilteredPreemptionData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.PreemptCallInputOn);
            filteredList.Add((int)DataLoggerEnum.PreemptGateDownInputReceived);
            filteredList.Add((int)DataLoggerEnum.PreemptCallInputOff);
            filteredList.Add((int)DataLoggerEnum.PreemptEntryStarted);
            filteredList.Add((int)DataLoggerEnum.PreemptionBeginDwellService);
            filteredList.Add((int)DataLoggerEnum.PreemptionMaxPresenceExceeded);
            filteredList.Add((int)DataLoggerEnum.PreemptionBeginExitInterval);
        }
    }
}
