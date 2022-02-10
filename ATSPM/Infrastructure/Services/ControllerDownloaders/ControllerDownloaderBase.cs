using ATSPM.Application.Common;
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
using System.Linq;
using System.Runtime.CompilerServices;
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
        //protected readonly IOptions<SignalControllerDownloaderConfiguration> _options;
        protected readonly SignalControllerDownloaderConfiguration _options;

        #endregion

        public ControllerDownloaderBase(ILogger log, IServiceProvider serviceProvider, IOptionsSnapshot<SignalControllerDownloaderConfiguration> options)
        {
            _log = log;
            _serviceProvider = serviceProvider;
            _options = options.Get(this.GetType().Name) ?? options.Value;
        }

        #region Properties

        public abstract SignalControllerType ControllerType { get; }

        #endregion


        #region Methods

        protected abstract IAsyncEnumerable<FileInfo> ExecutionTask(Signal parameter, IProgress<ControllerDownloadProgress> progress = default, CancellationToken cancelToken = default);

        //public override void Initialize()
        //{
        //}

        #region IExecuteWithProgress

        public virtual bool CanExecute(Signal value)
        {
            //check valid controller type
            return ControllerType.HasFlag((SignalControllerType)(1 << value.ControllerType.ControllerTypeId));
        }

        public async IAsyncEnumerable<FileInfo> Execute(Signal parameter, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            await foreach (var item in Execute(parameter, default, cancelToken).WithCancellation(cancelToken))
            {
                yield return item;
            }
        }

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FormatException"></exception>
        public async IAsyncEnumerable<FileInfo> Execute(Signal parameter, IProgress<ControllerDownloadProgress> progress = null, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            if (CanExecute(parameter) && !cancelToken.IsCancellationRequested)
            {
                if (parameter == null)
                {
                    progress?.Report(new ControllerDownloadProgress(new ArgumentNullException(nameof(parameter), $"Signal parameter can not be null")));

                    throw new ArgumentNullException(nameof(parameter), $"Signal parameter can not be null");
                }
                    
                if (!parameter.Ipaddress.IsValidIPAddress(_options.PingControllerToVerify))
                {
                    progress?.Report(new ControllerDownloadProgress(new FormatException($"Not a Valid IP Address: {parameter.Ipaddress}")));

                    throw new FormatException($"Not a Valid IP Address: {parameter.Ipaddress}");
                }
                    
                await foreach (var item in ExecutionTask(parameter, progress, cancelToken).WithCancellation(cancelToken))
                {
                    yield return item;
                }
            }
            else
            {
                progress?.Report(new ControllerDownloadProgress(file: null));
            }
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
                Task.Run(() => Execute(p, default, default));
        }

        #endregion

        //public override void Dispose()
        //{
        //    //throw new NotImplementedException();
        //}

        #endregion

    }
}
