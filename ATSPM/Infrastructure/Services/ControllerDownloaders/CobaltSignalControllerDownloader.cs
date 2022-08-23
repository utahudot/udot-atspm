using ATSPM.Application.Configuration;
using ATSPM.Application.Services.SignalControllerProtocols;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ATSPM.Infrasturcture.Services.ControllerDownloaders
{
    public class CobaltSignalControllerDownloader : ControllerDownloaderBase
    {
        public CobaltSignalControllerDownloader(IFTPDownloaderClient client, ILogger<CobaltSignalControllerDownloader> log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> options) : base(client, log, options) { }

        #region Properties

        public override int ControllerType => 2;

        public override string[] FileFilters { get; set; } = new string[] { "dat", "datZ" };

        #endregion
    }
}
