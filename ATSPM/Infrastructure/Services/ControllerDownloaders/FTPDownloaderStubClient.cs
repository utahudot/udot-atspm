using ATSPM.Application.Exceptions;
using ATSPM.Application.Services;
using ATSPM.Domain.BaseClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Services.ControllerDownloaders
{
    public class FTPDownloaderStubClient : ServiceObjectBase, IFTPDownloaderClient
    {
        private bool _connected;

        public bool IsConnected => _connected;

        public async Task ConnectAsync(NetworkCredential credentials, int connectionTimeout = 1000, int operationTImeout = 1000, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(credentials.UserName) || string.IsNullOrEmpty(credentials.Password) || string.IsNullOrEmpty(credentials.Domain))
                throw new ArgumentNullException("Network Credentials can't be null");

            await Task.Delay(TimeSpan.FromMilliseconds(250));

            _connected = true;
        }

        public async Task DeleteFileAsync(string path, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new ControllerConnectionException("", this, "Client not connected");

            await Task.Delay(TimeSpan.FromMilliseconds(250));

        }

        public async Task DisconnectAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new ControllerConnectionException("", this, "Client not connected");

            await Task.Delay(TimeSpan.FromMilliseconds(250));

            _connected = false;
        }

        public Task<FileInfo> DownloadFileAsync(string localPath, string remotePath, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new ControllerConnectionException("", this, "Client not connected");

            return Task.FromResult(new FileInfo(remotePath));
        }

        public Task<IEnumerable<string>> ListDirectoryAsync(string directory, CancellationToken token = default, params string[] filters)
        {
            token.ThrowIfCancellationRequested();

            if (!IsConnected)
                throw new ControllerConnectionException("", this, "Client not connected");

            var dir = new DirectoryInfo("C:\\temp\\ControllerLogs\\1065");

            return Task.FromResult(dir.GetFiles().Select(s => s.FullName));
        }
    }
}
