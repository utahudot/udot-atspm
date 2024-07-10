#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Exceptions/ControllerLoggerExceptions.cs
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
using ATSPM.Application.Services;
using System;
using System.Net;

#nullable enable
namespace ATSPM.Application.Exceptions
{
    /// <summary>
    /// Exceptions for implementing <see cref="IDownloaderClient"/>
    /// </summary>
    public abstract class DownloaderClientException : ATSPMException
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
    public class DownloaderClientDownloadFileException : DownloaderClientException
    {
        /// <summary>
        /// Thrown when there is an exception downloading from <see cref="IDownloaderClient"/> implementation
        /// </summary>
        /// <param name="fileName">Name of file that that threw the exception when trying to download</param>
        /// <param name="downloaderClient">Downloader client exception was thrown for</param>
        /// <param name="message">Exception message</param>
        public DownloaderClientDownloadFileException(string fileName, IDownloaderClient downloaderClient, string? message) : base(downloaderClient, message)
        {
            FileName = fileName;
        }

        /// <summary>
        /// Thrown when there is an exception downloading from <see cref="IDownloaderClient"/> implementation
        /// </summary>
        /// <param name="fileName">Name of file that that threw the exception when trying to download</param>
        /// <param name="downloaderClient">Downloader client exception was thrown for</param>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Exception thrown by the <see cref="IDownloaderClient"/> implementation</param>
        public DownloaderClientDownloadFileException(string fileName, IDownloaderClient downloaderClient, string? message, Exception? innerException) : base(downloaderClient, message, innerException)
        {
            FileName = fileName;
        }

        /// <summary>
        /// Name of file that that threw the exception when trying to download
        /// </summary>
        public string FileName { get; private set; }
    }

    /// <summary>
    /// Thrown when there is an exception deleting from <see cref="IDownloaderClient"/> implementation
    /// </summary>
    public class DownloaderClientDeleteFileException : DownloaderClientException
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName">Name of file that that threw the exception when trying to delete</param>
        /// <param name="downloaderClient">Downloader client exception was thrown for</param>
        /// <param name="message">Exception message</param>
        public DownloaderClientDeleteFileException(string fileName, IDownloaderClient downloaderClient, string? message) : base(downloaderClient, message)
        {
            FileName = fileName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName">Name of file that that threw the exception when trying to delete</param>
        /// <param name="downloaderClient">Downloader client exception was thrown for</param>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Exception thrown by the <see cref="IDownloaderClient"/> implementation</param>
        public DownloaderClientDeleteFileException(string fileName, IDownloaderClient downloaderClient, string? message, Exception? innerException) : base(downloaderClient, message, innerException)
        {
            FileName = fileName;
        }

        /// <summary>
        /// Name of file that that threw the exception when trying to delete
        /// </summary>
        public string FileName { get; private set; }
    }

    /// <summary>
    /// Thrown when there is an exception listing directory from <see cref="IDownloaderClient"/> implementation
    /// </summary>
    public class DownloaderClientListDirectoryException : DownloaderClientException
    {
        /// <summary>
        /// Thrown when there is an exception listing directory from <see cref="IDownloaderClient"/> implementation
        /// </summary>
        /// <param name="directory">Path of directory that threw the exception</param>
        /// <param name="downloaderClient">Downloader client exception was thrown for</param>
        /// <param name="message">Exception message</param>
        public DownloaderClientListDirectoryException(string directory, IDownloaderClient downloaderClient, string? message) : base(downloaderClient, message)
        {
            Directory = directory;
        }

        /// <summary>
        /// Thrown when there is an exception listing directory from <see cref="IDownloaderClient"/> implementation
        /// </summary>
        /// <param name="directory">Path of directory that threw the exception</param>
        /// <param name="downloaderClient">Downloader client exception was thrown for</param>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Exception thrown by the <see cref="IDownloaderClient"/> implementation</param>
        public DownloaderClientListDirectoryException(string directory, IDownloaderClient downloaderClient, string? message, Exception? innerException) : base(downloaderClient, message, innerException)
        {
            Directory = directory;
        }

        /// <summary>
        /// Path of directory that threw the exception
        /// </summary>
        public string Directory { get; private set; }
    }
}
