using ATSPM.Application.Common;
using ATSPM.Application.Configuration;
using ATSPM.Application.Exceptions;
using ATSPM.Application.Models;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Domain.BaseClasses;
using FluentFTP;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ATSPM.Infrasturcture.Services.ControllerDownloaders
{
    public class FluentFTPDownloaderClient : ServiceObjectBase, IFTPDownloaderClient
    {
        public IFtpClient Client;

        #region ISFTPDownloaderClient

        public bool IsConnected => Client != null && Client.IsConnected;

        public async Task ConnectAsync(NetworkCredential credentials, int connectionTimeout = 1000, int operationTImeout = 1000, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                var connectionInfo = new ConnectionInfo
                (credentials.Domain,
                credentials.UserName,
                new PasswordAuthenticationMethod(credentials.UserName, credentials.Password))
                {
                    Timeout = TimeSpan.FromSeconds(connectionTimeout)
                };

                Client ??= new FtpClient(credentials.Domain, credentials);

                Client.ConnectTimeout = connectionTimeout;
                Client.ReadTimeout = operationTImeout;
                Client.DataConnectionType = FtpDataConnectionType.AutoActive;

                await Client?.AutoConnectAsync(token);
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

            var fileInfo = new FileInfo(localPath);
            fileInfo.Directory.Create();

            try
            {
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

            var results = await Client.GetListingAsync(directory, FtpListOption.Auto, token);

            return results.Select(s => s.FullName).Where(f => filters.Any(a => f.Contains(a))).ToList();
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
