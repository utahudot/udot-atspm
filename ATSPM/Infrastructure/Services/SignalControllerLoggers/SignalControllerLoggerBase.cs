using ATSPM.Application.Exceptions;
using ATSPM.Application.LogMessages;
using ATSPM.Application.Services;
using ATSPM.Data.Models;
using ATSPM.Domain.BaseClasses;
using ATSPM.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Input;

namespace ATSPM.Infrastructure.Services.SignalControllerLoggers
{
    public abstract class SignalControllerLoggerBase : ServiceObjectBase, ISignalControllerLoggerService
    {
        public event EventHandler CanExecuteChanged;

        protected SignalControllerLoggerLogMessages logMessages;
        protected CancellationToken token;

        //TODO: pass in logger class instead
        public SignalControllerLoggerBase(ILogger log)
        {
            logMessages = new SignalControllerLoggerLogMessages(log);
        }

        public List<IDataflowBlock> Steps { get; set; } = new();

        #region IExecuteWithProgress

        public virtual async Task<bool> ExecuteAsync(IList<Location> parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            token = cancelToken;

            if (!IsInitialized)
                BeginInit();
            
            if (CanExecute(parameter))
            {
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                logMessages.LoggerStartedMessage(DateTime.Now, parameter.Count);

                var signalSender = new BufferBlock<Location>(new DataflowBlockOptions() { CancellationToken = cancelToken, NameFormat = "Signal Buffer" });
                
                foreach (ITargetBlock<Location> step in Steps.Where(f => f is ITargetBlock<Location>))
                {
                    signalSender.LinkTo(step, new DataflowLinkOptions() { PropagateCompletion = true });
                }

                Steps.Add(signalSender);

                try
                {
                    foreach (var signal in parameter)
                    {
                        await signalSender.SendAsync(signal);
                    }

                    signalSender.Complete();

                    await Task.WhenAll(Steps.Select(s => s.Completion));

                    return Steps.All(t => t.Completion.IsCompletedSuccessfully);
                }
                catch (Exception e)
                {
                    logMessages.LoggerExecutionException(new ControllerLoggerExecutionException(this, "Exception running Signal Controller Logger Service", e));
                }
                finally
                {
                    logMessages.LoggerCompletedMessage(DateTime.Now, sw.Elapsed);
                    sw.Stop();
                }

                return false;
            }
            else
            {
                throw new ExecuteException();
            }
        }

        public virtual bool CanExecute(IList<Location> parameter)
        {
            return this.IsInitialized && parameter?.Count > 0;
        }

        public async Task<bool> ExecuteAsync(IList<Location> parameter, CancellationToken cancelToken = default)
        {
            if (parameter is IList<Location> p)
                return await ExecuteAsync(p, default, cancelToken);
            else
                return false;
        }

        public async Task ExecuteAsync(object parameter)
        {
            if (parameter is IList<Location> p)
                await ExecuteAsync(p, default, default);
        }

        bool ICommand.CanExecute(object parameter)
        {
            if (parameter is IList<Location> p)
                return CanExecute(p);
            return false;
        }

        void ICommand.Execute(object parameter)
        {
            if (parameter is IList<Location> p)
                Task.Run(() => ExecuteAsync(p, default, default));
        }

        #endregion

        protected ITargetBlock<T> CreateActionStep<T>(Action<T> process, string processName, ExecutionDataflowBlockOptions options = default)
        {
            options.NameFormat = processName;
            options.SingleProducerConstrained = false;

            var block = new ActionBlock<T>(p =>
            {
                try
                {
                    process?.Invoke(p);
                }
                catch (Exception e)
                {
                    logMessages.StepExecutionException(processName, new ControllerLoggerStepExecutionException<T>(this, processName, p, null, e));
                }
            },options);

            block.Completion.ContinueWith(t => logMessages.StepCompletedMessage(block.ToString(), t.Status), options.CancellationToken);

            Steps.Add(block);

            return block;
        }

        protected ITargetBlock<T> CreateActionStep<T>(Func<T, Task> process, string processName, ExecutionDataflowBlockOptions options = default)
        {
            options.NameFormat = processName;
            options.SingleProducerConstrained = false;

            var block = new ActionBlock<T>(async p =>
            {
                try
                {
                    await process?.Invoke(p);
                }
                catch (Exception e)
                {
                    logMessages.StepExecutionException(processName, new ControllerLoggerStepExecutionException<T>(this, processName, p, null, e));
                }
            },options);

            block.Completion.ContinueWith(t => logMessages.StepCompletedMessage(block.ToString(), t.Status), options.CancellationToken);

            Steps.Add(block);

            return block;
        }

        protected IPropagatorBlock<T1, T2> CreateTransformManyStep<T1, T2>(Func<T1, IEnumerable<T2>> process, string processName, ExecutionDataflowBlockOptions options = default)
        {
            options.NameFormat = processName;

            var block = new TransformManyBlock<T1, T2>(p =>
            {
                try
                {
                    return process?.Invoke(p);
                }
                catch (Exception e)
                {
                    logMessages.StepExecutionException(processName, new ControllerLoggerStepExecutionException<T1>(this, processName, p, null, e));
                }

                return null;

            }, options);

            block.Completion.ContinueWith(t => logMessages.StepCompletedMessage(block.ToString(), t.Status), options.CancellationToken);

            Steps.Add(block);

            return block;
        }

        protected IPropagatorBlock<T1, T2> CreateTransformManyStep<T1, T2>(Func<T1, Task<IEnumerable<T2>>> process, string processName, ExecutionDataflowBlockOptions options = default)
        {
            options.NameFormat = processName;

            var block = new TransformManyBlock<T1, T2>(async p =>
            {
                try
                {
                    return await process?.Invoke(p);
                }
                catch (Exception e)
                {
                    logMessages.StepExecutionException(processName, new ControllerLoggerStepExecutionException<T1>(this, processName, p, null, e));
                }

                return null;

            }, options);

            block.Completion.ContinueWith(t => logMessages.StepCompletedMessage(block.ToString(), t.Status), options.CancellationToken);

            Steps.Add(block);

            return block;
        }
    }
}
