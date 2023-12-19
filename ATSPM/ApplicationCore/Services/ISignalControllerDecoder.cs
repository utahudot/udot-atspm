using ATSPM.Application.Common;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ATSPM.Application.Services
{
    public interface ILocationControllerDecoder : IExecuteAsyncWithProgress<FileInfo, HashSet<ControllerEventLog>, ControllerDecodeProgress>, IDisposable
    {
        bool IsCompressed(Stream stream);

        bool IsEncoded(Stream stream);

        Stream Decompress(Stream stream);

        /// <exception cref="ControllerLoggerDecoderException"></exception>
        IAsyncEnumerable<ControllerEventLog> DecodeAsync(string locationId, Stream stream, CancellationToken cancelToken = default);
    }
}
