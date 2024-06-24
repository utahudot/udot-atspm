#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Services.DownloaderClients/SSHNetSFTPDownloaderClient.cs
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
using ATSPM.Application.Exceptions;
using ATSPM.Application.Services;
using ATSPM.Domain.BaseClasses;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Services.DownloaderClients
{
    /// <summary>
    /// Connect to services and interact with their file directories using <see cref="ISftpClientWrapper"/>
    /// </summary>
    public class SSHNetSFTPDownloaderClient : ServiceObjectBase, ISFTPDownloaderClient
    {
        private ISftpClientWrapper _client;

        ///<inheritdoc/>
        public SSHNetSFTPDownloaderClient() { }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client">External client used for special settings and mocking</param>
        public SSHNetSFTPDownloaderClient(ISftpClientWrapper client)
        {
            _client = client;
        }

        #region ISFTPDownloaderClient

        ///<inheritdoc/>
        public bool IsConnected => _client != null && _client.IsConnected;

        ///<inheritdoc/>
        public Task ConnectAsync(NetworkCredential credentials, int connectionTimeout = 2, int operationTImeout = 2, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(credentials.Domain))
                throw new ArgumentNullException("Network Credentials can't be null");

            try
            {
                if (_client == null)
                {
                    var connectionInfo = new ConnectionInfo
                (credentials.Domain,
                credentials.UserName,
                new PasswordAuthenticationMethod(credentials.UserName, credentials.Password))
                    {
                        Timeout = TimeSpan.FromMilliseconds(connectionTimeout)
                    };

                    _client ??= new SftpClientWrapper(connectionInfo);

                    _client.OperationTimeout = TimeSpan.FromMilliseconds(operationTImeout);
                }

                _client.Connect();

                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                throw new ControllerConnectionException(credentials.Domain, this, e.Message, e);
            }
        }

        ///<inheritdoc/>
        public Task DeleteFileAsync(string path, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new ControllerConnectionException("", this, "Client not connected");

            try
            {
                _client.DeleteFile(path);

                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                throw new ControllerDeleteFileException(path, this, e.Message, e);
            }
        }

        ///<inheritdoc/>
        public Task DisconnectAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new ControllerConnectionException("", this, "Client not connected");

            try
            {
                _client.Disconnect();
            }
            catch (Exception e)
            {
                throw new ControllerConnectionException(_client.ConnectionInfo.Host, this, e.Message, e);
            }

            return Task.CompletedTask;
        }

        ///<inheritdoc/>
        public async Task<FileInfo> DownloadFileAsync(string localPath, string remotePath, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new ControllerConnectionException("", this, "Client not connected");

            try
            {
                return await _client.DownloadFileAsync(localPath, remotePath);
            }
            catch (Exception e)
            {
                throw new ControllerDownloadFileException(remotePath, this, e.Message, e);
            }
        }

        ///<inheritdoc/>
        public async Task<IEnumerable<string>> ListDirectoryAsync(string directory, CancellationToken token = default, params string[] filters)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new ControllerConnectionException("", this, "Client not connected");

            try
            {
                return await _client.ListDirectoryAsync(directory, filters);
            }
            catch (Exception e)
            {
                throw new ControllerListDirectoryException(directory, this, e.Message);
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

    /// <summary>
    /// Interface wrapper for Renci.SshNet so it can be mocked, tested and injected
    /// </summary>
    public interface ISftpClientWrapper : ISftpClient
    {
        ConnectionInfo ConnectionInfo { get; }
        bool IsConnected { get; }
        TimeSpan KeepAliveInterval { get; set; }

        event EventHandler<ExceptionEventArgs> ErrorOccurred;
        event EventHandler<HostKeyEventArgs> HostKeyReceived;

        void Connect();
        void Disconnect();
        void Dispose();
        void SendKeepAlive();

        Task<FileInfo> DownloadFileAsync(string localPath, string remotePath);

        Task<IEnumerable<string>> ListDirectoryAsync(string directory, params string[] filters);
    }

    /// <summary>
    /// Implementation wrapper for Renci.SshNet so it can be mocked, tested and injected
    /// </summary>
    public class SftpClientWrapper : SftpClient, ISftpClientWrapper
    {
        public SftpClientWrapper(ConnectionInfo connectionInfo) : base(connectionInfo) { }

        public SftpClientWrapper(string host, string username, string password) : base(host, username, password) { }

        public SftpClientWrapper(string host, string username, params PrivateKeyFile[] keyFiles) : base(host, username, keyFiles) { }

        public SftpClientWrapper(string host, int port, string username, string password) : base(host, port, username, password) { }

        public SftpClientWrapper(string host, int port, string username, params PrivateKeyFile[] keyFiles) : base(host, port, username, keyFiles) { }

        public async Task<FileInfo> DownloadFileAsync(string localPath, string remotePath)
        {
            var fileInfo = new FileInfo(localPath);
            fileInfo.Directory.Create();

            using (FileStream fileStream = fileInfo.Create())
            {
                await Task.Factory.FromAsync(BeginDownloadFile(remotePath, fileStream), EndDownloadFile);
            }

            return fileInfo;
        }

        public async Task<IEnumerable<string>> ListDirectoryAsync(string directory, params string[] filters)
        {
            var files = await Task.Factory.FromAsync(BeginListDirectory(directory, null, null), EndListDirectory);

            return files.Select(s => s.FullName).Where(f => filters.Any(a => f.Contains(a))).ToList();
        }
    }
}
