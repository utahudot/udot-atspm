using ATSPM.Application.Enums;
using ATSPM.Application.Models;
using ATSPM.Domain.Common;
using System.IO;

namespace ATSPM.Application.Services.SignalControllerProtocols
{
    public interface ISignalControllerDownloader : IExecuteAsync<Signal, DirectoryInfo>
    {
        SignalControllerType ControllerType { get; }
    }
}
