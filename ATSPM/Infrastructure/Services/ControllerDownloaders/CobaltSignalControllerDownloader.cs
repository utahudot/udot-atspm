using ATSPM.Application.Configuration;
using ATSPM.Application.Services.LocationControllerProtocols;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ATSPM.Infrastructure.Services.ControllerDownloaders
{
    public class CobaltLocationControllerDownloader : ControllerDownloaderBase
    {
        public CobaltLocationControllerDownloader(IFTPDownloaderClient client, ILogger<CobaltLocationControllerDownloader> log, IOptionsSnapshot<LocationControllerDownloaderConfiguration> options) : base(client, log, options) { }

        #region Properties

        public override int ControllerType => 2;

        public override string[] FileFilters { get; set; } = new string[] { "dat", "datZ" };

        #endregion
    }
}
