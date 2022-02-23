using ATSPM.Application.Common;
using ATSPM.Application.Configuration;
using ATSPM.Application.Enums;
using ATSPM.Application.Models;
using ATSPM.Application.Services.SignalControllerProtocols;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ATSPM.Infrasturcture.Services.ControllerDownloaders
{
    public class SFTPSignalControllerDownloader : ControllerDownloaderBase
    {
        public SFTPSignalControllerDownloader(ISFTPDownloaderClient client, ILogger<SFTPSignalControllerDownloader> log, IOptionsSnapshot<SignalControllerDownloaderConfiguration> options) : base(client, log, options) { }

        #region Properties

        public override int ControllerType => 2;

        public override string[] FileFilters { get; set; } = new string[] { "dat", "datZ" };

        #endregion
    }
}
