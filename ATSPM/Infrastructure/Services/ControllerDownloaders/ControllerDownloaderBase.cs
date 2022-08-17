using ATSPM.Application.Common;
using ATSPM.Application.Configuration;
using ATSPM.Application.Exceptions;
using ATSPM.Application.LogMessages;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Data.Models;
using ATSPM.Domain.BaseClasses;
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
        protected ILogger _log;
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

        public virtual bool CanExecute(Signal value)
        {
            return value?.ControllerType?.Id == ControllerType && value.Enabled;
        }

        public async IAsyncEnumerable<FileInfo> Execute(Signal parameter, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            await foreach (var item in Execute(parameter, default, cancelToken).WithCancellation(cancelToken))
            {
                yield return item;
            }
        }

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidSignalControllerIPAddressException"></exception>
        /// <exception cref="ExecuteException"></exception>
        public async IAsyncEnumerable<FileInfo> Execute(Signal parameter, IProgress<ControllerDownloadProgress> progress = null, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter), $"Signal parameter can not be null");

            //if (CanExecute(parameter) && !cancelToken.IsCancellationRequested)
            if (CanExecute(parameter))
            {
                if (!parameter.Ipaddress.IsValidIPAddress(_options.PingControllerToVerify)) 
                    throw new InvalidSignalControllerIpAddressException(parameter);

                var logMessages = new ControllerLoggerDownloaderLogMessages(_log, parameter);
                using (_client)
                {
                    try
                    {
                        var credentials = new NetworkCredential(parameter.ControllerType?.UserName, parameter.ControllerType?.Password, parameter.Ipaddress.ToString());

                        logMessages.ConnectingToHostMessage(parameter.SignalId, parameter.Ipaddress);

                        await _client.ConnectAsync(credentials, _options.ConnectionTimeout, _options.ReadTimeout, cancelToken);
                    }
                    catch (ControllerConnectionException e)
                    {
                        logMessages.ConnectingToHosException(parameter.SignalId, parameter.Ipaddress, e);
                    }
                    catch (OperationCanceledException e)
                    {
                        logMessages.OperationCancelledException(parameter.SignalId, parameter.Ipaddress, e);
                    }

                    if (_client.IsConnected)
                    {
                        logMessages.ConnectedToHostMessage(parameter.SignalId, parameter.Ipaddress);

                        IEnumerable<string> remoteFiles = new List<string>();

                        try
                        {
                            logMessages.GettingDirectoryListMessage(parameter.SignalId, parameter.Ipaddress);

                            remoteFiles = await _client.ListDirectoryAsync(parameter.ControllerType?.Ftpdirectory, cancelToken, FileFilters);
                        }
                        catch (ControllerListDirectoryException e)
                        {
                            logMessages.DirectoryListingException(parameter.SignalId, parameter.Ipaddress, e);
                        }
                        catch (ControllerConnectionException e)
                        {
                            logMessages.NotConnectedToHostException(parameter.SignalId, parameter.Ipaddress, e);
                        }

                        int total = remoteFiles.Count();
                        int current = 0;

                        logMessages.DirectoryListingMessage(total, parameter.SignalId, parameter.Ipaddress);

                        foreach (var file in remoteFiles)
                        {
                            var localFilePath = Path.Combine(_options.LocalPath, parameter.SignalId, Path.GetFileName(file));
                            FileInfo downloadedFile = null;

                            try
                            {
                                logMessages.DownloadingFileMessage(file, parameter.SignalId, parameter.Ipaddress);

                                downloadedFile = await _client.DownloadFileAsync(localFilePath, file, cancelToken);
                                current++;
                            }
                            catch (ControllerDownloadFileException e)
                            {
                                logMessages.DownloadFileException(file, parameter.SignalId, parameter.Ipaddress, e);
                            }
                            catch (ControllerConnectionException e)
                            {
                                logMessages.NotConnectedToHostException(parameter.SignalId, parameter.Ipaddress, e);
                            }
                            catch (OperationCanceledException e)
                            {
                                logMessages.OperationCancelledException(parameter.SignalId, parameter.Ipaddress, e);
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
                            //        _log.LogWarning(new EventId(Convert.ToInt32(parameter.SignalId)), e, "Exception deleting file {file} from {ip}", file, parameter.Ipaddress);
                            //    }
                            //    catch (OperationCanceledException e)
                            //    {
                            //        _log.LogDebug(new EventId(Convert.ToInt32(parameter.SignalId)), e, "Operation canceled connecting to {ip}", parameter.Ipaddress);
                            //    }
                            //}

                            //HACK: don't know why files aren't downloading without throwing an error
                            if (downloadedFile != null)
                            {
                                logMessages.DownloadedFileMessage(file, parameter.SignalId, parameter.Ipaddress);

                                progress?.Report(new ControllerDownloadProgress(downloadedFile, current, total));

                                yield return downloadedFile;
                            }
                            else
                            {
                                _log.LogWarning(new EventId(Convert.ToInt32(parameter.SignalId)), "File failed to download on {signal} file name: {file}", parameter.SignalId, file);
                            }
                        }

                        logMessages.DownloadedFilesMessage(current, total, parameter.SignalId, parameter.Ipaddress);
                        try
                        {
                            logMessages.DisconnectingFromHostMessage(parameter.SignalId, parameter.Ipaddress);

                            await _client.DisconnectAsync(cancelToken);
                        }
                        catch (ControllerConnectionException e)
                        {
                            logMessages.DisconnectingFromHostException(parameter.SignalId, parameter.Ipaddress);
                        }
                        catch (OperationCanceledException e)
                        {
                            logMessages.OperationCancelledException(parameter.SignalId, parameter.Ipaddress, e);
                        }
                    }
                }
            }
            else
            {
                throw new ExecuteException();
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
