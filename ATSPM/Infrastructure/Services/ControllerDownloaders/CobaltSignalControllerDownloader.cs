using ATSPM.Application.Configuration;
using ATSPM.Application.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ATSPM.Infrastructure.Services.ControllerDownloaders
{
    public class CobaltLocationControllerDownloader : ControllerDownloaderBase
    {
        public CobaltLocationControllerDownloader(IFTPDownloaderClient client, ILogger<CobaltLocationControllerDownloader> log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> options) : base(client, log, options) { }
    }
}
