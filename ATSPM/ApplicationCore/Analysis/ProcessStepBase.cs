using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using ATSPM.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Input;

namespace ATSPM.Application.Analysis
{
    /// <summary>
    /// Base class for ATSPM process steps
    /// </summary>
    /// <typeparam name="T1">Input data type</typeparam>
    /// <typeparam name="T2">Output data type</typeparam>
    public abstract class ProcessStepBase<T1, T2> : IPropagatorBlock<T1, T2>, IDataflowBlock, ISourceBlock<T2>, ITargetBlock<T1>
    {
        public event EventHandler CanExecuteChanged;

        protected IPropagatorBlock<T1, T2> workflowProcess;
        protected DataflowBlockOptions options;

        public ProcessStepBase(DataflowBlockOptions dataflowBlockOptions = default)
        {
            options = dataflowBlockOptions ?? new();
            options.NameFormat = this.GetType().Name;
        }

        #region IPropagatorBlock

        #region IDataflowBlock

        /// <inheritdoc/>
        public Task Completion => workflowProcess.Completion;

        /// <inheritdoc/>
        public void Complete()
        {
            workflowProcess.Complete();
        }

        /// <inheritdoc/>
        public void Fault(Exception exception)
        {
            workflowProcess.Fault(exception);
        }

        #endregion

        #region ISourceBlock

        /// <inheritdoc/>
        public T2 ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<T2> target, out bool messageConsumed)
        {
            return workflowProcess.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        /// <inheritdoc/>
        public IDisposable LinkTo(ITargetBlock<T2> target, DataflowLinkOptions linkOptions)
        {
            return workflowProcess.LinkTo(target, linkOptions);
        }

        /// <inheritdoc/>
        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<T2> target)
        {
            workflowProcess?.ReleaseReservation(messageHeader, target);
        }

        /// <inheritdoc/>
        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<T2> target)
        {
            return workflowProcess.ReserveMessage(messageHeader, target);
        }

        #endregion

        #region ITargetBlock

        /// <inheritdoc/>
        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, T1 messageValue, ISourceBlock<T1>? source, bool consumeToAccept)
        {
            return workflowProcess.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        #endregion

        #endregion
    }
}
