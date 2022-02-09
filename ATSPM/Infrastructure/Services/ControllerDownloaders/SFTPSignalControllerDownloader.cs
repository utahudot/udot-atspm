using ATSPM.Application.Configuration;
using ATSPM.Application.Enums;
using ATSPM.Application.Extensions;
using ATSPM.Application.Models;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Domain.BaseClasses;
using ATSPM.Domain.Common;
using ATSPM.Domain.Exceptions;
using ATSPM.Domain.Extensions;
using FluentFTP;
using FluentFTP.Rules;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Utah.Gov.Udot.PipelineManager;

namespace ATSPM.Infrasturcture.Services.ControllerDownloaders
{
    public class SFTPSignalControllerDownloader : ControllerDownloaderBase
    {

        public SFTPSignalControllerDownloader(ILogger<SFTPSignalControllerDownloader> log, IServiceProvider serviceProvider, IOptions<SignalControllerDownloaderConfiguration> options) : base(log, serviceProvider, options) { }

        #region Properties

        public override SignalControllerType ControllerType => SignalControllerType.Cobalt;

        #endregion

        #region Methods

        protected override async Task<DirectoryInfo> ExecutionTask(Signal parameter, CancellationToken cancelToken = default, IProgress<int> progress = null)
        {
            //return directory
            DirectoryInfo dir = null;

            

            return dir;
        }

        

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}
