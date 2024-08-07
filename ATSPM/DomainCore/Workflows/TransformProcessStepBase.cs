﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for DomainCore - Utah.Udot.NetStandardToolkit.Workflows/TransformProcessStepBase.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Utah.Udot.NetStandardToolkit.Common;
using Utah.Udot.NetStandardToolkit.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Input;

namespace Utah.Udot.NetStandardToolkit.Workflows
{
    /// <summary>
    /// Base class for workflow process steps using <see cref="TransformBlock{TInput, TOutput}"/>
    /// </summary>
    /// <typeparam name="T1">Input data type</typeparam>
    /// <typeparam name="T2">Output data type</typeparam>
    public abstract class TransformProcessStepBase<T1, T2> : ProcessStepBase<T1, T2>, IExecuteAsync<T1, T2>
    {
        /// <inheritdoc/>
        public event EventHandler CanExecuteChanged;

        /// <inheritdoc/>
        public TransformProcessStepBase(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions ?? new())
        {
            workflowProcess = new TransformBlock<T1, T2>(p => ExecuteAsync(p, options.CancellationToken), (ExecutionDataflowBlockOptions)options);
            workflowProcess.Completion.ContinueWith(t => Console.WriteLine($"!!!Task {options.NameFormat} is complete!!! {t.Status}"));
        }

        #region IExecuteAsync

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
