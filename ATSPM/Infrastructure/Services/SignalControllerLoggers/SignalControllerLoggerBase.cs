using ATSPM.Application.Common.EqualityComparers;
using ATSPM.Application.Configuration;
using ATSPM.Application.Exceptions;
using ATSPM.Application.Repositories;
using ATSPM.Application.Services;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Data;
using ATSPM.Data.Models;
using ATSPM.Domain.BaseClasses;
using ATSPM.Domain.Common;
using ATSPM.Domain.Exceptions;
using EFCore.BulkExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Input;

namespace ATSPM.Infrasturcture.Services.SignalControllerLoggers
{
    public abstract class SignalControllerLoggerBase : ServiceObjectBase, ISignalControllerLoggerService
    {
        public event EventHandler CanExecuteChanged;

        private readonly ILogger _log;

        //TODO: pass in logger class instead
        public SignalControllerLoggerBase(ILogger log)
        {
            _log = log;
        }

        #region IExecuteWithProgress

        public abstract Task<bool> ExecuteAsync(IList<Signal> parameter, IProgress<int> progress = null, CancellationToken cancelToken = default);

        public virtual bool CanExecute(IList<Signal> parameter)
        {
            return parameter?.Count > 0;
        }

        public async Task<bool> ExecuteAsync(IList<Signal> parameter, CancellationToken cancelToken = default)
        {
            if (parameter is IList<Signal> p)
                return await ExecuteAsync(p, default, cancelToken);
            else
                return false;
        }

        public async Task ExecuteAsync(object parameter)
        {
            if (parameter is IList<Signal> p)
                await ExecuteAsync(p, default, default);
        }

        bool ICommand.CanExecute(object parameter)
        {
            if (parameter is IList<Signal> p)
                return CanExecute(p);
            return false;
        }

        void ICommand.Execute(object parameter)
        {
            if (parameter is IList<Signal> p)
                Task.Run(() => ExecuteAsync(p, default, default));
        }

        #endregion

        protected ITargetBlock<T> CreateActionStep<T>(Action<T> process, string processName, ExecutionDataflowBlockOptions options = default)
        {
            options.NameFormat = processName;
            options.SingleProducerConstrained = false;

            var block = new ActionBlock<T>(process, options);

            block.Completion.ContinueWith(t => _log.LogInformation(t.Exception, "{block} has completed: {status}", block.ToString(), t.Status), options.CancellationToken);

            return block;
        }

        protected ITargetBlock<T> CreateActionStep<T>(Func<T, Task> process, string processName, ExecutionDataflowBlockOptions options = default)
        {
            options.NameFormat = processName;
            options.SingleProducerConstrained = false;

            var block = new ActionBlock<T>(process, options);

            block.Completion.ContinueWith(t => _log.LogInformation(t.Exception, "{block} has completed: {status}", block.ToString(), t.Status), options.CancellationToken);

            return block;
        }

        protected IPropagatorBlock<T1, T2> CreateTransformManyStep<T1, T2>(Func<T1, Task<IEnumerable<T2>>> process, string processName, ExecutionDataflowBlockOptions options = default)
        {
            options.NameFormat = processName;

            var block = new TransformManyBlock<T1, T2>(process, options);

            block.Completion.ContinueWith(t => _log.LogInformation(t.Exception, "{block} has completed: {status}", block.ToString(), t.Status), options.CancellationToken);

            return block;
        }

        protected IPropagatorBlock<T1, T2> CreateTransformManyStep<T1, T2>(Func<T1, IEnumerable<T2>> process, string processName, ExecutionDataflowBlockOptions options = default)
        {
            options.NameFormat = processName;

            var block = new TransformManyBlock<T1, T2>(process, options);

            block.Completion.ContinueWith(t => _log.LogInformation(t.Exception, "{block} has completed: {status}", block.ToString(), t.Status), options.CancellationToken);

            return block;
        }
    }
}
