using ATSPM.Application.Configuration;
using ATSPM.Application.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ATSPM.Infrastructure.Services.ControllerDownloaders
{
    public class ASC3SignalControllerDownloader : ControllerDownloaderBase
    {
        public ASC3SignalControllerDownloader(IFTPDownloaderClient client, ILogger<ASC3SignalControllerDownloader> log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> options) : base(client, log, options) { }
    }
}
