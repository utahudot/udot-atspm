using ATSPM.Application.Exceptions;
using ATSPM.Application.Services;
using ATSPM.Domain.BaseClasses;
using FluentFTP;
using Google.Apis.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Services.DownloaderClients
{
    public class FluentFTPDownloaderClient : ServiceObjectBase, IFTPDownloaderClient
    {
        public IAsyncFtpClient Client;

        #region IFTPDownloaderClient

        public bool IsConnected => Client != null && Client.IsConnected;

        public async Task ConnectAsync(NetworkCredential credentials, int connectionTimeout = 2000, int operationTImeout = 2000, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                if (string.IsNullOrEmpty(credentials.UserName) || string.IsNullOrEmpty(credentials.Password) || string.IsNullOrEmpty(credentials.Domain))
                    throw new ArgumentNullException("Network Credentials can't be null");

                Client ??= new AsyncFtpClient(credentials.Domain, credentials);
                Client.Config.DataConnectionConnectTimeout = connectionTimeout;
                Client.Config.DataConnectionReadTimeout = operationTImeout;
                Client.Config.ConnectTimeout = connectionTimeout;
                Client.Config.ReadTimeout = operationTImeout;
                Client.Config.DataConnectionType = FtpDataConnectionType.AutoActive;

                var result = await Client.AutoConnect(token);
                //await Client.Connect(token);
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

            try
            {
                await Client.DeleteFile(path, token);
            }
            catch (Exception e)
            {
                throw new ControllerDeleteFileException(path, this, e.Message, e);
            }
        }

        public async Task DisconnectAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new ControllerConnectionException("", this, "Client not connected");

            try
            {
                await Client.Disconnect(token);
            }
            catch (Exception e)
            {
                throw new ControllerConnectionException(Client.Host, this, e.Message, e);
            }
        }

        public async Task<FileInfo> DownloadFileAsync(string localPath, string remotePath, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new ControllerConnectionException("", this, "Client not connected");

            try
            {
                var fileInfo = new FileInfo(localPath);
                fileInfo.Directory.Create();

                await Client.DownloadFile(localPath, remotePath, FtpLocalExists.Overwrite, FtpVerify.None, null, token);

                return fileInfo;
            }
            catch (Exception e)
            {
                throw new ControllerDownloadFileException(remotePath, this, e.Message, e);
            }
        }

        public async Task<IEnumerable<string>> ListDirectoryAsync(string directory, CancellationToken token = default, params string[] filters)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new ControllerConnectionException("", this, "Client not connected");

            try
            {
                var results = await Client.GetListing(directory, FtpListOption.Auto, token);

                return results.Select(s => s.FullName).Where(f => filters.Any(a => f.Contains(a))).ToList();
            }
            catch (Exception e)
            {
                throw new ControllerListDirectoryException(directory, this, e.Message);
            }
        }

        #endregion

        protected override void DisposeManagedCode()
        {
            if (Client != null)
            {
                if (Client.IsConnected)
                {
                    Client.Disconnect();
                }
                Client.Dispose();
                Client = null;
            }
        }
    }
}
