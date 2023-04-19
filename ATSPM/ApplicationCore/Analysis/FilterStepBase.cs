using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis
{
    /// <summary>
    /// Base class for filter controller event log data used in process workflows
    /// </summary>
    public abstract class FilterStepBase : ProcessStepBase<IEnumerable<ControllerEventLog>, IEnumerable<ControllerEventLog>>
    {
        /// <summary>
        /// List of filtered event codes
        /// </summary>
        protected List<int> filteredList = new();

        public FilterStepBase(DataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            workflowProcess = new BroadcastBlock<IEnumerable<ControllerEventLog>>(f => f.Where(l => filteredList.Contains(l.EventCode)), options);
        }
    }
}
