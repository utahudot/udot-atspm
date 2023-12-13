using ATSPM.Data.Models;
using ATSPM.Application.Services.LocationControllerProtocols;
using System;
using System.Collections.Generic;
using System.Text;
using ATSPM.Application.Services;
using System.Threading.Tasks.Dataflow;

#nullable enable
namespace ATSPM.Application.Exceptions
{
    public class InvalidLocationControllerIpAddressException : ATSPMException
    {
        public InvalidLocationControllerIpAddressException(Location Location) : base($"{Location.LocationIdentifier} address {Location.Ipaddress} is either invalid or can't be reached")
        {
            LocationController = Location;
        }

        public Location LocationController { get; private set; }
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
