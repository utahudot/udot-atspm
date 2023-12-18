using ATSPM.Application.Configuration;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ATSPM.Infrastructure.Services.ControllerDecoders
{
    public class StubLocationControllerDecoder : ControllerDecoderBase
    {
        public StubLocationControllerDecoder(ILogger<StubLocationControllerDecoder> log, IOptionsSnapshot<SignalControllerDecoderConfiguration> options) : base(log, options) { }

        #region Properties

        #endregion

        #region Methods

        public override bool CanExecute(FileInfo parameter)
        {
            return true;
        }

        public override IAsyncEnumerable<ControllerEventLog> DecodeAsync(string locationId, Stream stream, CancellationToken cancelToken = default)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}