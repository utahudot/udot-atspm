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
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using ATSPM.Application.Services;
using System.Threading.Tasks.Dataflow;

#nullable enable
namespace ATSPM.Application.Exceptions
{
    public class InvalidSignalControllerIpAddressException : ATSPMException
    {
        public InvalidSignalControllerIpAddressException(Device device) : base($"{device.Location.LocationIdentifier} address {device.Ipaddress} is either invalid or can't be reached")
        {
            SignalController = device;
        }

        public Device SignalController { get; private set; }
    }

    public abstract class ControllerLoggerException : ATSPMException
    {
        public ControllerLoggerException(string? message) : base(message) { }

        public ControllerLoggerException(string? message, Exception? innerException) : base(message, innerException) { }
    }

    #region LocationControllerLoggerExceptions

    public class ControllerLoggerExecutionException : ControllerLoggerException
    {
        public ControllerLoggerExecutionException(ILocationControllerLoggerService LocationControllerLoggerService, string? message, Exception? innerException) 
            : base(message ?? $"Exception running Location Controller Logger Service", 
                  innerException)
        {
            LocationControllerLoggerService = LocationControllerLoggerService;
        }

        public ILocationControllerLoggerService LocationControllerLoggerService { get; private set; }
    }

    public class ControllerLoggerStepExecutionException<T> : ControllerLoggerExecutionException
    {
        public ControllerLoggerStepExecutionException(ILocationControllerLoggerService LocationControllerLoggerService,
            string step, 
            T item,
            string? message, Exception? innerException) : base(LocationControllerLoggerService,
                message ?? $"Exception running Location Controller Logger Service Step {step} on item {item}", 
                innerException)
        {
            Step = step;
            Item = item;
        }

        public string Step { get; private set; }
        public T Item { get; private set; }
    }

    #endregion

    #region ControllerLoggerDownloaderExceptions

    public abstract class ControllerLoggerDownloaderException : ControllerLoggerException
    {
        public ControllerLoggerDownloaderException(IDownloaderClient downloaderClient, string? message) : base(message)
        {
            //Location = Location;
            DownloaderClient = downloaderClient;
        }

        public ControllerLoggerDownloaderException(IDownloaderClient downloaderClient, string? message, Exception? innerException) : base(message, innerException)
        {
            //Location = Location;
            DownloaderClient = downloaderClient;
        }

        //public Location Location { get; private set; }

        public IDownloaderClient DownloaderClient { get; private set; }
    }

    public class ControllerConnectionException : ControllerLoggerDownloaderException
    {
        public ControllerConnectionException(string host, IDownloaderClient downloaderClient, string? message) : base(downloaderClient, message)
        {
            Host = host;
        }

        public ControllerConnectionException(string host, IDownloaderClient downloaderClient, string? message, Exception? innerException) : base(downloaderClient, message, innerException)
        {
            Host = host;
        }

        public string Host { get; private set; }
    }

    public class ControllerDownloadFileException : ControllerLoggerDownloaderException
    {
        public ControllerDownloadFileException(string fileName, IDownloaderClient downloaderClient, string? message) : base(downloaderClient, message)
        {
            FileName = fileName;
        }

        public ControllerDownloadFileException(string fileName, IDownloaderClient downloaderClient, string? message, Exception? innerException) : base(downloaderClient, message, innerException)
        {
            FileName = fileName;
        }

        public string FileName { get; private set; }
    }

    public class ControllerDeleteFileException : ControllerLoggerDownloaderException
    {
        public ControllerDeleteFileException(string fileName, IDownloaderClient downloaderClient, string? message) : base(downloaderClient, message)
        {
            FileName = fileName;
        }

        public ControllerDeleteFileException(string fileName, IDownloaderClient downloaderClient, string? message, Exception? innerException) : base(downloaderClient, message, innerException)
        {
            FileName = fileName;
        }

        public string FileName { get; private set; }
    }

    public class ControllerListDirectoryException : ControllerLoggerDownloaderException
    {
        public ControllerListDirectoryException(string directory, IDownloaderClient downloaderClient, string? message) : base(downloaderClient, message)
        {
            Directory = directory;
        }

        public ControllerListDirectoryException(string directory, IDownloaderClient downloaderClient, string? message, Exception? innerException) : base(downloaderClient, message, innerException)
        {
            Directory = directory;
        }

        public string Directory { get; private set; }
    }

    #endregion

    #region ControllerLoggerDecoderExceptions

    public class ControllerLoggerDecoderException : ControllerLoggerException
    {
        public ControllerLoggerDecoderException(string? message) : base(message) { }

        public ControllerLoggerDecoderException(string? message, Exception? innerException) : base(message, innerException) { }
    }

    #endregion
}
