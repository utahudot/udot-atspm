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
        /// <param name="operationTimeout">Timeout for operations</param>
        /// <param name="connectionProperties">Connection properties specific to the <see cref="Protocol"/></param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        /// <exception cref="DownloaderClientConnectionException"></exception>
        Task ConnectAsync(IPEndPoint connection, NetworkCredential credentials, int connectionTimeout = 2000, int operationTimeout = 2000, Dictionary<string, string> connectionProperties = null, CancellationToken token = default);

        /// <summary>
        /// Lists uri's of all resources
        /// </summary>
        /// <param name="path">Path to list resources from</param>
        /// <param name="token">Cancellation token</param>
        /// <param name="query">Uri query to filter resources</param>
        /// <returns>List of files in path</returns>
        /// <exception cref="DownloaderClientConnectionException"></exception>
        /// <exception cref="DownloaderClientListResourcesException"></exception>
        Task<IEnumerable<Uri>> ListResourcesAsync(string path, CancellationToken token = default, params string[] query);

        /// <summary>
        /// Download from remote path to local path
        /// </summary>
        /// <param name="localPath">Path to download path to</param>
        /// <param name="remotePath">Path of path to download from</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        /// <exception cref="DownloaderClientConnectionException"></exception>
        /// <exception cref="DownloaderClientDownloadFileException"></exception>
        Task<FileInfo> DownloadFileAsync(string localPath, string remotePath, CancellationToken token = default);

        /// <summary>
        /// Delete files in path
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
