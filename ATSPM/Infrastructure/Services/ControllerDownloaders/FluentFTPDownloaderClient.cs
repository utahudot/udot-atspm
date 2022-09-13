using ATSPM.Application.Exceptions;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Domain.BaseClasses;
using FluentFTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Services.ControllerDownloaders
{
    public class FluentFTPDownloaderClient : ServiceObjectBase, IFTPDownloaderClient
    {
        public IFtpClient Client;

        #region IFTPDownloaderClient

        public bool IsConnected => Client != null && Client.IsConnected;

        public async Task ConnectAsync(NetworkCredential credentials, int connectionTimeout = 2, int operationTImeout = 2, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                if (string.IsNullOrEmpty(credentials.UserName) || string.IsNullOrEmpty(credentials.Password) || string.IsNullOrEmpty(credentials.Domain))
                    throw new ArgumentNullException("Network Credentials can't be null");

                Client ??= new FtpClient(credentials.Domain, credentials);
                Client.DataConnectionConnectTimeout = connectionTimeout;
                Client.DataConnectionReadTimeout = operationTImeout;
                Client.ConnectTimeout = connectionTimeout;
                Client.ReadTimeout = operationTImeout;
                Client.DataConnectionType = FtpDataConnectionType.AutoActive;

                var result = await Client.AutoConnectAsync(token);
                //await Client.ConnectAsync(token);
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
                await Client.DeleteFileAsync(path, token);
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
                await Client.DisconnectAsync(token);
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

                await Client.DownloadFileAsync(localPath, remotePath, FtpLocalExists.Overwrite, FtpVerify.None, null, token);
                    
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
                var results = await Client.GetListingAsync(directory, FtpListOption.Auto, token);

                return results.Select(s => s.FullName).Where(f => filters.Any(a => f.Contains(a))).ToList();
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
                    if (Client.IsConnected)
                    {
                        Client.Disconnect();
                    }
                    Client.Dispose();
                    Client = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
