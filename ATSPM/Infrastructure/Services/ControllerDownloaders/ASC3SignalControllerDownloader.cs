using ATSPM.Application.Configuration;
using ATSPM.Application.Services.SignalControllerProtocols;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ATSPM.Infrastructure.Services.ControllerDownloaders
{
    public class ASC3SignalControllerDownloader : ControllerDownloaderBase
    {
        public ASC3SignalControllerDownloader(IFTPDownloaderClient client, ILogger<ASC3SignalControllerDownloader> log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> options) : base(client, log, options) { }

        #region Properties

        public override int ControllerType => 1;

        public override string[] FileFilters { get; set; } = new string[] { "dat", "datZ" };

        #endregion
    }
}
