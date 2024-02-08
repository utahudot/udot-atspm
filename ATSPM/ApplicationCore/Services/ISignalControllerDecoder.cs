using ATSPM.Application.Common;
using ATSPM.Application.Exceptions;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using ATSPM.Domain.Common;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ATSPM.Application.Services
{
    public interface ILocationControllerDecoder : IExecutableServiceWithProgressAsync<FileInfo, EventLogModelBase, ControllerDecodeProgress>, IDisposable
    {
        bool IsCompressed(Stream stream);

        bool IsEncoded(Stream stream);

        Stream Decompress(Stream stream);

        /// <exception cref="ControllerLoggerDecoderException"></exception>
        HashSet<EventLogModelBase> Decode(string locationId, Stream stream);
    }
}
