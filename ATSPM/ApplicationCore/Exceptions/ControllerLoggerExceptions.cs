using ATSPM.Application.Models;
using ATSPM.Application.Services.SignalControllerProtocols;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATSPM.Application.Exceptions
{
    public abstract class ControllerLoggerExceptions : ATSPMExceptionBase
    {
        public ControllerLoggerExceptions(IDownloaderClient downloaderClient, string? message) : base(message)
        {
            //Signal = signal;
            DownloaderClient = downloaderClient;
        }

        public ControllerLoggerExceptions(IDownloaderClient downloaderClient, string? message, Exception? innerException) : base(message, innerException) 
        {
            //Signal = signal;
            DownloaderClient = downloaderClient;
        }

        //public Signal Signal { get; private set; }

        public IDownloaderClient DownloaderClient { get; private set; }
    }

    public class ControllerConnectionException : ControllerLoggerExceptions
    {
        public ControllerConnectionException(string host, IDownloaderClient downloaderClient, string? message) : base(downloaderClient, message)
        {
            Host = host;
        }

        public ControllerConnectionException(string host, IDownloaderClient downloaderClient, string? message, Exception? innerException) : base( downloaderClient, message, innerException)
        {
            Host = host;
        }

        public string Host { get; private set; }
    }

    public class ControllerDownloadFileException : ControllerLoggerExceptions
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

    public class ControllerDeleteFileException : ControllerLoggerExceptions
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
}
