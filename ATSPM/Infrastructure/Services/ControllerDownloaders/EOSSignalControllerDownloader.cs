using ATSPM.Application.Configuration;
using ATSPM.Application.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ATSPM.Infrastructure.Services.ControllerDownloaders
{
    public class EOSSignalControllerDownloader : ControllerDownloaderBase
    {
        public EOSSignalControllerDownloader(IFTPDownloaderClient client, ILogger<EOSSignalControllerDownloader> log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> options) : base(client, log, options) { }
    }
}
