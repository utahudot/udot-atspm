﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Services.DownloaderClients/FluentFTPDownloaderClient.cs
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

using FluentFTP;
using System.Net;
using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.Infrastructure.Services.DownloaderClients
{
    /// <summary>
    /// Connect to services and interact with their file directories using <see cref="IAsyncFtpClient"/>
    /// </summary>
    public class FluentFTPDownloaderClient : ServiceObjectBase, IDownloaderClient
    {
        private IAsyncFtpClient _client;

        ///<inheritdoc/>
        public FluentFTPDownloaderClient() : base(true) { }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client">External client used for special settings and mocking</param>
        public FluentFTPDownloaderClient(IAsyncFtpClient client) : base(true)
        {
            _client = client;
        }

        #region IDownloaderClient

        ///<inheritdoc/>
        public TransportProtocols Protocol => TransportProtocols.Ftp;

        ///<inheritdoc/>
        public bool IsConnected => _client != null && _client.IsConnected;

        ///<inheritdoc/>
        public async Task ConnectAsync(IPEndPoint connection, NetworkCredential credentials, int connectionTimeout = 2000, int operationTImeout = 2000, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                if (_client == null)
                {
                    _client = new AsyncFtpClient();
                }

                _client.Host = connection.Address.ToString();
                _client.Port = connection.Port;
                _client.Credentials = credentials;
                _client.Config.DataConnectionConnectTimeout = connectionTimeout;
                _client.Config.DataConnectionReadTimeout = operationTImeout;
                _client.Config.ConnectTimeout = connectionTimeout;
                _client.Config.ReadTimeout = operationTImeout;
                _client.Config.DataConnectionType = FtpDataConnectionType.AutoActive;

                var result = await _client.AutoConnect(token);
                //await client.Connect(token);
            }
            catch (Exception e)
            {
                throw new DownloaderClientConnectionException(credentials.Domain, this, e.Message, e);
            }
        }

        ///<inheritdoc/>
        public async Task DeleteFileAsync(string path, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new DownloaderClientConnectionException(_client.Host, this, "Client not connected");

            try
            {
                await _client.DeleteFile(path, token);
            }
            catch (Exception e)
            {
                throw new DownloaderClientDeleteFileException(path, this, e.Message, e);
            }
        }

        ///<inheritdoc/>
        public async Task DisconnectAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new DownloaderClientConnectionException(_client.Host, this, "Client not connected");

            try
            {
                await _client.Disconnect(token);
            }
            catch (Exception e)
            {
                throw new DownloaderClientConnectionException(_client.Host, this, e.Message, e);
            }
        }

        ///<inheritdoc/>
        public async Task<FileInfo> DownloadFileAsync(string localPath, string remotePath, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new DownloaderClientConnectionException(_client.Host, this, "Client not connected");

            try
            {
                var fileInfo = new FileInfo(localPath);
                fileInfo.Directory.Create();

                await _client.DownloadFile(localPath, remotePath, FtpLocalExists.Overwrite, FtpVerify.None, null, token);

                return fileInfo;
            }
            catch (Exception e)
            {
                throw new DownloaderClientDownloadFileException(remotePath, this, e.Message, e);
            }
        }

        ///<inheritdoc/>
        public async Task<IEnumerable<string>> ListDirectoryAsync(string directory, CancellationToken token = default, params string[] filters)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new DownloaderClientConnectionException(_client.Host, this, "Client not connected");

            try
            {
                var results = await _client.GetListing(directory, FtpListOption.Auto, token);

                return results.Select(s => s.FullName).Where(f => filters.Any(a => f.Contains(a))).ToList();
            }
            catch (Exception e)
            {
                throw new DownloaderClientListDirectoryException(directory, this, e.Message);
            }
        }

        #endregion

        ///<inheritdoc/>
        protected override void DisposeManagedCode()
        {
            if (_client != null)
            {
                if (_client.IsConnected)
                {
                    _client.Disconnect();
                }
                _client.Dispose();
                _client = null;
            }
        }
    }
}
