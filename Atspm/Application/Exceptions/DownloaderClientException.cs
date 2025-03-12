#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Exceptions/DownloaderClientException.cs
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

using Utah.Udot.Atspm.Services;

#nullable enable
namespace Utah.Udot.Atspm.Exceptions
{
    /// <summary>
    /// Exceptions for implementing <see cref="IDownloaderClient"/>
    /// </summary>
    public abstract class DownloaderClientException : AtspmException
    {
        /// <summary>
        /// Exceptions for implementing <see cref="IDownloaderClient"/>
        /// </summary>
        /// <param name="downloaderClient">Downloader client exception was thrown for</param>
        /// <param name="message">Exception message</param>
        public DownloaderClientException(IDownloaderClient downloaderClient, string? message) : base(message)
        {
            DownloaderClient = downloaderClient;
        }

        /// <summary>
        /// Exceptions for implementing <see cref="IDownloaderClient"/>
        /// </summary>
        /// <param name="downloaderClient">Downloader client exception was thrown for</param>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Exception thrown by the <see cref="IDownloaderClient"/> implementation</param>
        public DownloaderClientException(IDownloaderClient downloaderClient, string? message, Exception? innerException) : base(message, innerException)
        {
            DownloaderClient = downloaderClient;
        }

        /// <summary>
        /// Downloader client exception was thrown for
        /// </summary>
        public IDownloaderClient DownloaderClient { get; private set; }
    }

    /// <summary>
    /// Thrown where there is an exception connecting to <see cref="IDownloaderClient"/> implementation
    /// </summary>
    public class DownloaderClientConnectionException : DownloaderClientException
    {
        /// <summary>
        /// Thrown where there is an exception connecting to <see cref="IDownloaderClient"/> implementation
        /// </summary>
        /// <param name="host">IpAddress of remote host</param>
        /// <param name="downloaderClient">Downloader client exception was thrown for</param>
        /// <param name="message">Exception message</param>
        public DownloaderClientConnectionException(string host, IDownloaderClient downloaderClient, string? message) : base(downloaderClient, message)
        {
            Host = host;
        }

        /// <summary>
        /// Thrown where there is an exception connecting to <see cref="IDownloaderClient"/> implementation
        /// </summary>
        /// <param name="host">IpAddress of remote host</param>
        /// <param name="downloaderClient">Downloader client exception was thrown for</param>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Exception thrown by the <see cref="IDownloaderClient"/> implementation</param>
        public DownloaderClientConnectionException(string host, IDownloaderClient downloaderClient, string? message, Exception? innerException) : base(downloaderClient, message, innerException)
        {
            Host = host;
        }

        /// <summary>
        /// IpAddress of remote host
        /// </summary>
        public string Host { get; private set; }
    }

    /// <summary>
    /// Thrown when there is an exception downloading from <see cref="IDownloaderClient"/> implementation
    /// </summary>
    public class DownloaderClientDownloadResourceException : DownloaderClientException
    {
        /// <summary>
        /// Thrown when there is an exception downloading from <see cref="IDownloaderClient"/> implementation
        /// </summary>
        /// <param name="resource">Name of the resource that threw the exception when trying to download</param>
        /// <param name="downloaderClient">Downloader client exception was thrown for</param>
        /// <param name="message">Exception message</param>
        public DownloaderClientDownloadResourceException(Uri resource, IDownloaderClient downloaderClient, string? message) : base(downloaderClient, message)
        {
            Resource = resource;
        }

        /// <summary>
        /// Thrown when there is an exception downloading from <see cref="IDownloaderClient"/> implementation
        /// </summary>
        /// <param name="resource">Name of the resource that threw the exception when trying to download</param>
        /// <param name="downloaderClient">Downloader client exception was thrown for</param>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Exception thrown by the <see cref="IDownloaderClient"/> implementation</param>
        public DownloaderClientDownloadResourceException(Uri resource, IDownloaderClient downloaderClient, string? message, Exception? innerException) : base(downloaderClient, message, innerException)
        {
            Resource = resource;
        }

        /// <summary>
        /// Name of the resource that threw the exception when trying to download
        /// </summary>
        public Uri Resource { get; private set; }
    }

    /// <summary>
    /// Thrown when there is an exception deleting from <see cref="IDownloaderClient"/> implementation
    /// </summary>
    public class DownloaderClientDeleteResourceException : DownloaderClientException
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resource">Name of resource that that threw the exception when trying to delete</param>
        /// <param name="downloaderClient">Downloader client exception was thrown for</param>
        /// <param name="message">Exception message</param>
        public DownloaderClientDeleteResourceException(Uri resource, IDownloaderClient downloaderClient, string? message) : base(downloaderClient, message)
        {
            Resource = resource;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resource">Name of resource that that threw the exception when trying to delete</param>
        /// <param name="downloaderClient">Downloader client exception was thrown for</param>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Exception thrown by the <see cref="IDownloaderClient"/> implementation</param>
        public DownloaderClientDeleteResourceException(Uri resource, IDownloaderClient downloaderClient, string? message, Exception? innerException) : base(downloaderClient, message, innerException)
        {
            Resource = resource;
        }

        /// <summary>
        /// Name of file that that threw the exception when trying to delete
        /// </summary>
        public Uri Resource { get; private set; }
    }

    /// <summary>
    /// Thrown when there is an exception listing resources from <see cref="IDownloaderClient"/> implementation
    /// </summary>
    public class DownloaderClientListResourcesException : DownloaderClientException
    {
        /// <summary>
        /// Thrown when there is an exception listing resources from <see cref="IDownloaderClient"/> implementation
        /// </summary>
        /// <param name="path">Path of resources that threw the exception</param>
        /// <param name="downloaderClient">Downloader client exception was thrown for</param>
        /// <param name="message">Exception message</param>
        public DownloaderClientListResourcesException(string path, IDownloaderClient downloaderClient, string? message) : base(downloaderClient, message)
        {
            Path = path;
        }

        /// <summary>
        /// Thrown when there is an exception listing resources from <see cref="IDownloaderClient"/> implementation
        /// </summary>
        /// <param name="path">Path of resources that threw the exception</param>
        /// <param name="downloaderClient">Downloader client exception was thrown for</param>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Exception thrown by the <see cref="IDownloaderClient"/> implementation</param>
        public DownloaderClientListResourcesException(string path, IDownloaderClient downloaderClient, string? message, Exception? innerException) : base(downloaderClient, message, innerException)
        {
            Path = path;
        }

        /// <summary>
        /// Path of resources that threw the exception
        /// </summary>
        public string Path { get; private set; }
    }
}
