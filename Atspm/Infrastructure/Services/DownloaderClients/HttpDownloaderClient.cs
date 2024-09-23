﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Services.DownloaderClients/HttpDownloaderClient.cs
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

using System.Diagnostics;
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
        private Uri getPath;
        private IPEndPoint ip;

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
        public Task ConnectAsync(IPEndPoint connection, NetworkCredential credentials, int connectionTimeout = 2000, int operationTImeout = 2000, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            ip = connection;

            try
            {
                _client ??= new HttpClient();

                _client.Timeout = TimeSpan.FromMilliseconds(operationTImeout);
                _client.BaseAddress = new Uri($"http://{ip.Address}:{ip.Port}/");

                _client.DefaultRequestHeaders.Accept.Clear();
                //HACK: this is specific to maxtimecontrollers future versions will need this adjustable
                _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/xml"));

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

            getPath = null;

            await Task.CompletedTask;
        }

        ///<inheritdoc/>
        public async Task<FileInfo> DownloadFileAsync(string localPath, string remotePath, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new DownloaderClientConnectionException(_client?.BaseAddress?.Host, this, "Client not connected");

            //if (getPath == null)
            //    throw new DownloaderClientDownloadFileException(remotePath, this, "HTTP Get Path not defined");

            HttpResponseMessage response = new HttpResponseMessage();

            var sw = new Stopwatch();

            try
            {
                response = await _client.GetAsync(getPath, token);

                sw.Stop();

                if (response.IsSuccessStatusCode && response?.Content != null)
                {
                    string data = await response.Content.ReadAsStringAsync();

                    var fileInfo = new FileInfo(localPath);
                    fileInfo.Directory.Create();

                    await File.WriteAllTextAsync(localPath, data, token).ConfigureAwait(false);

                    return fileInfo;
                }
                else
                    throw new DownloaderClientDownloadFileException(remotePath, this, response.StatusCode.ToString());
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

            try
            {
                var builder = new UriBuilder(_client.BaseAddress);
                builder.Path = directory;

                //for maxtime controllers it uses this searchterm:  $"since={DateTime.Now.AddHours(-24):MM-dd-yyyy HH:mm:ss.f}"

                //HACK: this is for maxtime controllers, needs to be moved to search terms in the db
                builder.Query = $"since={DateTime.Now.AddHours(-1):MM-dd-yyyy HH:mm:ss.f}";

                if (filters?.Length > 0)
                {
                    foreach (var filter in filters)
                    {
                        builder.Query += filter;
                    }
                }

                getPath = builder.Uri;

                return Task.FromResult<IEnumerable<string>>(new List<string>() { $"{DateTime.Now.Ticks}.xml" });
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
