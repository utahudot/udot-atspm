﻿using ATSPM.Data.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ATSPM.Application.Services
{
    /// <summary>
    /// Client for connecting to services and interacting with their file directories
    /// </summary>
    public interface IDownloaderClient : IDisposable
    {
        /// <summary>
        /// Tracks the connections status
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Open connection to the server
        /// </summary>
        /// <param name="credentials">Uwername/password</param>
        /// <param name="connectionTimeout">Timeout for connections</param>
        /// <param name="operationTImeout">Timeout for operations</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        Task ConnectAsync(NetworkCredential credentials, int connectionTimeout = 1000, int operationTImeout = 1000, CancellationToken token = default);

        /// <summary>
        /// List files in directory
        /// </summary>
        /// <param name="directory">Directory to list</param>
        /// <param name="token">Cancellation token</param>
        /// <param name="filters">File type and name filters</param>
        /// <returns></returns>
        Task<IEnumerable<string>> ListDirectoryAsync(string directory, CancellationToken token = default, params string[] filters);

        /// <summary>
        /// Download from remote path to local path
        /// </summary>
        /// <param name="localPath">Path to download directory to</param>
        /// <param name="remotePath">Path of directory to download from</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        Task<FileInfo> DownloadFileAsync(string localPath, string remotePath, CancellationToken token = default);

        /// <summary>
        /// Delete files in directory
        /// </summary>
        /// <param name="path">Directory of files to delete</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        Task DeleteFileAsync(string path, CancellationToken token = default);

        /// <summary>
        /// Disconnects from the server
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        Task DisconnectAsync(CancellationToken token = default);
    }

    /// <summary>
    /// Client for connecting and downloading files via FTP servers
    /// </summary>
    public interface IFTPDownloaderClient : IDownloaderClient
    {
    }

    /// <summary>
    /// Client for connecting and downloading files via SFTP servers
    /// </summary>
    public interface ISFTPDownloaderClient : IDownloaderClient
    {
    }

    /// <summary>
    /// Client for connecting and downloading files via HTTP servers
    /// </summary>
    public interface IHTTPDownloaderClient : IDownloaderClient
    {
    }

    /// <summary>
    /// Client for connecting and downloading files via SNMP servers
    /// </summary>
    public interface ISNMPDownloaderClient : IDownloaderClient
    {
    }
}