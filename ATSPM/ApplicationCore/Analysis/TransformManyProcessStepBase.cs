﻿using ATSPM.Data.Enums;
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
    /// Base class for ATSPM process steps using <see cref="TransformManyBlock{TInput, TOutput}"/>
    /// </summary>
    /// <typeparam name="T1">Input data type</typeparam>
    /// <typeparam name="T2">Output data type</typeparam>
    public abstract class TransformManyProcessStepBase<T1, T2> : ProcessStepBase<T1, T2>, IExecuteAsync<T1, IEnumerable<T2>>
    {
        public TransformManyProcessStepBase(ExecutionDataflowBlockOptions dataflowBlockOptions) : base(dataflowBlockOptions ?? new())
        {
            workflowProcess = new TransformManyBlock<T1, T2>(p => ExecuteAsync(p, options.CancellationToken), (ExecutionDataflowBlockOptions)options);
        }

        #region IExecuteAsyncWithProgress

        /// <inheritdoc/>
        public virtual bool CanExecute(T1 parameter)
        {
            return true;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T2>> ExecuteAsync(T1 parameter, CancellationToken cancelToken = default)
        {
            if (cancelToken.IsCancellationRequested)
                return await Task.FromCanceled<IEnumerable<T2>>(cancelToken);

            if (!CanExecute(parameter))
                return await Task.FromException<IEnumerable<T2>>(new ExecuteException());

            try
            {
                return await Process(parameter, cancelToken);
            }
            catch (Exception e)
            {
                return await Task.FromException<IEnumerable<T2>>(e);
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
        protected abstract Task<IEnumerable<T2>> Process(T1 input, CancellationToken cancelToken = default);
    }
}