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
    public abstract class ProcessStepBase<T1, T2> : IExecuteAsync<T1, T2>, IPropagatorBlock<T1, T2>, IDataflowBlock, ISourceBlock<T2>, ITargetBlock<T1>
    {
        public event EventHandler CanExecuteChanged;

        private readonly IPropagatorBlock<T1, T2> _workflowProcess;

        public ProcessStepBase(ExecutionDataflowBlockOptions dataflowBlockOptions = default)
        {
            dataflowBlockOptions ??= new();
            dataflowBlockOptions.NameFormat = this.GetType().Name;
            _workflowProcess = new TransformBlock<T1, T2>(p => ExecuteAsync(p, dataflowBlockOptions.CancellationToken), dataflowBlockOptions);
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
        public T2? ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<T2> target, out bool messageConsumed)
        {
            return _workflowProcess.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        /// <inheritdoc/>
        public IDisposable LinkTo(ITargetBlock<T2> target, DataflowLinkOptions linkOptions)
        {
            return _workflowProcess.LinkTo(target, linkOptions);
        }

        /// <inheritdoc/>
        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<T2> target)
        {
            _workflowProcess?.ReleaseReservation(messageHeader, target);
        }

        /// <inheritdoc/>
        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<T2> target)
        {
            return _workflowProcess.ReserveMessage(messageHeader, target);
        }

        #endregion

        #region ITargetBlock

        /// <inheritdoc/>
        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, T1 messageValue, ISourceBlock<T1>? source, bool consumeToAccept)
        {
            return _workflowProcess.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        #endregion

        #endregion

        #region IExecuteAsyncWithProgress

        /// <inheritdoc/>
        public virtual bool CanExecute(T1 parameter)
        {
            return true;
        }

        /// <inheritdoc/>
        public virtual async Task<T2> ExecuteAsync(T1 parameter, CancellationToken cancelToken = default)
        {
            if (cancelToken.IsCancellationRequested)
                return await Task.FromCanceled<T2>(cancelToken);

            if (!CanExecute(parameter))
                return await Task.FromException<T2>(new ExecuteException());

            try
            {
                return await Process(parameter, cancelToken);
            }
            catch (Exception e)
            {
                return await Task.FromException<T2>(e);
            }
        }

        /// <inheritdoc/>
        Task IExecuteAsync.ExecuteAsync(object parameter)
        {
            if (parameter is T1 p)
                return Task.Run(() => ExecuteAsync(p, default));
            return default;
        }

        /// <inheritdoc/>
        bool ICommand.CanExecute(object? parameter)
        {
            if (parameter is T1 p)
                return CanExecute(p);
            return default;
        }

        /// <inheritdoc/>
        void ICommand.Execute(object? parameter)
        {
            if (parameter is T1 p)
                Task.Run(() => ExecuteAsync(p, default));
        }

        #endregion

        public abstract Task<T2> Process(T1 input, CancellationToken cancelToken = default);
    }
}
