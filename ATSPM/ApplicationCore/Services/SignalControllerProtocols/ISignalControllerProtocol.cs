using ControllerLogger.Application.Enums;
using ControllerLogger.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ControllerLogger.Application.Services
{
    public interface ISignalControllerProtocol
    {
        SignalControllerType ControllerType { get; }
        ISignalControllerDownloader Downloader { get; }
    }
}
