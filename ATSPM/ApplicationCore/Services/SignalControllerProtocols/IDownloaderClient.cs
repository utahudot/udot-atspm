using ATSPM.Application.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ATSPM.Application.Services.SignalControllerProtocols
{
    public interface IDownloaderClient : IDisposable
    {
        bool IsConnected { get; }

        Task ConnectAsync(NetworkCredential credentials, int connectionTimeout = 1000, int operationTImeout = 1000, CancellationToken token = default);

        Task<IEnumerable<string>> ListDirectoryAsync(string directory, CancellationToken token = default, params string[] filters);

        Task<FileInfo> DownloadFileAsync(string localPath, string remotePath, CancellationToken token = default);

        Task DeleteFileAsync(string path, CancellationToken token = default);

        Task DisconnectAsync(CancellationToken token = default);
    }

    public interface IFTPDownloaderClient : IDownloaderClient
    {

    }

    public interface ISFTPDownloaderClient : IDownloaderClient
    {

    }

    public interface IHTTPDownloaderClient : IDownloaderClient
    {

    }
}
