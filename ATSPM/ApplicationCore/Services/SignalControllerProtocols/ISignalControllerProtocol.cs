using ATSPM.Application.Enums;

namespace ATSPM.Application.Services.SignalControllerProtocols
{
    public interface ISignalControllerProtocol
    {
        SignalControllerType ControllerType { get; }
        ISignalControllerDownloader Downloader { get; }
    }
}
