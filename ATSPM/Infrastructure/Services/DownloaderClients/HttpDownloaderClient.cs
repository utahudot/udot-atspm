#region license
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
using ATSPM.Application.Exceptions;
using ATSPM.Application.Services;
using ATSPM.Domain.BaseClasses;
using ATSPM.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Services.DownloaderClients
{
    ///<inheritdoc/>
    public class HttpDownloaderClient : ServiceObjectBase, IHTTPDownloaderClient
    {
        private HttpClient client;
        private Uri getPath;

        #region IHTTPDownloaderClient

        ///<inheritdoc/>
        public bool IsConnected => client != null && client.BaseAddress.Host.IsValidIPAddress();

        ///<inheritdoc/>
        public Task<bool> ConnectAsync(NetworkCredential credentials, int connectionTimeout = 2000, int operationTImeout = 2000, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                if (string.IsNullOrEmpty(credentials.UserName) || string.IsNullOrEmpty(credentials.Password) || string.IsNullOrEmpty(credentials.Domain))
                    throw new ArgumentNullException("Network Credentials can't be null");

                client ??= new HttpClient() { Timeout = TimeSpan.FromMilliseconds(operationTImeout), BaseAddress = new Uri($"http://{credentials.Domain}/") };
                //client ??= new HttpClient() { BaseAddress = new Uri($"http://{credentials.Domain}/") };

                client.DefaultRequestHeaders.Accept.Clear();
                //HACK: this is specific to maxtimecontrollers future versions will need this adjustable
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/xml"));

                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                throw new ControllerConnectionException(credentials.Domain, this, e.Message, e);
            }
        }

        ///<inheritdoc/>
        public async Task DeleteFileAsync(string path, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new ControllerConnectionException("", this, "Client not connected");

            await Task.CompletedTask;
        }

        ///<inheritdoc/>
        public async Task DisconnectAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new ControllerConnectionException("", this, "Client not connected");

            client.CancelPendingRequests();

            getPath = null;

            await Task.CompletedTask;
        }

        ///<inheritdoc/>
        public async Task<FileInfo> DownloadFileAsync(string localPath, string remotePath, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new ControllerConnectionException("", this, "Client not connected");

            if (getPath == null)
                throw new ControllerDownloadFileException(remotePath, this, "HTTP Get Path not defined");

            HttpResponseMessage response = new HttpResponseMessage();

            var sw = new Stopwatch();
            
            try
            {
                response = await client.GetAsync(getPath, token);

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
                    throw new ControllerDownloadFileException(remotePath, this, response.StatusCode.ToString());
            }
            catch (Exception e)
            {
                throw new ControllerDownloadFileException(remotePath, this, e.Message, e);
            }
        }

        ///<inheritdoc/>
        public Task<IEnumerable<string>> ListDirectoryAsync(string directory, CancellationToken token = default, params string[] filters)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new ControllerConnectionException("", this, "Client not connected");

            try
            {
                var builder = new UriBuilder("http", client.BaseAddress.Host.ToString(), 80, directory);

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
                throw new ControllerListDirectoryException(directory, this, e.Message);
            }
        }

        #endregion

        ///<inheritdoc/>
        protected override void DisposeManagedCode()
        {
            if (client != null)
            {
                client.CancelPendingRequests();
                client.Dispose();
                client = null;
            }
        }
    }
}
