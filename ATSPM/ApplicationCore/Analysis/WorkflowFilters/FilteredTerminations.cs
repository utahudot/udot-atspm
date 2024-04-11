using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="IndianaEnumerations.PhaseGapOut"/></item>
    /// <item><see cref="IndianaEnumerations.PhaseMaxOut"/></item>
    /// <item><see cref="IndianaEnumerations.PhaseForceOff"/></item>
    /// <item><see cref="IndianaEnumerations.PhaseGreenTermination"/></item>
    /// </list>
    /// </summary>
    public class FilteredTerminations : FilterEventCodeLocationBase
    {
        /// <inheritdoc/>
        public FilteredTerminations(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)IndianaEnumerations.PhaseGapOut);
            filteredList.Add((int)IndianaEnumerations.PhaseMaxOut);
            filteredList.Add((int)IndianaEnumerations.PhaseForceOff);
            filteredList.Add((int)IndianaEnumerations.PhaseGreenTermination);
        }
    }
}
