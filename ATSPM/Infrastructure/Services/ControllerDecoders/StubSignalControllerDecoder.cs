using ATSPM.Application.Common.EqualityComparers;
using ATSPM.Application.Configuration;
using ATSPM.Application.Enums;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Application.ValueObjects;
using ATSPM.Domain.BaseClasses;
using ATSPM.Domain.Common;
using ATSPM.Domain.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ATSPM.Infrasturcture.Services.ControllerDecoders
{
    public class StubSignalControllerDecoder : ControllerDecoderBase
    {
        public StubSignalControllerDecoder(ILogger<StubSignalControllerDecoder> log, IOptions<SignalControllerDecoderConfiguration> options) : base(log, options) { }

        #region Properties

        #endregion

        #region Methods

        public override bool CanExecute(FileInfo parameter)
        {
            return true;
        }

        public override IAsyncEnumerable<ControllerEventLog> DecodeAsync(string signalId, Stream stream, CancellationToken cancelToken = default)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}