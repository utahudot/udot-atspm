#region license
// Copyright 2024 Utah Departement of Transportation
// for DomainCore - ATSPM.Domain.Workflows/TransformManyProcessStepBaseAsync.cs
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
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Input;

namespace ATSPM.Domain.Workflows
{
    /// <summary>
    /// Base class for workflow process steps using <see cref="TransformManyBlock{TInput, TOutput}"/>
    /// </summary>
    /// <typeparam name="T1">Input data type</typeparam>
    /// <typeparam name="T2">Output data type</typeparam>
    public abstract class TransformManyProcessStepBaseAsync<T1, T2> : ProcessStepBase<T1, T2>, IExecute<T1, IAsyncEnumerable<T2>>
    {
        /// <inheritdoc/>
        public TransformManyProcessStepBaseAsync(ExecutionDataflowBlockOptions dataflowBlockOptions) : base(dataflowBlockOptions ?? new())
        {
            workflowProcess = new TransformManyBlock<T1, T2>(p => Execute(p, options.CancellationToken), (ExecutionDataflowBlockOptions)options);

            workflowProcess.Completion.ContinueWith(t => Console.WriteLine($"!!!Task {options.NameFormat} is complete!!! {t.Status}"));
        }

        #region IExecuteAsync

        /// <inheritdoc/>
        public event EventHandler CanExecuteChanged;

        /// <inheritdoc/>
        public virtual bool CanExecute(T1 parameter)
        {
            return true;
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<T2> Execute(T1 parameter, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            await using (IAsyncEnumerator<T2> process = Process(parameter, cancelToken).GetAsyncEnumerator(cancelToken))
            {
                bool active = true;

                while (active)
                {
                    try
                    {
                        active = await process.MoveNextAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                    if (active)
                        yield return process.Current;
                }
            }
        }

        /// <inheritdoc/>
        bool ICommand.CanExecute(object parameter)
        {
            if (parameter is T1 p)
                return CanExecute(p);
            return false;
        }

        /// <inheritdoc/>
        void ICommand.Execute(object parameter)
        {
            if (parameter is T1 p)
                Task.Run(() => Execute(p, default));
        }

        #endregion

        /// <summary>
        /// Process to perform when <see cref="Execute(T1, CancellationToken)"/> is called
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        protected abstract IAsyncEnumerable<T2> Process(T1 input, CancellationToken cancelToken = default);
    }
}
