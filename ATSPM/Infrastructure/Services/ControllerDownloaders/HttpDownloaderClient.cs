using ATSPM.Application.Exceptions;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Domain.BaseClasses;
using ATSPM.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace ATSPM.Infrastructure.Services.ControllerDownloaders
{
    public class HttpDownloaderClient : ServiceObjectBase, IHTTPDownloaderClient
    {
        public HttpClient Client;
        private Uri _getPath;

        #region IHTTPDownloaderClient

        public bool IsConnected => Client != null && Client.BaseAddress.Host.IsValidIPAddress();

        public async Task ConnectAsync(NetworkCredential credentials, int connectionTimeout = 2, int operationTImeout = 2, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                //if (string.IsNullOrEmpty(credentials.UserName) || string.IsNullOrEmpty(credentials.Password) || string.IsNullOrEmpty(credentials.Domain))
                //    throw new ArgumentNullException("Network Credentials can't be null");

                Client ??= new HttpClient() { Timeout = TimeSpan.FromSeconds(connectionTimeout), BaseAddress = new Uri($"http://{credentials.Domain}/") };

                Client.DefaultRequestHeaders.Accept.Clear();
                Client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/xml"));

                await Task.CompletedTask;
            }
            catch (Exception e)
            {
                throw new ControllerConnectionException(credentials.Domain, this, e.Message, e);
            }
        }

        public async Task DeleteFileAsync(string path, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new ControllerConnectionException("", this, "Client not connected");

            await Task.CompletedTask;
        }

        public async Task DisconnectAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new ControllerConnectionException("", this, "Client not connected");

            Client.CancelPendingRequests();

            _getPath = null;

            await Task.CompletedTask;
        }

        public async Task<FileInfo> DownloadFileAsync(string localPath, string remotePath, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new ControllerConnectionException("", this, "Client not connected");

            if (_getPath == null)
                throw new ControllerDownloadFileException(remotePath, this, "HTTP Get Path not defined");

            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                response = await Client.GetAsync(_getPath, token);

                if (response.IsSuccessStatusCode && response?.Content != null)
                {
                    string data = await response.Content.ReadAsStringAsync();

                    var fileInfo = new FileInfo(localPath);
                    fileInfo.Directory.Create();

                    XmlDocument xml = new XmlDocument();

                    xml.LoadXml(data);

                    xml.Save(fileInfo.FullName);

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

        public Task<IEnumerable<string>> ListDirectoryAsync(string directory, CancellationToken token = default, params string[] filters)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new ControllerConnectionException("", this, "Client not connected");

            try
            {
                var builder = new UriBuilder("http", Client.BaseAddress.Host.ToString(), 80, directory)
                {
                    //Query = $"since={DateTime.Now:MM-dd-yyyy} 00:00:00.0"
                    Query = filters.ToString()
                };

                _getPath = builder.Uri;

                return Task.FromResult<IEnumerable<string>>(new List<string>() { $"{DateTime.Now.Ticks}.xml" });
            }
            catch (Exception e)
            {
                throw new ControllerListDirectoryException(directory, this, e.Message);
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Client != null)
                {
                    Client.CancelPendingRequests();
                    Client.Dispose();
                    Client = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
