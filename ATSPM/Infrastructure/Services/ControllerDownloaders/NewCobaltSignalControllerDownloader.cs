using ATSPM.Application.Configuration;
using ATSPM.Application.Services.LocationControllerProtocols;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ATSPM.Infrastructure.Services.ControllerDownloaders
{
    public class NewCobaltLocationControllerDownloader : ControllerDownloaderBase
    {
        public NewCobaltLocationControllerDownloader(ISFTPDownloaderClient client, ILogger<NewCobaltLocationControllerDownloader> log, IOptionsSnapshot<LocationControllerDownloaderConfiguration> options) : base(client, log, options) { }

        #region Properties

        public override int ControllerType => 10;

        public override string[] FileFilters { get; set; } = new string[] { "dat", "datZ" };

        #endregion
    }
}
