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
    public interface ILocationControllerDecoder<T> : IExecutableServiceWithProgressAsync<Tuple<Device, FileInfo>, Tuple<Device, T>, ControllerDecodeProgress> where T : EventLogModelBase
    {
        bool IsCompressed(Stream stream);

        bool IsEncoded(Stream stream);

        Stream Decompress(Stream stream);

        /// <exception cref="ControllerLoggerDecoderException"></exception>
        IEnumerable<T> Decode(Device device, Stream stream);
    }
}
