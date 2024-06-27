#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Services.ControllerDownloaders/DeviceDownloaderBase.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using ATSPM.Application.Common;
using ATSPM.Application.Configuration;
using ATSPM.Application.Exceptions;
using ATSPM.Application.LogMessages;
using ATSPM.Application.Services;
using ATSPM.Data.Enums;
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

namespace ATSPM.Infrastructure.Services.ControllerDownloaders
{
    ///<inheritdoc cref="IDeviceDownloader"/>
    public abstract class DeviceDownloaderBase : ExecutableServiceWithProgressAsyncBase<Device, Tuple<Device, FileInfo>, ControllerDownloadProgress>, IDeviceDownloader
    {
        #region Fields

        private readonly IDownloaderClient _client;
        private readonly ILogger _log;
        private readonly SignalControllerDownloaderConfiguration _options;

        #endregion

        ///<inheritdoc/>
        public DeviceDownloaderBase(IDownloaderClient client, ILogger log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> options) : base(true)
        {
            _client = client;
            _log = log;
            _options = options?.Get(GetType().Name) ?? options?.Value;
        }

        #region Properties

        ///<inheritdoc/>
        public virtual TransportProtocols Protocol => TransportProtocols.Unknown;

        #endregion

        #region Methods

        //public override void Initialize()
        //{
        //}

        /// <summary>
        /// Generates a pattern for folder structure used for temp event log storage
        /// </summary>
        /// <param name="value"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public virtual string GenerateLocalFilePath(Device value, string file)
        {
            var result = Path.Combine
                (_options.LocalPath,
                $"{value.Location?.LocationIdentifier} - {value.Location?.PrimaryName}",
                value.DeviceType.ToString(),
                value.Ipaddress.ToString(),
                Path.GetFileName(file));

            return result;
        }

        ///<inheritdoc/>
        public override bool CanExecute(Device value)
        {
            return value.DeviceConfiguration.Protocol == Protocol && value.LoggingEnabled;
        }

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidDeviceIpAddressException"></exception>
        /// <exception cref="ExecuteException"></exception>
        public override async IAsyncEnumerable<Tuple<Device, FileInfo>> Execute(Device parameter, IProgress<ControllerDownloadProgress> progress = null, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter), $"Parameter can not be null");

            //if (CanExecute(parameter) && !cancelToken.IsCancellationRequested)
            if (CanExecute(parameter))
            {
                if (!parameter.Ipaddress.IsValidIPAddress(_options.PingControllerToVerify))
                    throw new InvalidDeviceIpAddressException(parameter);

                var locationIdentifier = parameter?.Location?.LocationIdentifier;
                var user = parameter?.DeviceConfiguration?.UserName;
                var password = parameter?.DeviceConfiguration?.Password;
                var ipaddress = IPAddress.Parse(parameter?.Ipaddress);
                var directory = parameter?.DeviceConfiguration?.Directory;
                var searchTerms = parameter?.DeviceConfiguration?.SearchTerms;

                var logMessages = new ControllerLoggerDownloaderLogMessages(_log, parameter);

                using (_client)
                {
                    try
                    {
                        var connection = new IPEndPoint(ipaddress, parameter.DeviceConfiguration.Port);
                        var credentials = new NetworkCredential(user, password, ipaddress.ToString());
                        
                        logMessages.ConnectingToHostMessage(locationIdentifier, ipaddress);

                        await _client.ConnectAsync(connection, credentials, _options.ConnectionTimeout, _options.ReadTimeout, cancelToken);
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
                            var localFilePath = GenerateLocalFilePath(parameter, file);
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

                            if (_options.DeleteFile)
                            {
                                try
                                {
                                    logMessages.DeletingFileMessage(file, locationIdentifier, ipaddress);

                                    await _client.DeleteFileAsync(file, cancelToken);
                                }
                                catch (ControllerDeleteFileException e)
                                {
                                    logMessages.DeleteFileException(file, locationIdentifier, ipaddress, e);
                                }
                                catch (OperationCanceledException e)
                                {
                                    logMessages.OperationCancelledException(locationIdentifier, ipaddress, e);
                                }

                                logMessages.DeletedFileMessage(file, locationIdentifier, ipaddress);
                            }

                            //HACK: don't know why files aren't downloading without throwing an error
                            if (downloadedFile != null)
                            {
                                logMessages.DownloadedFileMessage(file, locationIdentifier, ipaddress);

                                progress?.Report(new ControllerDownloadProgress(downloadedFile, current, total));

                                yield return Tuple.Create<Device, FileInfo>(parameter, downloadedFile);
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

        #endregion

        ///<inheritdoc/>
        protected override void DisposeManagedCode()
        {
            _client.Dispose();
        }
    }
}
