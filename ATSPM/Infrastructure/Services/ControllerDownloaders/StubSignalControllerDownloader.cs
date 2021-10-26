using ATSPM.Application.Configuration;
using ATSPM.Application.Enums;
using ATSPM.Application.Extensions;
using ATSPM.Application.Models;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Domain.BaseClasses;
using ATSPM.Domain.Common;
using ATSPM.Domain.Exceptions;
using ATSPM.Domain.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ATSPM.Infrasturcture.Services.ControllerDownloaders
{
    public class StubSignalControllerDownloader : ControllerDownloaderBase
    {

        public StubSignalControllerDownloader(ILogger<StubSignalControllerDownloader> log, IServiceProvider serviceProvider, IOptions<SignalControllerDownloaderConfiguration> options) : base(log, serviceProvider, options) { }

        #region Properties

        public override SignalControllerType ControllerType => SignalControllerType.MaxTime;

        #endregion

        #region Methods

        public override void Initialize()
        {
        }

        protected override Task<DirectoryInfo> ExecutionTask(Signal parameter, CancellationToken cancelToken = default, IProgress<int> progress = null)
        {
            //return directory
            DirectoryInfo dir = null;

            return Task.FromResult<DirectoryInfo>(dir);
        }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}
