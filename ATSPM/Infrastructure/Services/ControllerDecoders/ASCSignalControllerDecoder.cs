using ATSPM.Application.Common.EqualityComparers;
using ATSPM.Application.Configuration;
using ATSPM.Application.Enums;
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

namespace ATSPM.Infrasturcture.Services.ControllerDecoders
{
    public class ASCSignalControllerDecoder : ServiceObjectBase, ISignalControllerDecoder
    {
        private readonly ILogger _log;
        private readonly IServiceProvider _serviceProvider;
        protected readonly IOptions<SignalControllerDownloaderConfiguration> _options;

        public ASCSignalControllerDecoder(ILogger<ASCSignalControllerDecoder> log, IServiceProvider serviceProvider, IOptions<SignalControllerDownloaderConfiguration> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);

        public override void Initialize()
        {
        }

        #region ISignalControllerDecoder

        public event EventHandler CanExecuteChanged;

        public SignalControllerType ControllerType => SignalControllerType.ASC3 | SignalControllerType.EOS | SignalControllerType.Cobalt;

        public bool CanExecute(FileInfo parameter)
        {
            return parameter.Exists && (parameter.Extension == ".dat" || parameter.Extension == ".datZ" || parameter.Extension == ".DAT");
        }

        public Task<HashSet<ControllerEventLog>> ExecuteAsync(FileInfo parameter, CancellationToken cancelToken = default)
        {
            return ExecuteAsync(parameter, cancelToken);
        }

        public Task<HashSet<ControllerEventLog>> ExecuteAsync(FileInfo parameter, CancellationToken cancelToken = default, IProgress<int> progress = null)
        {
            _log.LogDebug("Decoding {FileName}", parameter.FullName);

            if (cancelToken.IsCancellationRequested)
                return Task.FromCanceled<HashSet<ControllerEventLog>>(cancelToken);

            //TODO: integrate CancellationToken
            //TODO: write out detailed logs
            if (CanExecute(parameter))
            {
                //convert file to stream
                var memoryStream = parameter.ToMemoryStream();

                //check if stream is compressed
                memoryStream = IsCompressed(memoryStream) ? (MemoryStream)Decompress(memoryStream) : memoryStream;

                //decode stream
                try
                {
                    return Task.FromResult(Decode(parameter.Directory.Name, memoryStream, progress, cancelToken));
                }
                catch (OperationCanceledException)
                {
                    return Task.FromCanceled<HashSet<ControllerEventLog>>(cancelToken);
                }
            }

            return Task.FromResult(new HashSet<ControllerEventLog>());
        }

        Task IExecuteAsync.ExecuteAsync(object parameter)
        {
            if (parameter is FileInfo p)
                return Task.Run(() => ExecuteAsync(p, default, default));
            return default;
        }

        bool ICommand.CanExecute(object parameter)
        {
            if (parameter is FileInfo p)
                return CanExecute(p);
            return default;
        }

        void ICommand.Execute(object parameter)
        {
            if (parameter is FileInfo p)
                Task.Run(() => ExecuteAsync(p, default, default));
        }

        #endregion

        public bool IsCompressed(Stream stream)
        {
            return stream.IsCompressed();
        }

        public bool IsEncoded(Stream stream)
        {
            MemoryStream memoryStream = (MemoryStream)stream;
            var bytes = memoryStream.ToArray();

            //ASCII doesn't have anything above 0x80
            return bytes.Any(b => b >= 0x80);
        }

        //HACK: need to use extension methods and GetFileSignatureFromMagicHeader to get compression type
        public Stream Decompress(Stream stream)
        {
            // read past the first two bytes of the zlib header
            stream?.Seek(2, SeekOrigin.Begin);

            // decompress the file
            using (DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress))
            {
                // copy decompressed data into return stream
                MemoryStream returnStream = new MemoryStream();
                deflateStream.CopyTo(returnStream);
                returnStream.Position = 0;

                return returnStream;
            }
        }

        public HashSet<ControllerEventLog> Decode(string signalId, Stream stream, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            if (cancelToken.IsCancellationRequested)
                cancelToken.ThrowIfCancellationRequested();

            HashSet<ControllerEventLog> logList = new HashSet<ControllerEventLog>(new ControllerEventLogEqualityComparer());

            using (var br = new BinaryReader(stream, Encoding.ASCII))
            {
                br.BaseStream.Position = 0;

                if (br.BaseStream.Position + 20 <= br.BaseStream.Length && DateTime.TryParse(br.ReadChars(20), out DateTime startTime))
                {
                    //find  line feed characters, that should take us to the end of the header.
                    // First line break is after Version
                    // Second LF is after FileName
                    // Third LF is after Interseciton number, which isn't used as far as I can tell
                    // Fourth LF is after IP address
                    // Fifth is after MAC Address
                    // Sixth is after "Controller data log beginning:," and then the date
                    // Seven is after "Phases in use:," and then the list of phases, seperated by commas

                    var i = 0;

                    while (i < 7 && br.BaseStream.Position < br.BaseStream.Length)
                    {
                        var c = br.ReadChar();
                        if (c == '\n')
                            i++;
                    }

                    // after that, we start reading until we reach the end 
                    while (br.BaseStream.Position + sizeof(byte) * 4 <= br.BaseStream.Length)
                    {
                        var eventTime = new DateTime();
                        var eventCode = new int();
                        var eventParam = new int();

                        for (var eventPart = 1; eventPart < 4; eventPart++)
                        {
                            //getting the EventCode
                            if (eventPart == 1)
                                eventCode = Convert.ToInt32(br.ReadByte());

                            if (eventPart == 2)
                                eventParam = Convert.ToInt32(br.ReadByte());

                            //getting the time offset
                            if (eventPart == 3)
                            {
                                //var rawoffset = new byte[2];
                                var rawoffset = br.ReadBytes(2);
                                Array.Reverse(rawoffset);
                                int offset = BitConverter.ToInt16(rawoffset, 0);
                                var tenths = Convert.ToDouble(offset) / 10;
                                eventTime = startTime.AddSeconds(tenths);
                            }
                        }


                        //going to write to new ControllerLogEvent object
                        if (eventTime <= DateTime.Now && eventTime > _options.Value.EarliestAcceptableDate)
                        {
                            logList.Add(new ControllerEventLog() { SignalId = signalId, EventCode = eventCode, EventParam = eventParam, Timestamp = eventTime });

                            //report progress
                            //TODO: make a decoder progess object that tracks number of decoded vs number of added with current date and signalid
                            progress?.Report(logList.Count);

                            //_log.LogDebug("Decoded {ListCount} items from {SignalID}", logList.Count, signalId);
                        }
                    }

                    progress?.Report(logList.Count);
                    return logList;
                }
            }

            throw new InvalidDataException($"Decoding error, not a valid file or stream format {signalId}");
        }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}