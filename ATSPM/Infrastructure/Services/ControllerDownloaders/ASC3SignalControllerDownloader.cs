using ATSPM.Application.Configuration;
using ATSPM.Application.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ATSPM.Infrastructure.Services.ControllerDownloaders
{
    public class ASC3LocationControllerDownloader : ControllerDownloaderBase
    {
        public ASC3LocationControllerDownloader(IFTPDownloaderClient client, ILogger<ASC3LocationControllerDownloader> log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> options) : base(client, log, options) { }

        #region Properties

        public override int ControllerType => 1;

        public override string[] FileFilters { get; set; } = new string[] { "dat", "datZ" };

        #endregion
    }
}
