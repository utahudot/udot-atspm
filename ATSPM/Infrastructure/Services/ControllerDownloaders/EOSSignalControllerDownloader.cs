using ATSPM.Application.Configuration;
using ATSPM.Application.Services.LocationControllerProtocols;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ATSPM.Infrastructure.Services.ControllerDownloaders
{
    public class EOSLocationControllerDownloader : ControllerDownloaderBase
    {
        public EOSLocationControllerDownloader(IFTPDownloaderClient client, ILogger<EOSLocationControllerDownloader> log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> options) : base(client, log, options) { }

        #region Properties

        public override int ControllerType => 9;

        public override string[] FileFilters { get; set; } = new string[] { "dat", "datZ" };

        #endregion
    }
}
