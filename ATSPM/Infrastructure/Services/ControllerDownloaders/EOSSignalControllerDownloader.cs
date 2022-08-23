using ATSPM.Application.Configuration;
using ATSPM.Application.Services.SignalControllerProtocols;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ATSPM.Infrasturcture.Services.ControllerDownloaders
{
    public class EOSSignalControllerDownloader : ControllerDownloaderBase
    {
        public EOSSignalControllerDownloader(IFTPDownloaderClient client, ILogger<EOSSignalControllerDownloader> log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> options) : base(client, log, options) { }

        #region Properties

        public override int ControllerType => 9;

        public override string[] FileFilters { get; set; } = new string[] { "dat", "datZ" };

        #endregion
    }
}
