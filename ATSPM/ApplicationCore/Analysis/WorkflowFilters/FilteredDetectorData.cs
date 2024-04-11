using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Workflows;
using System.Collections.Generic;
using System;
using System.Threading.Tasks.Dataflow;
using System.Linq;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Filters <see cref="ControllerEventLog"/> workflow events to
    /// <list type="bullet">
    /// <item><see cref="IndianaEnumerations.DetectorOff"/></item>
    /// <item><see cref="IndianaEnumerations.DetectorOn"/></item>
    /// </list>
    /// </summary>
    public class FilteredDetectorData : FilterEventCodeLocationBase
    {
        /// <inheritdoc/>
        public FilteredDetectorData(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)IndianaEnumerations.DetectorOff);
            filteredList.Add((int)IndianaEnumerations.DetectorOn);
        }
    }
}
