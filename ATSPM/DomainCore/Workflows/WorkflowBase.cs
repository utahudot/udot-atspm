using ATSPM.Domain.BaseClasses;
using ATSPM.Domain.Common;
using ATSPM.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Input;

namespace ATSPM.Domain.Workflows
{
    /// <summary>
    /// Used as a base to create complex, inter-linkable, parallel workflows
    /// </summary>
    /// <typeparam name="T1">Input data type</typeparam>
    /// <typeparam name="T2">Output data type</typeparam>
    public abstract class WorkflowBase<T1, T2> : ServiceObjectBase, IExecuteWithProgress<T1, IAsyncEnumerable<T2>, int>
    {
        /// <inheritdoc/>
        public event EventHandler CanExecuteChanged;
        protected DataflowBlockOptions blockOptions;

        /// <inheritdoc/>
        public WorkflowBase(DataflowBlockOptions dataflowBlockOptions = default)
        {
            blockOptions = dataflowBlockOptions ?? new();
            blockOptions.NameFormat = GetType().Name;
        }

        /// <summary>
        /// Used for tracking workflow step task completion results
        /// </summary>
        public List<IDataflowBlock> Steps { get; set; }

        /// <summary>
        /// Can be used to post or send data or link from other workflows
        /// </summary>
        public BroadcastBlock<T1> Input { get; set; }

        /// <summary>
        /// Can be used to recieve data or link to other workflows
        /// </summary>
        public BufferBlock<T2> Output { get; set; }

        /// <inheritdoc/>
        public override Task Initialize()
        {
            Steps = new();

            Input = new(null, blockOptions);
            Output = new(blockOptions);

            InstantiateSteps();

            Steps.Add(Input);

            AddStepsToTracker();

            LinkSteps();

            //base.Initialize();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Instantiate workflow steps objects
        /// </summary>
        protected abstract void InstantiateSteps();

        /// <summary>
        /// Add steps to <see cref="Steps"/> for step task tracking
        /// </summary>
        protected abstract void AddStepsToTracker();

        /// <summary>
        /// Link workflow steps
        /// </summary>
        protected abstract void LinkSteps();

        #region IExecuteWithProgress

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<T2> Execute(T1 parameter, IProgress<int> progress = null, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            if (!IsInitialized)
                BeginInit();

            if (CanExecute(parameter))
            {
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                //logMessages.LoggerStartedMessage(DateTime.Now, parameter.Count);

                //var LocationSender = new BufferBlock<Location>(new DataflowBlockOptions() { CancellationToken = cancelToken, NameFormat = "Location Buffer" });

                //foreach (ITargetBlock<Location> step in Steps.Where(f => f is ITargetBlock<Location>))
                //{
                //    LocationSender.LinkTo(step, new DataflowLinkOptions() { PropagateCompletion = true });
                //}

                //Steps.Add(LocationSender);

                try
                {
                    //foreach (T1 item in parameter)
                    //{
                    await Input.SendAsync(parameter);
                    //}

                    Input.Complete();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Output.Completion.ContinueWith(t =>
                    {
                        Console.WriteLine($"Output is complete! {t.Status}");
                        this.IsInitialized = false;
                    }, cancelToken);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed




                    //return Steps.All(t => t.Completion.IsCompletedSuccessfully);
                }
                catch (Exception e)
                {
                    //logMessages.LoggerExecutionException(new ControllerLoggerExecutionException(this, "Exception running Location Controller Logger Service", e));
                }
                finally
                {
                    //logMessages.LoggerCompletedMessage(DateTime.Now, sw.Elapsed);
                    sw.Stop();
                }

                await foreach (var item in Output.ReceiveAllAsync(cancelToken))
                    yield return item;

                await Task.WhenAll(Steps.Select(s => s.Completion)).ContinueWith(t => Console.WriteLine($"All steps are complete! {t.Status} in {sw.Elapsed}"), cancelToken);
            }
            else
            {
                throw new ExecuteException();
            }
        }

        /// <inheritdoc/>
        public virtual bool CanExecute(T1 parameter)
        {
            return this.IsInitialized;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<T2> Execute(T1 parameter, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            await foreach (var item in Execute(parameter, default, cancelToken).WithCancellation(cancelToken))
            {
                yield return item;
            }
        }

        bool ICommand.CanExecute(object parameter)
        {
            if (parameter is T1 p)
                return CanExecute(p);
            return false;
        }

        void ICommand.Execute(object parameter)
        {
            if (parameter is T1 p)
                Task.Run(() => Execute(p, default, default));
        }

        #endregion
    }
}
