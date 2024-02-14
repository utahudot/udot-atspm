using ATSPM.Domain.Common;
using ATSPM.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
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
        public virtual async IAsyncEnumerable<T2> Execute(T1 parameter, CancellationToken cancelToken = default)
        {
            if (!cancelToken.IsCancellationRequested)
            {
                IAsyncEnumerable<T2> result = default;

                try
                {
                    //if (!CanExecute(parameter))
                    //    throw new ExecuteException();

                    //return Process(parameter, cancelToken);

                    result = Process(parameter, cancelToken);

                }
                catch (Exception)
                {
                }

                await foreach (var r in result.WithCancellation(cancelToken))
                {
                    yield return r;
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
