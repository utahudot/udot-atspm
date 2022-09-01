using ATSPM.Application.Configuration;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ATSPM.Infrasturcture.Services.ControllerDecoders
{
    public class StubSignalControllerDecoder : ControllerDecoderBase
    {
        public StubSignalControllerDecoder(ILogger<StubSignalControllerDecoder> log, IOptionsSnapshot<SignalControllerDecoderConfiguration> options) : base(log, options) { }

        #region Properties

        #endregion

        #region Methods

        public override bool CanExecute(FileInfo parameter)
        {
            return true;
        }

        public override IAsyncEnumerable<ControllerEventLog> DecodeAsync(string SignalId, Stream stream, CancellationToken cancelToken = default)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}