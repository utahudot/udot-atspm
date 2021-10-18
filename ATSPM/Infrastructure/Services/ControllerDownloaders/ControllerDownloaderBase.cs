using ATSPM.Application.Configuration;
using ATSPM.Application.Enums;
using ATSPM.Application.Extensions;
using ATSPM.Application.Models;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Domain.BaseClasses;
using ATSPM.Domain.Common;
using ATSPM.Domain.Exceptions;
using ATSPM.Domain.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ATSPM.Infrasturcture.Services.ControllerDownloaders
{
    public abstract class ControllerDownloaderBase : ServiceObjectBase, ISignalControllerDownloader
    {
        public event EventHandler CanExecuteChanged;

        #region Fields

        protected readonly ILogger _log;
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IOptions<SignalControllerDownloaderConfiguration> _options;

        #endregion

        public ControllerDownloaderBase(ILogger log, IServiceProvider serviceProvider, IOptions<SignalControllerDownloaderConfiguration> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);

        #region Properties
        public abstract SignalControllerType ControllerType { get; }

        #endregion


        #region Methods

        protected abstract Task<DirectoryInfo> ExecutionTask(Signal parameter, CancellationToken cancelToken = default, IProgress<int> progress = default);

        //public override void Initialize()
        //{
        //}

        #region IPipelineExecute<Tin, Tout>

        public virtual bool CanExecute(Signal value)
        {
            //check valid controller type
            return ControllerType.HasFlag((SignalControllerType)(1 << value.ControllerType.ControllerTypeId));
        }

        public async Task<DirectoryInfo> ExecuteAsync(Signal parameter, CancellationToken cancelToken = default)
        {
            return await ExecuteAsync(parameter, cancelToken, default);
        }

        public async Task<DirectoryInfo> ExecuteAsync(Signal parameter, CancellationToken cancelToken = default, IProgress<int> progress = default)
        {
            //TODO: write out detailed logs

            cancelToken.ThrowIfCancellationRequested();
            
            if (!parameter.Ipaddress.IsValidIPAddress(_options.Value.PingControllerToVerify))
            //if (!parameter.Ipaddress.IsValidIPAddress(false))
                return await Task.FromException<DirectoryInfo>(new FormatException($"Not a Valid IP Address: {parameter.Ipaddress}"));

            //return directory
            DirectoryInfo dir = null;

            if (CanExecute(parameter))
            {
                try
                {
                    dir = await ExecutionTask(parameter, cancelToken, progress);
                }
                catch (TaskCanceledException e)
                {
                    e.LogE();
                    return await Task.FromCanceled<DirectoryInfo>(cancelToken);
                }
                catch (Exception e)
                {
                    e.LogE();
                    return await Task.FromException<DirectoryInfo>(e);
                }
            }
            else
            {
                dir ??= new DirectoryInfo(Path.Combine(_options.Value.LocalPath, "DidNotPassCanExecute", $"{parameter.SignalId} - {parameter.ControllerTypeId}"));

                return await Task.FromException<DirectoryInfo>(new ExecuteException());
            }

            if (dir != null && !dir.Exists)
                dir.Create();

            return dir;
        }

        #endregion

        Task IExecuteAsync.ExecuteAsync(object parameter)
        {
            if (parameter is Signal p)
                return Task.Run(() => ExecuteAsync(p, default, default));
            return default;
        }

        bool ICommand.CanExecute(object parameter)
        {
            if (parameter is Signal p)
                return CanExecute(p);
            return default;
        }

        void ICommand.Execute(object parameter)
        {
            if (parameter is Signal p)
                Task.Run(() => ExecuteAsync(p, default, default));
        }

        //public override void Dispose()
        //{
        //    //throw new NotImplementedException();
        //}

        #endregion

    }
}
