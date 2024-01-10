using ATSPM.Application.Configuration;
using ATSPM.Application.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ATSPM.Infrastructure.Services.ControllerDownloaders
{
    public class NewCobaltLocationControllerDownloader : ControllerDownloaderBase
    {
        public NewCobaltLocationControllerDownloader(ISFTPDownloaderClient client, ILogger<NewCobaltLocationControllerDownloader> log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> options) : base(client, log, options) { }
    }
}
