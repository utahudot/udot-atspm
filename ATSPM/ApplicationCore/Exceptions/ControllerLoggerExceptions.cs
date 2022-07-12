using ATSPM.Application.Models;
using ATSPM.Application.Services.SignalControllerProtocols;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATSPM.Application.Exceptions
{
    public class InvalidSignalControllerIpAddressException : ATSPMException
    {
        public InvalidSignalControllerIpAddressException(Signal signal) : base($"{signal.SignalId} address {signal.Ipaddress} is either invalid or can't be reached")
        {
            SignalController = signal;
        }

        public Signal SignalController { get; private set; }
    }

    public abstract class ControllerLoggerException : ATSPMException
    {
        public ControllerLoggerException(string? message) : base(message) { }

        public ControllerLoggerException(string? message, Exception? innerException) : base(message, innerException) { }
    }

    #region ControllerLoggerDownloaderExceptions

    public abstract class ControllerLoggerDownloaderException : ControllerLoggerException
    {
        public ControllerLoggerDownloaderException(IDownloaderClient downloaderClient, string? message) : base(message)
        {
            //Signal = signal;
            DownloaderClient = downloaderClient;
        }

        public ControllerLoggerDownloaderException(IDownloaderClient downloaderClient, string? message, Exception? innerException) : base(message, innerException)
        {
            //Signal = signal;
            DownloaderClient = downloaderClient;
        }

        //public Signal Signal { get; private set; }

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
