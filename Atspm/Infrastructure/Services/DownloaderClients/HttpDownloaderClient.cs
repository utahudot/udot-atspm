#region license
// Copyright 2025 Utah Departement of Transportation
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
    public class HttpDownloaderClient : DownloaderClientBase
    {
        private HttpClient _client;

        ///<inheritdoc/>
        public HttpDownloaderClient() { }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client">External client used for special settings and mocking</param>
        public HttpDownloaderClient(HttpClient client)
        {
            _client = client;
        }

        #region IDownloaderClient

        ///<inheritdoc/>
        public override TransportProtocols Protocol => TransportProtocols.Http;

        ///<inheritdoc/>
        public override bool IsConnected => _client != null && _client.BaseAddress.Host.IsValidIpAddress();

        ///<inheritdoc/>
        protected override Task Connect(IPEndPoint connection, NetworkCredential credentials, int connectionTimeout = 2000, int operationTimeout = 2000, Dictionary<string, string> connectionProperties = null, CancellationToken token = default)
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

        ///<inheritdoc/>
        protected override Task DeleteResource(Uri resource, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        ///<inheritdoc/>
        protected override Task Disconnect(CancellationToken token = default)
        {
            _client.CancelPendingRequests();

            return Task.CompletedTask;
        }

        ///<inheritdoc/>
        protected override async Task<FileInfo> DownloadResource(FileInfo file, Uri remote, CancellationToken token = default)
        {
            var response = await _client.GetAsync(remote, token);

            if (response.IsSuccessStatusCode && response?.Content != null)
            {
                var data = await response.Content.ReadAsStringAsync(token);

                await File.WriteAllTextAsync(file.FullName, data, token).ConfigureAwait(false);

                return file;
            }
            else
                throw new HttpRequestException(response.StatusCode.ToString());
        }

        ///<inheritdoc/>
        protected override Task<IEnumerable<Uri>> ListResources(string path, CancellationToken token = default, params string[] query)
        {
            List<Uri> results = [];

            if (Uri.TryCreate(_client.BaseAddress, path, out Uri baseAddress) && Uri.IsWellFormedUriString(baseAddress.ToString(), UriKind.Absolute))
            {
                if (query.Length <= 0)
                {
                    results.Add(baseAddress);
                }
                else
                {
                    foreach (var f in query)
                    {
                        if (Uri.IsWellFormedUriString(Uri.EscapeDataString(f), UriKind.Relative) && Uri.TryCreate(baseAddress + f, UriKind.Absolute, out Uri result))
                        {
                            results.Add(result);
                        }
                        else
                        {
                            throw new UriFormatException($"Invalid search term: {f}");
                        }
                    }
                }
            }
            else
            {
                throw new UriFormatException($"Invalid path: {path}");
            }

            return Task.FromResult<IEnumerable<Uri>>(results);
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
