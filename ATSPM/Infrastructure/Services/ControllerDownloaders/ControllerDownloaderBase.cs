using ATSPM.Application.Common;
using ATSPM.Application.Configuration;
using ATSPM.Application.Exceptions;
using ATSPM.Application.LogMessages;
using ATSPM.Application.Services;
using ATSPM.Data.Models;
using ATSPM.Domain.BaseClasses;
using ATSPM.Domain.Exceptions;
using ATSPM.Domain.Extensions;
using Duende.IdentityServer.EntityFramework.Entities;
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
    public abstract class ControllerDownloaderBase : ServiceObjectBase, IDeviceDownloader
    {
        public event EventHandler CanExecuteChanged;

        #region Fields

        protected IDownloaderClient _client;
        protected ILogger _log;
        protected readonly SignalControllerDownloaderConfiguration _options;


        #endregion

        public ControllerDownloaderBase(IDownloaderClient client, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> options)
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

        public virtual bool CanExecute(Device value)
        {
            return value.LoggingEnabled;
        }

        public async IAsyncEnumerable<FileInfo> Execute(Device parameter, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            await foreach (var item in Execute(parameter, default, cancelToken).WithCancellation(cancelToken))
            {
                yield return item;
            }
        }

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidLocationControllerIPAddressException"></exception>
        /// <exception cref="ExecuteException"></exception>
        public async IAsyncEnumerable<FileInfo> Execute(Device parameter, IProgress<ControllerDownloadProgress> progress = null, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            var locationIdentifier = parameter?.Location?.LocationIdentifier;
            var user = parameter?.DeviceConfiguration?.UserName;
            var password = parameter?.DeviceConfiguration?.Password;
            var ipaddress = parameter?.Ipaddress;
            var directory = parameter?.DeviceConfiguration?.Directory;
            var searchTerms = parameter?.DeviceConfiguration?.SearchTerms;

            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter), $"Location parameter can not be null");

            //if (CanExecute(parameter) && !cancelToken.IsCancellationRequested)
            if (CanExecute(parameter))
            {
                if (!ipaddress.IsValidIPAddress(_options.PingControllerToVerify))
                    throw new InvalidSignalControllerIpAddressException(parameter);

                var logMessages = new ControllerLoggerDownloaderLogMessages(_log, parameter);

                using (_client)
                {
                    try
                    {
                        var credentials = new NetworkCredential(user, password, ipaddress.ToString());

                        logMessages.ConnectingToHostMessage(locationIdentifier, ipaddress);

                        await _client.ConnectAsync(credentials, _options.ConnectionTimeout, _options.ReadTimeout, cancelToken);
                    }
                    catch (ControllerConnectionException e)
                    {
                        logMessages.ConnectingToHostException(locationIdentifier, ipaddress, e);
                    }
                    catch (OperationCanceledException e)
                    {
                        logMessages.OperationCancelledException(locationIdentifier, ipaddress, e);
                    }

                    if (_client.IsConnected)
                    {
                        logMessages.ConnectedToHostMessage(locationIdentifier, ipaddress);

                        IEnumerable<string> remoteFiles = new List<string>();

                        try
                        {
                            logMessages.GettingDirectoryListMessage(locationIdentifier, ipaddress, directory);

                            remoteFiles = await _client.ListDirectoryAsync(directory, cancelToken, searchTerms);
                        }
                        catch (ControllerListDirectoryException e)
                        {
                            logMessages.DirectoryListingException(locationIdentifier, ipaddress, directory, e);
                        }
                        catch (ControllerConnectionException e)
                        {
                            logMessages.NotConnectedToHostException(locationIdentifier, ipaddress, e);
                        }

                        int total = remoteFiles.Count();
                        int current = 0;

                        logMessages.DirectoryListingMessage(total, locationIdentifier, ipaddress);

                        foreach (var file in remoteFiles)
                        {
                            var localFilePath = Path.Combine(_options.LocalPath, locationIdentifier, Path.GetFileName(file));
                            FileInfo downloadedFile = null;

                            try
                            {
                                logMessages.DownloadingFileMessage(file, locationIdentifier, ipaddress);

                                downloadedFile = await _client.DownloadFileAsync(localFilePath, file, cancelToken);
                                current++;
                            }
                            catch (ControllerDownloadFileException e)
                            {
                                logMessages.DownloadFileException(file, locationIdentifier, ipaddress, e);
                            }
                            catch (ControllerConnectionException e)
                            {
                                logMessages.NotConnectedToHostException(locationIdentifier, ipaddress, e);
                            }
                            catch (OperationCanceledException e)
                            {
                                logMessages.OperationCancelledException(locationIdentifier, ipaddress, e);
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
                            //        _log.LogWarning(new EventId(Convert.ToInt32(parameter.LocationId)), e, "Exception deleting file {file} from {ip}", file, ipaddress);
                            //    }
                            //    catch (OperationCanceledException e)
                            //    {
                            //        _log.LogDebug(new EventId(Convert.ToInt32(parameter.LocationId)), e, "Operation canceled connecting to {ip}", ipaddress);
                            //    }
                            //}

                            //HACK: don't know why files aren't downloading without throwing an error
                            if (downloadedFile != null)
                            {
                                logMessages.DownloadedFileMessage(file, locationIdentifier, ipaddress);

                                progress?.Report(new ControllerDownloadProgress(downloadedFile, current, total));

                                yield return downloadedFile;
                            }
                            else
                            {
                                _log.LogWarning(new EventId(Convert.ToInt32(locationIdentifier)), "File failed to download on {Location} file name: {file}", locationIdentifier, file);
                            }
                        }

                        logMessages.DownloadedFilesMessage(current, total, locationIdentifier, ipaddress);
                        try
                        {
                            logMessages.DisconnectingFromHostMessage(locationIdentifier, ipaddress);

                            await _client.DisconnectAsync(cancelToken);
                        }
                        catch (ControllerConnectionException e)
                        {
                            logMessages.DisconnectingFromHostException(locationIdentifier, ipaddress);
                        }
                        catch (OperationCanceledException e)
                        {
                            logMessages.OperationCancelledException(locationIdentifier, ipaddress, e);
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
            if (parameter is Device p)
                return CanExecute(p);
            return default;
        }

        void ICommand.Execute(object parameter)
        {
            if (parameter is Device p)
                Task.Run(() => Execute(p, default, default));
        }

        protected override void DisposeManagedCode()
        {
            _client?.Dispose();
        }

        #endregion
    }
}
