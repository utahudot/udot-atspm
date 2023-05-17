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

namespace ATSPM.Application.Analysis.WorkflowSteps
{
    /// <summary>
    /// Base class for ATSPM process steps using <see cref="TransformBlock{TInput, TOutput}"/>
    /// </summary>
    /// <typeparam name="T1">Input data type</typeparam>
    /// <typeparam name="T2">Output data type</typeparam>
    public abstract class TransformProcessStepBase<T1, T2> : ProcessStepBase<T1, T2>, IExecuteAsync<T1, T2>
    {
        public event EventHandler CanExecuteChanged;

        public TransformProcessStepBase(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions ?? new())
        {
            workflowProcess = new TransformBlock<T1, T2>(p => ExecuteAsync(p, options.CancellationToken), (ExecutionDataflowBlockOptions)options);
            workflowProcess.Completion.ContinueWith(t => Console.WriteLine($"!!!Task {options.NameFormat} is complete!!! {t.Status}"));
        }

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
        bool ICommand.CanExecute(object parameter)
        {
            if (parameter is T1 p)
                return CanExecute(p);
            return default;
        }

        /// <inheritdoc/>
        void ICommand.Execute(object parameter)
        {
            if (parameter is T1 p)
                Task.Run(() => ExecuteAsync(p, default));
        }

        #endregion

        /// <summary>
        /// Process to perform when <see cref="ExecuteAsync(T1, CancellationToken)"/> is called
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        protected abstract Task<T2> Process(T1 input, CancellationToken cancelToken = default);
    }
}
