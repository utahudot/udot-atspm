using ControllerLogger.Application.Enums;
using ControllerLogger.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ControllerLogger.Application.Services
{
    public interface ISignalControllerDownloader : IPipelineExecute<Signal, DirectoryInfo>, IPipelineExecute
    {
        SignalControllerType ControllerType { get; }
    }
}
