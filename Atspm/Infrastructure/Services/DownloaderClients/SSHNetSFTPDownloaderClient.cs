#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Services.DownloaderClients/SSHNetSFTPDownloaderClient.cs
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

using Renci.SshNet;
using Renci.SshNet.Common;
using System.Net;
using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.Infrastructure.Services.DownloaderClients
{
    /// <summary>
    /// Connect to services and interact with their file directories using <see cref="ISftpClientWrapper"/>
    /// </summary>
    public class SSHNetSFTPDownloaderClient : ServiceObjectBase, IDownloaderClient
    {
        private ISftpClientWrapper _client;

        ///<inheritdoc/>
        public SSHNetSFTPDownloaderClient() : base(true) { }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client">External client used for special settings and mocking</param>
        public SSHNetSFTPDownloaderClient(ISftpClientWrapper client) : base(true)
        {
            _client = client;
        }

        #region IDownloaderClient

        ///<inheritdoc/>
        public TransportProtocols Protocol => TransportProtocols.Sftp;

        ///<inheritdoc/>
        public bool IsConnected => _client != null && _client.IsConnected;

        ///<inheritdoc/>
        public Task ConnectAsync(IPEndPoint connection, NetworkCredential credentials, int connectionTimeout = 2, int operationTImeout = 2, Dictionary<string, string> connectionProperties = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                var connectionInfo = new ConnectionInfo
                (connection.Address.ToString(),
                connection.Port,
                credentials.UserName,
                new PasswordAuthenticationMethod(credentials.UserName, credentials.Password))
                {
                    Timeout = TimeSpan.FromMilliseconds(connectionTimeout)
                };

                _client ??= new SftpClientWrapper(connectionInfo);

                _client.OperationTimeout = TimeSpan.FromMilliseconds(operationTImeout);

                _client.Connect();

                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                throw new DownloaderClientConnectionException(credentials.Domain, this, e.Message, e);
            }
        }

        ///<inheritdoc/>
        public Task DeleteFileAsync(string path, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new DownloaderClientConnectionException("", this, "Client not connected");

            try
            {
                _client.DeleteFile(path);

                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                throw new DownloaderClientDeleteFileException(path, this, e.Message, e);
            }
        }

        ///<inheritdoc/>
        public Task DisconnectAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new DownloaderClientConnectionException("", this, "Client not connected");

            try
            {
                _client.Disconnect();
            }
            catch (Exception e)
            {
                throw new DownloaderClientConnectionException(_client.ConnectionInfo.Host, this, e.Message, e);
            }

            return Task.CompletedTask;
        }

        ///<inheritdoc/>
        public async Task<FileInfo> DownloadResourceAsync(string localPath, string remotePath, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new DownloaderClientConnectionException("", this, "Client not connected");

            try
            {
                return await _client.DownloadFileAsync(localPath, remotePath);
            }
            catch (Exception e)
            {
                throw new DownloaderClientDownloadResourceException(remotePath, this, e.Message, e);
            }
        }

        ///<inheritdoc/>
        public async Task<IEnumerable<Uri>> ListResourcesAsync(string path, CancellationToken token = default, params string[] query)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new DownloaderClientConnectionException("", this, "Client not connected");

            try
            {
                var result = await _client.ListDirectoryAsync(path, query);
                    
                return result.Select(s => new UriBuilder(Uri.UriSchemeSftp, _client.ConnectionInfo.Host, _client.ConnectionInfo.Port, s).Uri).ToList();
            }
            catch (Exception e)
            {
                throw new DownloaderClientListResourcesException(path, this, e.Message);
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
