using ATSPM.Application.Common;
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
using System.Runtime.CompilerServices;
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

        public StubSignalControllerDownloader(ILogger<StubSignalControllerDownloader> log, IServiceProvider serviceProvider, IOptionsSnapshot<SignalControllerDownloaderConfiguration> options) : base(log, serviceProvider, options) { }

        #region Properties

        public override SignalControllerType ControllerType => SignalControllerType.Unknown;

        #endregion

        #region Methods

        public override void Initialize()
        {
        }

        protected override async IAsyncEnumerable<FileInfo> ExecutionTask(Signal parameter, IProgress<ControllerDownloadProgress> progress = null, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            foreach (var i in Enumerable.Range(1, 10))
            {
                var file = new FileInfo(Path.Combine(_options.LocalPath, parameter.SignalId, $"File {i}.txt"));

                //if (!file.Exists)
                //{
                //    file.Create();
                //}

                await Task.Delay(TimeSpan.FromSeconds(2), cancelToken);

                progress?.Report(new ControllerDownloadProgress(file, i, 10));

                yield return file;
            }
        }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }

        //public override bool CanExecute(Signal value)
        //{
        //    return true;
        //}

        #endregion
    }
}
