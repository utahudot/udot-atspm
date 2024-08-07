#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Services.EventLogDecoders/AscToIndianaDecoder.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using ATSPM.Application.Exceptions;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;

namespace ATSPM.Infrastructure.Services.EventLogDecoders
{
    /// <inheritdoc/>
    public class AscToIndianaDecoder : EventLogDecoderBase<IndianaEvent>
    {
        /// <inheritdoc/>
        //public ASCEventLogDecoder(ILogger<ASCEventLogDecoder> log, IOptionsSnapshot<SignalControllerDecoderConfiguration> options) : base(log, options) { }

        #region Properties

        #endregion

        #region Methods

        /// <inheritdoc/>
        //public override bool CanExecute(Tuple<Device, FileInfo> parameter)
        //{
        //    var device = parameter.Item1;
        //    var file = parameter.Item2;

        //    return base.CanExecute(parameter) && file.Exists && (file.Extension == ".dat" || file.Extension == ".datZ" || file.Extension == ".DAT");
        //}

        //HACK: need to use extension methods and GetFileSignatureFromMagicHeader to get compression type
        /// <inheritdoc/>
        public override Stream Decompress(Stream stream)
        {
            stream?.Seek(2, SeekOrigin.Begin);

            using (DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress))
            {
                MemoryStream returnStream = new MemoryStream();
                deflateStream.CopyTo(returnStream);
                returnStream.Position = 0;

                return returnStream;
            }
        }

        /// <inheritdoc/>
        public override IEnumerable<IndianaEvent> Decode(Device device, Stream stream, CancellationToken cancelToken = default)
        {
            cancelToken.ThrowIfCancellationRequested();

            if (device == null)
                throw new ArgumentNullException(nameof(device), "Device can not be null");

            if (stream?.Length == 0)
                throw new InvalidDataException("Stream is empty");

            var locationIdentifider = device.Location.LocationIdentifier;

            HashSet<IndianaEvent> decodedLogs = new();

            try
            {
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
                            cancelToken.ThrowIfCancellationRequested();

                            var c = br.ReadChar();
                            if (c == '\n')
                                i++;
                        }

                        // after that, we start reading until we reach the end 
                        while (br.BaseStream.Position + sizeof(byte) * 4 <= br.BaseStream.Length)
                        {
                            cancelToken.ThrowIfCancellationRequested();

                            var log = new IndianaEvent() { LocationIdentifier = locationIdentifider };

                            for (var eventPart = 1; eventPart < 4; eventPart++)
                            {
                                //getting the EventCode
                                if (eventPart == 1)
                                    log.EventCode = Convert.ToInt16(br.ReadByte());

                                if (eventPart == 2)
                                    log.EventParam = br.ReadByte();

                                //getting the time offset
                                if (eventPart == 3)
                                {
                                    //var rawoffset = new byte[2];
                                    var rawoffset = br.ReadBytes(2);
                                    Array.Reverse(rawoffset);
                                    int offset = BitConverter.ToInt16(rawoffset, 0);
                                    var tenths = Convert.ToDouble(offset) / 10;
                                    log.Timestamp = startTime.AddSeconds(tenths);
                                }
                            }

                            decodedLogs.Add(log);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new EventLogDecoderException(e);
            }

            return decodedLogs;
        }

        #endregion
    }
}