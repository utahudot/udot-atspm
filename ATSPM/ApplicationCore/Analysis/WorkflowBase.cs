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

namespace ATSPM.Application.Analysis
{
    public abstract class WorkflowBase<T1, T2> : ServiceObjectBase, IExecuteWithProgress<T1, IAsyncEnumerable<T2>, int>
    {
        public event EventHandler CanExecuteChanged;

        public List<IDataflowBlock> Steps { get; set; }

        public BroadcastBlock<T1> Input { get; set; }
        public BufferBlock<T2> Output { get; set; }

        #region IExecuteWithProgress

        public virtual async IAsyncEnumerable<T2> Execute(T1 parameter, IProgress<int> progress = null, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            if (!IsInitialized)
                BeginInit();

            if (CanExecute(parameter))
            {
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                //logMessages.LoggerStartedMessage(DateTime.Now, parameter.Count);

                //var signalSender = new BufferBlock<Signal>(new DataflowBlockOptions() { CancellationToken = cancelToken, NameFormat = "Signal Buffer" });

                //foreach (ITargetBlock<Signal> step in Steps.Where(f => f is ITargetBlock<Signal>))
                //{
                //    signalSender.LinkTo(step, new DataflowLinkOptions() { PropagateCompletion = true });
                //}

                //Steps.Add(signalSender);

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
                    //logMessages.LoggerExecutionException(new ControllerLoggerExecutionException(this, "Exception running Signal Controller Logger Service", e));
                }
                finally
                {
                    //logMessages.LoggerCompletedMessage(DateTime.Now, sw.Elapsed);
                    sw.Stop();
                }

                await foreach (var item in Output.ReceiveAllAsync(cancelToken))
                    yield return item;

                await Task.WhenAll(Steps.Select(s => s.Completion)).ContinueWith(t => Console.WriteLine($"All steps are complete! {t.Status}"), cancelToken);
            }
            else
            {
                throw new ExecuteException();
            }
        }

        public virtual bool CanExecute(T1 parameter)
        {
            return this.IsInitialized;
        }

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
