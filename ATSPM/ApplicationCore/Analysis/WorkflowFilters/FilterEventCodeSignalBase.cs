using ATSPM.Application.Specifications;
using ATSPM.Data.Models;
using ATSPM.Domain.Extensions;
using ATSPM.Domain.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.WorkflowFilters
{
    /// <summary>
    /// Base class for filter controller event log data used in process workflows
    /// </summary>
    public abstract class FilterEventCodeLocationBase : ProcessStepBase<Tuple<Location, IEnumerable<ControllerEventLog>>, Tuple<Location, IEnumerable<ControllerEventLog>>>
    {
        /// <summary>
        /// List of filtered event codes
        /// </summary>
        protected List<int> filteredList = new();

        /// <inheritdoc/>
        public FilterEventCodeLocationBase(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            workflowProcess = new BroadcastBlock<Tuple<Location, IEnumerable<ControllerEventLog>>>(f =>
            {
               return Tuple.Create(f.Item1, f.Item2
                    .FromSpecification(new ControllerLogLocationFilterSpecification(f.Item1))
                    .Where(w => filteredList.Contains(w.EventCode)));
            }, options);
            workflowProcess.Completion.ContinueWith(t => Console.WriteLine($"!!!Task {options.NameFormat} is complete!!!"));
        }
    }
}
