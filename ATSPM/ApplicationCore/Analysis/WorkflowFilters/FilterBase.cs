using ATSPM.Data.Models;
using ATSPM.Domain.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Base class for <see cref="BroadcastBlock{T}"/> filters used in workflows
    /// </summary>
    public abstract class FilterBase : ProcessStepBase<IEnumerable<ControllerEventLog>, IEnumerable<ControllerEventLog>>
    {
        /// <inheritdoc/>
        public FilterBase(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions){}
    }
}
