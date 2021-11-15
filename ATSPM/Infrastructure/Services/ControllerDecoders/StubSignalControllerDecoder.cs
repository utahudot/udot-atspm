using ATSPM.Application.Common.EqualityComparers;
using ATSPM.Application.Configuration;
using ATSPM.Application.Enums;
using ATSPM.Application.Extensions;
using ATSPM.Application.Models;
using ATSPM.Application.Services.SignalControllerProtocols;
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
using System.Xml.Linq;

namespace ATSPM.Infrasturcture.Services.ControllerDecoders
{
    public class StubSignalControllerDecoder : ControllerDecoderBase
    {
        public StubSignalControllerDecoder(ILogger<StubSignalControllerDecoder> log, IServiceProvider serviceProvider, IOptions<SignalControllerDownloaderConfiguration> options) : base(log, serviceProvider, options) { }

        #region Properties

        public override SignalControllerType ControllerType => SignalControllerType.ASC3;

        #endregion

        #region Methods

        public override void Initialize()
        {
        }

        public override bool CanExecute(FileInfo parameter)
        {
            return parameter.Exists;
        }

        public override bool IsCompressed(Stream stream)
        {
            return base.IsCompressed(stream);
        }

        public override bool IsEncoded(Stream stream)
        {
            return base.IsEncoded(stream);
        }

        public override Stream Decompress(Stream stream)
        {
            return base.Decompress(stream);
        }

        public override Task<HashSet<ControllerEventLog>> DecodeAsync(string signalId, Stream stream, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            cancelToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(signalId))
                throw new ArgumentNullException(nameof(signalId));

            if (stream?.Length == 0)
                throw new InvalidDataException("Stream is empty");

            HashSet<ControllerEventLog> logList = new HashSet<ControllerEventLog>(new ControllerEventLogEqualityComparer());

            if (stream is MemoryStream memoryStream)
            {
                stream.Position = 0;

                var data = memoryStream.ToArray();

                var st = Encoding.Default.GetString(data);

                foreach (int item in st.Split(",").Select(int.Parse).ToList())
                {
                    var log = new ControllerEventLog()
                    {
                        SignalId = signalId,
                        EventCode = item,
                        EventParam = item,
                        Timestamp = DateTime.Now
                    };

                    logList.Add(log);

                    progress?.Report(logList.Count);
                }
            }

            progress?.Report(logList.Count);
            return Task.FromResult(logList);

            throw new InvalidDataException($"Decoding error, not a valid file or stream format {signalId}");
        }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}