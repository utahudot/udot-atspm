using ATSPM.Application.Common;
using ATSPM.Application.Configuration;
using ATSPM.Application.Exceptions;
using ATSPM.Application.LogMessages;
using ATSPM.Application.Services.LocationControllerProtocols;
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

namespace ATSPM.Infrastructure.Services.ControllerDownloaders
{
    public abstract class ControllerDownloaderBase : ServiceObjectBase, ILocationControllerDownloader
    {
        public event EventHandler CanExecuteChanged;

        #region Fields

        protected IDownloaderClient _client;
        protected ILogger _log;
        //protected readonly IOptions<LocationControllerDownloaderConfiguration> _options;
        protected readonly LocationControllerDownloaderConfiguration _options;


        #endregion

        public ControllerDownloaderBase(IDownloaderClient client, ILogger log, IOptionsSnapshot<LocationControllerDownloaderConfiguration> options)
        {
            _client = client;
            _log = log;
            _options = options?.Get(GetType().Name) ?? options?.Value;
        }

        #region Properties

        public abstract int ControllerType { get; }

        public abstract string[] FileFilters { get; set; }

        #endregion

        #region Methods
        //public override void Initialize()
        //{
        //}

        public virtual bool CanExecute(Location value)
        {
            return value?.ControllerTypeId == ControllerType && value.ChartEnabled;
        }

        public async IAsyncEnumerable<FileInfo> Execute(Location parameter, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            await foreach (var item in Execute(parameter, default, cancelToken).WithCancellation(cancelToken))
            {
                yield return item;
            }
        }

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidLocationControllerIPAddressException"></exception>
        /// <exception cref="ExecuteException"></exception>
        public async IAsyncEnumerable<FileInfo> Execute(Location parameter, IProgress<ControllerDownloadProgress> progress = null, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter), $"Location parameter can not be null");

            //if (CanExecute(parameter) && !cancelToken.IsCancellationRequested)
            if (CanExecute(parameter))
            {
                if (!parameter.Ipaddress.IsValidIPAddress(_options.PingControllerToVerify))
                    throw new InvalidLocationControllerIpAddressException(parameter);

                var logMessages = new ControllerLoggerDownloaderLogMessages(_log, parameter);
                using (_client)
                {
                    try
                    {
                        var credentials = new NetworkCredential(parameter.ControllerType?.UserName, parameter.ControllerType?.Password, parameter.Ipaddress.ToString());

                        logMessages.ConnectingToHostMessage(parameter.LocationIdentifier, parameter.Ipaddress);

                        await _client.ConnectAsync(credentials, _options.ConnectionTimeout, _options.ReadTimeout, cancelToken);
                    }
                    catch (ControllerConnectionException e)
                    {
                        logMessages.ConnectingToHostException(parameter.LocationIdentifier, parameter.Ipaddress, e);
                    }
                    catch (OperationCanceledException e)
                    {
                        logMessages.OperationCancelledException(parameter.LocationIdentifier, parameter.Ipaddress, e);
                    }

                    if (_client.IsConnected)
                    {
                        logMessages.ConnectedToHostMessage(parameter.LocationIdentifier, parameter.Ipaddress);

                        IEnumerable<string> remoteFiles = new List<string>();

                        try
                        {
                            logMessages.GettingDirectoryListMessage(parameter.LocationIdentifier, parameter.Ipaddress, parameter.ControllerType?.Directory);

                            remoteFiles = await _client.ListDirectoryAsync(parameter.ControllerType?.Directory, cancelToken, FileFilters);
                        }
                        catch (ControllerListDirectoryException e)
                        {
                            logMessages.DirectoryListingException(parameter.LocationIdentifier, parameter.Ipaddress, parameter.ControllerType?.Directory, e);
                        }
                        catch (ControllerConnectionException e)
                        {
                            logMessages.NotConnectedToHostException(parameter.LocationIdentifier, parameter.Ipaddress, e);
                        }

                        int total = remoteFiles.Count();
                        int current = 0;

                        logMessages.DirectoryListingMessage(total, parameter.LocationIdentifier, parameter.Ipaddress);

                        foreach (var file in remoteFiles)
                        {
                            var localFilePath = Path.Combine(_options.LocalPath, parameter.LocationIdentifier, Path.GetFileName(file));
                            FileInfo downloadedFile = null;

                            try
                            {
                                logMessages.DownloadingFileMessage(file, parameter.LocationIdentifier, parameter.Ipaddress);

                                downloadedFile = await _client.DownloadFileAsync(localFilePath, file, cancelToken);
                                current++;
                            }
                            catch (ControllerDownloadFileException e)
                            {
                                logMessages.DownloadFileException(file, parameter.LocationIdentifier, parameter.Ipaddress, e);
                            }
                            catch (ControllerConnectionException e)
                            {
                                logMessages.NotConnectedToHostException(parameter.LocationIdentifier, parameter.Ipaddress, e);
                            }
                            catch (OperationCanceledException e)
                            {
                                logMessages.OperationCancelledException(parameter.LocationIdentifier, parameter.Ipaddress, e);
                            }

                            // TODO: delete file here
                            //if (_options.DeleteFile)
                            //{
                            //    try
                            //    {
                            //        await client.DeleteFileAsync(file, cancelToken);
                            //    }
                            //    catch (ControllerDownloadFileException e)
                            //    {
                            //        _log.LogWarning(new EventId(Convert.ToInt32(parameter.LocationId)), e, "Exception deleting file {file} from {ip}", file, parameter.Ipaddress);
                            //    }
                            //    catch (OperationCanceledException e)
                            //    {
                            //        _log.LogDebug(new EventId(Convert.ToInt32(parameter.LocationId)), e, "Operation canceled connecting to {ip}", parameter.Ipaddress);
                            //    }
                            //}

                            //HACK: don't know why files aren't downloading without throwing an error
                            if (downloadedFile != null)
                            {
                                logMessages.DownloadedFileMessage(file, parameter.LocationIdentifier, parameter.Ipaddress);

                                progress?.Report(new ControllerDownloadProgress(downloadedFile, current, total));

                                yield return downloadedFile;
                            }
                            else
                            {
                                _log.LogWarning(new EventId(Convert.ToInt32(parameter.LocationIdentifier)), "File failed to download on {Location} file name: {file}", parameter.LocationIdentifier, file);
                            }
                        }

                        logMessages.DownloadedFilesMessage(current, total, parameter.LocationIdentifier, parameter.Ipaddress);
                        try
                        {
                            logMessages.DisconnectingFromHostMessage(parameter.LocationIdentifier, parameter.Ipaddress);

                            await _client.DisconnectAsync(cancelToken);
                        }
                        catch (ControllerConnectionException e)
                        {
                            logMessages.DisconnectingFromHostException(parameter.LocationIdentifier, parameter.Ipaddress);
                        }
                        catch (OperationCanceledException e)
                        {
                            logMessages.OperationCancelledException(parameter.LocationIdentifier, parameter.Ipaddress, e);
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
            if (parameter is Location p)
                return CanExecute(p);
            return default;
        }

        void ICommand.Execute(object parameter)
        {
            if (parameter is Location p)
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
