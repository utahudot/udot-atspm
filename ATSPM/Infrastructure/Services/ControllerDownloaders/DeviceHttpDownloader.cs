using ATSPM.Application.Configuration;
using ATSPM.Application.Services;
using ATSPM.Data.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ATSPM.Infrastructure.Services.ControllerDownloaders
{
    ///<inheritdoc/>
    public class DeviceHttpDownloader : DeviceDownloaderBase
    {
        ///<inheritdoc/>
        public DeviceHttpDownloader(IHTTPDownloaderClient client, ILogger<DeviceHttpDownloader> log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> options) : base(client, log, options) { }

        ///<inheritdoc/>
        public override TransportProtocols Protocol => TransportProtocols.Http;
    }
}
