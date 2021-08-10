using ATSPM.Application.Configuration;
using ATSPM.Application.Enums;
using ATSPM.Application.Models;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Domain.Common;
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
using Utah.Gov.Udot.PipelineManager;

namespace ATSPM.Infrasturcture.Services.ControllerDownloaders
{
    public class SignalControllerDownloaderStubA : ISignalControllerDownloader
    {
        public event EventHandler CanExecuteChanged;

        private readonly ILogger _log;
        private readonly IServiceProvider _serviceProvider;
        protected readonly IOptions<SignalControllerDownloaderConfiguration> _options;


        public SignalControllerDownloaderStubA(ILogger<SignalControllerDownloaderStubA> log, IServiceProvider serviceProvider, IOptions<SignalControllerDownloaderConfiguration> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);


        public SignalControllerType ControllerType => SignalControllerType.EOS;

        public bool CanExecute(Signal value)
        {
            //_log.LogWarning($"{value.ControllerType.ControllerTypeId} - {Convert.ToInt32(ControllerType)} - {value.ControllerType.ControllerTypeId == Convert.ToInt32(ControllerType)}");
            return (value.ControllerType.ControllerTypeId == Convert.ToInt32(ControllerType));
        }

        public Task<DirectoryInfo> Execute(Signal input, CancellationToken cancelToken = default, IProgress<PipelineProgress> progress = null)
        {
            _log.LogWarning(new EventId(Convert.ToInt32(input.SignalId)), $"Controller Type: {input.ControllerType.Description} - {input.ControllerType.Ftpdirectory}");
            DirectoryInfo dir = null;
            return dir.AsTask();
        }

        public Task<DirectoryInfo> ExecuteAsync(Signal parameter, CancellationToken cancelToken = default)
        {
            return ExecuteAsync(parameter, cancelToken, default);
        }

        public Task<DirectoryInfo> ExecuteAsync(Signal parameter, CancellationToken cancelToken = default, IProgress<int> progress = default)
        {
            _log.LogWarning(new EventId(Convert.ToInt32(parameter.SignalId)), $"Controller Type: {parameter.ControllerType.Description} - {parameter.ControllerType.Ftpdirectory}");
            DirectoryInfo dir = null;
            return dir.AsTask();
        }

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
    }
}
