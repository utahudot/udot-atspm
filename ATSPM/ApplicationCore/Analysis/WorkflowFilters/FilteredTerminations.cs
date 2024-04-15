using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="4"/></item>
    /// <item><see cref="5"/></item>
    /// <item><see cref="6"/></item>
    /// <item><see cref="IndianaEnumerations.PhaseGreenTermination"/></item>
    /// </list>
    /// </summary>
    public class FilteredTerminations : FilterEventCodeLocationBase
    {
        /// <inheritdoc/>
        public FilteredTerminations(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)4);
            filteredList.Add((int)5);
            filteredList.Add((int)6);
            filteredList.Add((int)IndianaEnumerations.PhaseGreenTermination);
        }
    }
}
