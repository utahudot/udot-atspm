using ATSPM.Application.Common;
using ATSPM.Application.Configuration;
using ATSPM.Application.Enums;
using ATSPM.Application.Exceptions;
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
using System.Net;
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

        protected IDownloaderClient _client;
        protected readonly ILogger _log;
        //protected readonly IOptions<SignalControllerDownloaderConfiguration> _options;
        protected readonly SignalControllerDownloaderConfiguration _options;
        

        #endregion

        public ControllerDownloaderBase(IDownloaderClient client, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> options)
        {
            _client = client;
            _log = log;
            _options = options?.Get(this.GetType().Name) ?? options?.Value;
        }

        #region Properties
        
        public abstract int ControllerType { get; }

        public abstract string[] FileFilters { get; set; }

        #endregion

        #region Methods

        //public override void Initialize()
        //{
        //}

        #region IExecuteWithProgress

        public virtual bool CanExecute(Signal value)
        {
            //check valid controller type
            //return ControllerType.HasFlag((SignalControllerType)(1 << value?.ControllerType?.ControllerTypeId));
            return value?.ControllerType?.ControllerTypeId == ControllerType && value.Enabled;
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
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter), $"Signal parameter can not be null");

            if (CanExecute(parameter) && !cancelToken.IsCancellationRequested)
            {
                if (!parameter.Ipaddress.IsValidIPAddress(_options.PingControllerToVerify)) 
                    throw new FormatException($"Not a Valid IP Address: {parameter.Ipaddress}");

                try
                {
                    var credentials = new NetworkCredential(parameter.ControllerType?.UserName, parameter.ControllerType?.Password, parameter.Ipaddress);

                    await _client.ConnectAsync(credentials, _options.ConnectionTimeout, _options.ReadTimeout, cancelToken);
                }
                catch (ControllerConnectionException e)
                {
                    _log.LogDebug(new EventId(Convert.ToInt32(parameter.SignalId)), e, "Exception connecting to {ip}", parameter.Ipaddress);
                }
                catch (OperationCanceledException e)
                {
                    _log.LogDebug(new EventId(Convert.ToInt32(parameter.SignalId)), e, "Operation canceled connecting to {ip}", parameter.Ipaddress);
                }

                if (_client.IsConnected)
                {
                    var remoteFiles = await _client.ListDirectoryAsync(parameter.ControllerType?.Ftpdirectory, cancelToken, FileFilters);

                    int total = remoteFiles.Count();
                    int current = 0;

                    foreach (var file in remoteFiles)
                    {
                        var localFilePath = Path.Combine(_options.LocalPath,parameter.SignalId, Path.GetFileName(file));
                        FileInfo downloadedFile = null;

                        try
                        {
                            downloadedFile = await _client.DownloadFileAsync(localFilePath, file, cancelToken);
                            current++;
                        }
                        catch (ControllerDownloadFileException e)
                        {
                            _log.LogDebug(new EventId(Convert.ToInt32(parameter.SignalId)), e, "Exception downloading file {file} from {ip}", file, parameter.Ipaddress);
                        }
                        catch (OperationCanceledException e)
                        {
                            _log.LogDebug(new EventId(Convert.ToInt32(parameter.SignalId)), e, "Operation canceled connecting to {ip}", parameter.Ipaddress);
                        }

                        // TODO: delete file here
                        //if (_options.DeleteAfterDownload)
                        //{
                        //    try
                        //    {
                        //        await client.DeleteFileAsync(file, cancelToken);
                        //    }
                        //    catch (ControllerDownloadFileException e)
                        //    {
                        //        _log.LogDebug(new EventId(Convert.ToInt32(parameter.SignalId)), e, "Exception deleting file {file} from {ip}", file, parameter.Ipaddress);
                        //    }
                        //    catch (OperationCanceledException e)
                        //    {
                        //        _log.LogDebug(new EventId(Convert.ToInt32(parameter.SignalId)), e, "Operation canceled connecting to {ip}", parameter.Ipaddress);
                        //    }
                        //}

                        progress?.Report(new ControllerDownloadProgress(downloadedFile, current, total));

                        yield return downloadedFile;
                    }

                    progress?.Report(new ControllerDownloadProgress(new FileInfo(Path.Combine(_options.LocalPath, parameter.SignalId)), current, total));

                    try
                    {
                        await _client.DisconnectAsync(cancelToken);
                    }
                    catch (ControllerConnectionException e)
                    {
                        _log.LogDebug(new EventId(Convert.ToInt32(parameter.SignalId)), e, "Exception diconnecting from {ip}", parameter.Ipaddress);
                    }
                    catch (OperationCanceledException e)
                    {
                        _log.LogDebug(new EventId(Convert.ToInt32(parameter.SignalId)), e, "Operation canceled connecting to {ip}", parameter.Ipaddress);
                    }
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_client != null)
                {
                    if (_client.IsConnected)
                    {
                        _client.DisconnectAsync();
                    }
                    _client.Dispose();
                    _client = null;
                }
            }

            base.Dispose(disposing);
        }

        #endregion

    }
}
