using ATSPM.Application.Exceptions;
using ATSPM.Application.Services.LocationControllerProtocols;
using ATSPM.Domain.BaseClasses;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Services.ControllerDownloaders
{
    public class SSHNetSFTPDownloaderClient : ServiceObjectBase, ISFTPDownloaderClient
    {
        public ISftpClientWrapper Client;

        #region ISFTPDownloaderClient

        public bool IsConnected => Client != null && Client.IsConnected;

        public Task ConnectAsync(NetworkCredential credentials, int connectionTimeout = 2, int operationTImeout = 2, CancellationToken token = default)
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

                Client ??= new SftpClientWrapper(connectionInfo);

                Client.OperationTimeout = TimeSpan.FromSeconds(operationTImeout);

                //await Task.Run(() => Client.Connect(), token);
                Client.Connect();

                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                throw new ControllerConnectionException(credentials.Domain, this, e.Message, e);
            }
        }

        public Task DeleteFileAsync(string path, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new ControllerConnectionException("", this, "Client not connected");

            try
            {
                //Task.Run(() => Client.DeleteFile(path), token);
                Client.DeleteFile(path);

                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                throw new ControllerDeleteFileException(path, this, e.Message, e);
            }
        }

        public Task DisconnectAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new ControllerConnectionException("", this, "Client not connected");

            try
            {
                //await Task.Run(() => Client.Disconnect(), token);
                Client.Disconnect();

                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                throw new ControllerConnectionException(Client.ConnectionInfo.Host, this, e.Message, e);
            }
        }

        public async Task<FileInfo> DownloadFileAsync(string localPath, string remotePath, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new ControllerConnectionException("", this, "Client not connected");

            try
            {
                return await Client.DownloadFileAsync(localPath, remotePath);
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
                return await Client.ListDirectoryAsync(directory, filters);
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
