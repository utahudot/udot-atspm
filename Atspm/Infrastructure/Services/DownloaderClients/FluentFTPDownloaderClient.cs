#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Services.DownloaderClients/FluentFTPDownloaderClient.cs
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
    public class FluentFTPDownloaderClient : DownloaderClientBase
    {
        private IAsyncFtpClient _client;

        ///<inheritdoc/>
        public FluentFTPDownloaderClient() { }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client">External client used for special settings and mocking</param>
        public FluentFTPDownloaderClient(IAsyncFtpClient client)
        {
            _client = client;
        }

        #region IDownloaderClient

        ///<inheritdoc/>
        public override TransportProtocols Protocol => TransportProtocols.Ftp;

        ///<inheritdoc/>
        public override bool IsConnected => _client != null && _client.IsConnected;

        ///<inheritdoc/>
        protected override async Task Connect(IPEndPoint connection, NetworkCredential credentials, int connectionTimeout = 2000, int operationTimeout = 2000, Dictionary<string, string> connectionProperties = null, CancellationToken token = default)
        {
            if (_client == null)
            {
                _client = new AsyncFtpClient();
            }

            _client.Host = connection.Address.ToString();
            _client.Port = connection.Port;
            _client.Credentials = credentials;
            _client.Config.ConnectTimeout = connectionTimeout;
            _client.Config.ReadTimeout = operationTimeout;

            //https://github.com/robinrodricks/FluentFTP/wiki/Timeouts
            if (connectionProperties != null && connectionProperties.TryGetValue("DataConnectionConnectTimeout", out string v1) && int.TryParse(v1, out int r1))
            {
                _client.Config.DataConnectionConnectTimeout = r1;
            }

            if (connectionProperties != null && connectionProperties.TryGetValue("DataConnectionReadTimeout", out string v2) && int.TryParse(v2, out int r2))
            {
                _client.Config.DataConnectionReadTimeout = r2;
            }

            if (connectionProperties != null && connectionProperties.TryGetValue("DataConnectionType", out string v3) && Enum.TryParse(typeof(FtpDataConnectionType), v3, true, out object r3))
            {
                _client.Config.DataConnectionType = (FtpDataConnectionType)r3;
            }

            if (connectionProperties != null && connectionProperties.TryGetValue("AutoConnect", out string v4) && bool.TryParse(v4, out bool r4) && r4)
            {
                var result = await _client.AutoConnect(token);
            }
            else
            {
                await _client.Connect(token);
            }
        }

        ///<inheritdoc/>
        protected override async Task DeleteResource(Uri resource, CancellationToken token = default)
        {
            await _client.DeleteFile(resource.LocalPath, token);
        }

        ///<inheritdoc/>
        protected override async Task Disconnect(CancellationToken token = default)
        {
            await _client.Disconnect(token);
        }

        ///<inheritdoc/>
        protected override async Task<FileInfo> DownloadResource(FileInfo file, Uri remote, CancellationToken token = default)
        {
            FtpStatus result = await _client.DownloadFile(file.FullName, remote.LocalPath, FtpLocalExists.Overwrite, FtpVerify.None, null, token);

            if (result == FtpStatus.Success)
                return file;
            else
                return null;
        }

        ///<inheritdoc/>
        protected override async Task<IEnumerable<Uri>> ListResources(string path, CancellationToken token = default, params string[] query)
        {
            var result = await _client.GetListing(path, FtpListOption.Auto, token);

            return result
                .Where(w => w.Type == FtpObjectType.File)
                .Where(w => query.Any(a => w.Name.Contains(a)))
                .Where(w => w.RawModified != result.Max(m => m.RawModified))
                .Select(s => new UriBuilder(Uri.UriSchemeFtp, _client.Host, _client.Port, s.FullName).Uri)
                .ToList();
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