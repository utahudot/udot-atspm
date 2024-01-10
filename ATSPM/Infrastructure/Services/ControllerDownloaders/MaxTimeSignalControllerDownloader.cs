using ATSPM.Application.Configuration;
using ATSPM.Application.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace ATSPM.Infrastructure.Services.ControllerDownloaders
{
    public class MaxTimeLocationControllerDownloader : ControllerDownloaderBase
    {
        public MaxTimeLocationControllerDownloader(IHTTPDownloaderClient client, ILogger<MaxTimeLocationControllerDownloader> log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> options) : base(client, log, options) { }
    }
}
