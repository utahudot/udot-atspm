using ATSPM.Application.Common;
using ATSPM.Application.Enums;
using ATSPM.Application.Models;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ATSPM.Application.Services.SignalControllerProtocols
{
    public interface ISignalControllerDecoder : IExecuteAsyncWithProgress<FileInfo, HashSet<ControllerEventLog>, ControllerDecodeProgress>
    {
        bool IsCompressed(Stream stream);

        bool IsEncoded(Stream stream);

        Stream Decompress(Stream stream);

        IAsyncEnumerable<ControllerEventLog> DecodeAsync(string signalId, Stream stream, CancellationToken cancelToken = default);
    }
}
