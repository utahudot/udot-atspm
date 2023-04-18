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
    public abstract class FilterStepBase : IPropagatorBlock<IEnumerable<ControllerEventLog>, IEnumerable<ControllerEventLog>>,
        IDataflowBlock,
        ISourceBlock<IEnumerable<ControllerEventLog>>,
        ITargetBlock<IEnumerable<ControllerEventLog>>
    {
        private readonly IPropagatorBlock<IEnumerable<ControllerEventLog>, IEnumerable<ControllerEventLog>> _workflowProcess;

        protected List<int> filteredList = new();

        public FilterStepBase(DataflowBlockOptions dataflowBlockOptions = default)
        {
            dataflowBlockOptions ??= new();
            dataflowBlockOptions.NameFormat = this.GetType().Name;
            _workflowProcess = new BroadcastBlock<IEnumerable<ControllerEventLog>>(f => f.Where(l => filteredList.Contains(l.EventCode)),dataflowBlockOptions);
        }

        #region IPropagatorBlock

        #region IDataflowBlock

        /// <inheritdoc/>
        public Task Completion => _workflowProcess.Completion;

        /// <inheritdoc/>
        public void Complete()
        {
            _workflowProcess.Complete();
        }

        /// <inheritdoc/>
        public void Fault(Exception exception)
        {
            _workflowProcess.Fault(exception);
        }

        #endregion

        #region ISourceBlock

        /// <inheritdoc/>
        public IEnumerable<ControllerEventLog> ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<IEnumerable<ControllerEventLog>> target, out bool messageConsumed)
        {
            return _workflowProcess.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        /// <inheritdoc/>
        public IDisposable LinkTo(ITargetBlock<IEnumerable<ControllerEventLog>> target, DataflowLinkOptions linkOptions)
        {
            return _workflowProcess.LinkTo(target, linkOptions);
        }

        /// <inheritdoc/>
        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<IEnumerable<ControllerEventLog>> target)
        {
            _workflowProcess?.ReleaseReservation(messageHeader, target);
        }

        /// <inheritdoc/>
        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<IEnumerable<ControllerEventLog>> target)
        {
            return _workflowProcess.ReserveMessage(messageHeader, target);
        }

        #endregion

        #region ITargetBlock

        /// <inheritdoc/>
        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, IEnumerable<ControllerEventLog> messageValue, ISourceBlock<IEnumerable<ControllerEventLog>> source, bool consumeToAccept)
        {
            return _workflowProcess.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        #endregion

        #endregion
    }
}
