using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.BaseClasses;
using ATSPM.Domain.Common;
using ATSPM.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Input;

namespace ATSPM.Application.Analysis
{
    public abstract class ProcessBase<T1, T2> : ServiceObjectBase, IExecuteAsync<T1, T2>, IPropagatorBlock<T1, T2>, IDataflowBlock, ISourceBlock<T2>, ITargetBlock<T1>
    {
        public event EventHandler? CanExecuteChanged;

        private readonly IPropagatorBlock<T1, T2> _workflowProcess;

        public ProcessBase(ExecutionDataflowBlockOptions dataflowBlockOptions = default)
        {
            dataflowBlockOptions.NameFormat = this.GetType().Name;
            _workflowProcess = new TransformBlock<T1, T2>(p => ExecuteAsync(p, dataflowBlockOptions.CancellationToken), dataflowBlockOptions);
        }

        #region IPropagatorBlock

        #region IDataflowBlock

        public Task Completion => _workflowProcess.Completion;

        public void Complete()
        {
            _workflowProcess.Complete();
        }

        public void Fault(Exception exception)
        {
            _workflowProcess.Fault(exception);
        }

        #endregion

        #region ISourceBlock

        public T2? ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<T2> target, out bool messageConsumed)
        {
            return _workflowProcess.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public IDisposable LinkTo(ITargetBlock<T2> target, DataflowLinkOptions linkOptions)
        {
            return _workflowProcess.LinkTo(target, linkOptions);
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<T2> target)
        {
            _workflowProcess?.ReleaseReservation(messageHeader, target);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<T2> target)
        {
            return _workflowProcess.ReserveMessage(messageHeader, target);
        }

        #endregion

        #region ITargetBlock

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, T1 messageValue, ISourceBlock<T1>? source, bool consumeToAccept)
        {
            return _workflowProcess.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        #endregion

        #endregion

        #region IExecuteAsyncWithProgress

        public virtual bool CanExecute(T1 parameter)
        {
            return true;
        }

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

        Task? IExecuteAsync.ExecuteAsync(object parameter)
        {
            if (parameter is T1 p)
                return Task.Run(() => ExecuteAsync(p, default));
            return default;
        }

        bool ICommand.CanExecute(object? parameter)
        {
            if (parameter is T1 p)
                return CanExecute(p);
            return default;
        }

        void ICommand.Execute(object? parameter)
        {
            if (parameter is T1 p)
                Task.Run(() => ExecuteAsync(p, default));
        }

        #endregion
    }
}
