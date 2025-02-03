#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Services.DownloaderClients/HttpDownloaderClient.cs
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

using System.Net;
using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.Infrastructure.Services.DownloaderClients
{
    /// <summary>
    /// Connect to services and interact with their file directories using <see cref="HttpClient"/>
    /// </summary>
    public class HttpDownloaderClient : ServiceObjectBase, IDownloaderClient
    {
        private HttpClient _client;

        ///<inheritdoc/>
        public HttpDownloaderClient() : base(true) { }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client">External client used for special settings and mocking</param>
        public HttpDownloaderClient(HttpClient client) : base(true)
        {
            _client = client;
        }

        #region IDownloaderClient

        ///<inheritdoc/>
        public TransportProtocols Protocol => TransportProtocols.Http;

        ///<inheritdoc/>
        public bool IsConnected => _client != null && _client.BaseAddress.Host.IsValidIpAddress();

        ///<inheritdoc/>
        public Task ConnectAsync(IPEndPoint connection, NetworkCredential credentials, int connectionTimeout = 2000, int operationTimeout = 2000, Dictionary<string, string> connectionProperties = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                _client ??= new HttpClient();
                _client.Timeout = TimeSpan.FromMilliseconds(operationTimeout);

                var uriBuilder = new UriBuilder(Uri.UriSchemeHttp, connection.Address.ToString(), connection.Port)
                {
                    UserName = credentials.UserName,
                    Password = credentials.Password,
                };

                if (Uri.TryCreate(uriBuilder.Uri.ToString(), UriKind.Absolute, out Uri baseAddress) && Uri.IsWellFormedUriString(baseAddress.ToString(), UriKind.Absolute))
                {
                    _client.BaseAddress = baseAddress;
                }
                else
                {
                    throw new UriFormatException($"Invalid Uri: {uriBuilder.Uri}");
                }

                if (connectionProperties != null && connectionProperties.Count > 0)
                {
                    _client.DefaultRequestHeaders.Accept.Clear();

                    foreach (var prop in connectionProperties)
                    {
                        _client.DefaultRequestHeaders.Add(prop.Key, prop.Value);
                    }
                }

                return Task.CompletedTask;
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
                throw new DownloaderClientConnectionException(_client?.BaseAddress?.Host, this, "Client not connected");

            await Task.CompletedTask;
        }

        ///<inheritdoc/>
        public async Task DisconnectAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new DownloaderClientConnectionException(_client?.BaseAddress?.Host, this, "Client not connected");

            try
            {
                _client.CancelPendingRequests();
                _client = null;
            }
            catch (Exception e)
            {
                throw new DownloaderClientConnectionException(_client?.BaseAddress?.Host, this, e.Message, e);
            }

            await Task.CompletedTask;
        }

        ///<inheritdoc/>
        public async Task<FileInfo> DownloadFileAsync(string localPath, string remotePath, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new DownloaderClientConnectionException(_client?.BaseAddress?.Host, this, "Client not connected");

            try
            {
                if (Uri.TryCreate(remotePath, UriKind.Absolute, out Uri request) && Uri.IsWellFormedUriString(Uri.EscapeDataString(request.Query), UriKind.Relative))
                {
                    var response = await _client.GetAsync(request, token);

                    if (response.IsSuccessStatusCode && response?.Content != null)
                    {
                        var data = await response.Content.ReadAsStringAsync();

                        var fileInfo = new FileInfo(localPath);
                        fileInfo.Directory.Create();

                        await File.WriteAllTextAsync(localPath, data, token).ConfigureAwait(false);

                        return fileInfo;
                    }
                    else
                        throw new HttpRequestException(response.StatusCode.ToString());
                }
                else
                {
                    throw new UriFormatException($"Invalid Uri: {remotePath}");
                }
            }
            catch (Exception e)
            {
                throw new DownloaderClientDownloadFileException(remotePath, this, e.Message, e);
            }
        }

        ///<inheritdoc/>
        public Task<IEnumerable<string>> ListDirectoryAsync(string directory, CancellationToken token = default, params string[] filters)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new DownloaderClientConnectionException(_client?.BaseAddress?.Host, this, "Client not connected");

            List<string> results = new List<string>();

            try
            {
                if (Uri.TryCreate(_client.BaseAddress, directory, out Uri baseAddress) && Uri.IsWellFormedUriString(baseAddress.ToString(), UriKind.Absolute))
                {
                    foreach (var f in filters)
                    {
                        if (Uri.IsWellFormedUriString(Uri.EscapeDataString(f), UriKind.Relative) && Uri.TryCreate(baseAddress + f, UriKind.Absolute, out Uri result))
                        {
                            results.Add(result.ToString());
                        }
                        else
                        {
                            throw new UriFormatException($"Invalid search term: {f}");
                        }
                    }
                }
                else
                {
                    throw new UriFormatException($"Invalid directory: {directory}");
                }

                return Task.FromResult<IEnumerable<string>>(results);
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
                _client.CancelPendingRequests();
                _client.Dispose();
                _client = null;
            }
        }
    }
}
