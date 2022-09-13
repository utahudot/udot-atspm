using ATSPM.Application.Configuration;
using ATSPM.Application.Services.SignalControllerProtocols;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace ATSPM.Infrastructure.Services.ControllerDownloaders
{
    public class MaxTimeSignalControllerDownloader : ControllerDownloaderBase
    {
        public MaxTimeSignalControllerDownloader(IHTTPDownloaderClient client, ILogger<MaxTimeSignalControllerDownloader> log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> options) : base(client, log, options) { }

        #region Properties

        public override int ControllerType => 4;

        public override string[] FileFilters { get; set; } = new string[] { $"since={DateTime.Now.AddHours(-24):MM-dd-yyyy HH:mm:ss.f}" };

        #endregion
    }
}
