#region license
// Copyright 2024 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Services/IDownloaderClient.cs
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

using System.Net;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Exceptions;

namespace Utah.Udot.Atspm.Services
{
    /// <summary>
    /// Client for connecting to services and interacting with their file directories
    /// </summary>
    public interface IDownloaderClient : IDisposable
    {
        ///<inheritdoc cref="TransportProtocols"/>
        TransportProtocols Protocol { get; }

        /// <summary>
        /// Tracks the connections status
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Open connection to the server
        /// </summary>
        /// <param name="connection">Ip connection info</param>
        /// <param name="credentials">username/password</param>
        /// <param name="connectionTimeout">Timeout for connections</param>
        /// <param name="operationTImeout">Timeout for operations</param>
        /// <param name="connectionProperties">Connection properties specific to the <see cref="Protocol"/></param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        /// <exception cref="DownloaderClientConnectionException"></exception>
        Task ConnectAsync(IPEndPoint connection, NetworkCredential credentials, int connectionTimeout = 2000, int operationTImeout = 2000, Dictionary<string, string> connectionProperties = null, CancellationToken token = default);

        /// <summary>
        /// List files in directory
        /// </summary>
        /// <param name="directory">Directory to list</param>
        /// <param name="token">Cancellation token</param>
        /// <param name="filters">File type and name filters</param>
        /// <returns>List of files in directory</returns>
        /// <exception cref="DownloaderClientConnectionException"></exception>
        /// <exception cref="DownloaderClientListDirectoryException"></exception>
        Task<IEnumerable<string>> ListDirectoryAsync(string directory, CancellationToken token = default, params string[] filters);

        /// <summary>
        /// Download from remote path to local path
        /// </summary>
        /// <param name="localPath">Path to download directory to</param>
        /// <param name="remotePath">Path of directory to download from</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        /// <exception cref="DownloaderClientConnectionException"></exception>
        /// <exception cref="DownloaderClientDownloadFileException"></exception>
        Task<FileInfo> DownloadFileAsync(string localPath, string remotePath, CancellationToken token = default);

        /// <summary>
        /// Delete files in directory
        /// </summary>
        /// <param name="path">Directory of files to delete</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        /// <exception cref="DownloaderClientConnectionException"></exception>
        /// <exception cref="DownloaderClientDeleteFileException"></exception>
        Task DeleteFileAsync(string path, CancellationToken token = default);

        /// <summary>
        /// Disconnects from the server
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        /// <exception cref="DownloaderClientConnectionException"></exception>
        Task DisconnectAsync(CancellationToken token = default);
    }
}
