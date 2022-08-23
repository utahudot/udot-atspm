using ATSPM.Application.Configuration;
using ATSPM.Application.Services.SignalControllerProtocols;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ATSPM.Infrasturcture.Services.ControllerDownloaders
{
    public class NewCobaltSignalControllerDownloader : ControllerDownloaderBase
    {
        public NewCobaltSignalControllerDownloader(ISFTPDownloaderClient client, ILogger<NewCobaltSignalControllerDownloader> log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> options) : base(client, log, options) { }

        #region Properties

        public override int ControllerType => 10;

        public override string[] FileFilters { get; set; } = new string[] { "dat", "datZ" };

        #endregion
    }
}
