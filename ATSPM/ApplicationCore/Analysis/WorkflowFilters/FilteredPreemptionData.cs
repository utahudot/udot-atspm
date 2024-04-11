using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="IndianaEnumerations.PreemptCallInputOn"/></item>
    /// <item><see cref="IndianaEnumerations.PreemptGateDownInputReceived"/></item>
    /// <item><see cref="IndianaEnumerations.PreemptCallInputOff"/></item>
    /// <item><see cref="IndianaEnumerations.PreemptEntryStarted"/></item>
    /// <item><see cref="IndianaEnumerations.PreemptionBeginTrackClearance"/></item>
    /// <item><see cref="IndianaEnumerations.PreemptionBeginDwellService"/></item>
    /// <item><see cref="IndianaEnumerations.PreemptionMaxPresenceExceeded"/></item>
    /// <item><see cref="IndianaEnumerations.PreemptionBeginExitInterval"/></item>
    /// </list>
    /// </summary>
    public class FilteredPreemptionData : FilterEventCodeLocationBase
    {
        /// <inheritdoc/>
        public FilteredPreemptionData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)IndianaEnumerations.PreemptCallInputOn);
            filteredList.Add((int)IndianaEnumerations.PreemptGateDownInputReceived);
            filteredList.Add((int)IndianaEnumerations.PreemptCallInputOff);
            filteredList.Add((int)IndianaEnumerations.PreemptEntryStarted);
            filteredList.Add((int)IndianaEnumerations.PreemptionBeginTrackClearance);
            filteredList.Add((int)IndianaEnumerations.PreemptionBeginDwellService);
            filteredList.Add((int)IndianaEnumerations.PreemptionMaxPresenceExceeded);
            filteredList.Add((int)IndianaEnumerations.PreemptionBeginExitInterval);
        }
    }
}
