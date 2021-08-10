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

        public SignalControllerType ControllerType => throw new NotImplementedException();

        public bool CanExecute(FileInfo parameter)
        {
            throw new NotImplementedException();
        }

        public Task<HashSet<ControllerEventLog>> ExecuteAsync(FileInfo parameter, CancellationToken cancelToken = default)
        {
            return ExecuteAsync(parameter, cancelToken);
        }

        public Task<HashSet<ControllerEventLog>> ExecuteAsync(FileInfo parameter, CancellationToken cancelToken = default, IProgress<int> progress = null)
        {
            HashSet<ControllerEventLog> logList = new HashSet<ControllerEventLog>(new ControllerEventLogEqualityComparer());

            var memoryStream = parameter.ToMemoryStream();

            // set the memory position to beggining
            memoryStream.Position = 0;

            //check to see if data is compressed
            if (memoryStream.IsCompressed())
            {
                // ok it is compressed, let's decompress it before we procced
                memoryStream = memoryStream.GZipDecompressToStream();
            }

            using (var br = new BinaryReader(memoryStream, Encoding.ASCII))
            {
                if (br.BaseStream.Position + 21 < br.BaseStream.Length)
                {
                    //Find the start Date
                    var dateString = "";
                    for (var i = 1; i < 21; i++)
                    {
                        var c = br.ReadChar();
                        dateString += c;
                    }

                    var startTime = new DateTime();
                    if (DateTime.TryParse(dateString, out startTime) && br.BaseStream.Position < br.BaseStream.Length)
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
                                logList.Add(new ControllerEventLog() { SignalId = parameter.DirectoryName, EventCode = eventCode, EventParam = eventParam, Timestamp = eventTime });
                            }
                        }
                    }
                    //this is what we do when the datestring doesn't parse
                }
                //this is what we do when the datestring doesn't parse
            }

            return logList.AsTask();
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



        //TODO: replace file name as string with FileInfo type
        //public static IEnumerable<ControllerEventLog> DecodeAsc3File(FileInfo file, Signal signal, DateTime earliestAcceptableDate)
        //{
        //    HashSet<ControllerEventLog> logList = new HashSet<ControllerEventLog>(new ControllerEventLogEqualityComparer());

        //    var memoryStream = file.ToMemoryStream();

        //    // set the memory position to beggining
        //    memoryStream.Position = 0;

        //    //check to see if data is compressed
        //    if (memoryStream.IsCompressed())
        //    {
        //        // ok it is compressed, let's decompress it before we procced
        //        memoryStream = memoryStream.GZipDecompressToStream();
        //    }

        //    using (var br = new BinaryReader(memoryStream, Encoding.ASCII))
        //    {
        //        if (br.BaseStream.Position + 21 < br.BaseStream.Length)
        //        {
        //            //Find the start Date
        //            var dateString = "";
        //            for (var i = 1; i < 21; i++)
        //            {
        //                var c = br.ReadChar();
        //                dateString += c;
        //            }

        //            var startTime = new DateTime();
        //            if (DateTime.TryParse(dateString, out startTime) && br.BaseStream.Position < br.BaseStream.Length)
        //            {
        //                //find  line feed characters, that should take us to the end of the header.
        //                // First line break is after Version
        //                // Second LF is after FileName
        //                // Third LF is after Interseciton number, which isn't used as far as I can tell
        //                // Fourth LF is after IP address
        //                // Fifth is after MAC Address
        //                // Sixth is after "Controller data log beginning:," and then the date
        //                // Seven is after "Phases in use:," and then the list of phases, seperated by commas

        //                var i = 0;

        //                while (i < 7 && br.BaseStream.Position < br.BaseStream.Length)
        //                {
        //                    var c = br.ReadChar();
        //                    if (c == '\n')
        //                        i++;
        //                }

        //                // after that, we start reading until we reach the end 
        //                while (br.BaseStream.Position + sizeof(byte) * 4 <= br.BaseStream.Length)
        //                {
        //                    var eventTime = new DateTime();
        //                    var eventCode = new int();
        //                    var eventParam = new int();

        //                    for (var eventPart = 1; eventPart < 4; eventPart++)
        //                    {
        //                        //getting the EventCode
        //                        if (eventPart == 1)
        //                            eventCode = Convert.ToInt32(br.ReadByte());

        //                        if (eventPart == 2)
        //                            eventParam = Convert.ToInt32(br.ReadByte());

        //                        //getting the time offset
        //                        if (eventPart == 3)
        //                        {
        //                            //var rawoffset = new byte[2];
        //                            var rawoffset = br.ReadBytes(2);
        //                            Array.Reverse(rawoffset);
        //                            int offset = BitConverter.ToInt16(rawoffset, 0);
        //                            var tenths = Convert.ToDouble(offset) / 10;
        //                            eventTime = startTime.AddSeconds(tenths);
        //                        }
        //                    }


        //                    //going to write to new ControllerLogEvent object
        //                    if (eventTime <= DateTime.Now && eventTime > earliestAcceptableDate)
        //                    {
        //                        logList.Add(new ControllerEventLog() { SignalId = signal.SignalId, EventCode = eventCode, EventParam = eventParam, Timestamp = eventTime });
        //                    }
        //                }
        //            }
        //            //this is what we do when the datestring doesn't parse
        //        }
        //        //this is what we do when the datestring doesn't parse
        //    }

        //    return logList;
        //}

        //public static MemoryStream FileToMemoryStream(string fileName)
        //{
        //    // load the file into memory stream
        //    var fileStream = File.Open(fileName, FileMode.Open);
        //    var memoryStream = new MemoryStream();
        //    fileStream.CopyTo(memoryStream);
        //    fileStream.Close();

        //    return memoryStream;
        //}

        /// <summary>
        /// Determines if filestream passed in contains a hex signature of one of the known compression algorithms
        /// </summary>
        /// <param name="fileStream"></param>
        /// <returns>true or false if compression is detected</returns>
        //public static bool IsCompressed(MemoryStream fileStream)
        //{
        //    // read the magic header
        //    byte[] header = new byte[2];
        //    fileStream.Read(header, 0, 2);

        //    // let seek to back of file
        //    fileStream.Seek(0, SeekOrigin.Begin);

        //    // let's check for zlib compression
        //    if (AreEqual(header, _ZlibHeaderNoCompression) || AreEqual(header, _ZlibHeaderDefaultCompression) || AreEqual(header, _ZlibHeaderBestCompression))
        //    {
        //        return true;
        //    }
        //    // let's make sure it is not another compression 
        //    else if (AreEqual(header, _GZipHeader) || AreEqual(header, _EOSHeader))
        //    {
        //        return true;
        //    }

        //    return false;
        //}

        /// <summary>
        /// Decompresses the passed in filestream using deflatestream
        /// </summary>
        /// <param name="fileStream"></param>
        /// <returns>decompressed filestream</returns>
        //public static MemoryStream DecompessedStream(MemoryStream fileStream)
        //{
        //    // read past the first two bytes of the zlib header
        //    fileStream.Seek(2, SeekOrigin.Begin);

        //    // decompress the file
        //    using (DeflateStream deflateStream = new DeflateStream(fileStream, CompressionMode.Decompress))
        //    {
        //        // copy decompressed data into return stream
        //        MemoryStream returnStream = new MemoryStream();
        //        deflateStream.CopyTo(returnStream);
        //        returnStream.Position = 0;

        //        return returnStream;
        //    }
        //}


        /// <summary>
        /// Compares if two byte arrays are equal
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="b1"></param>
        /// <returns>returns true if they are equal and false if they are not equal</returns>
        //private static bool AreEqual(byte[] a1, byte[] b1)
        //{
        //    if (a1.Length != b1.Length)
        //    {
        //        return false;
        //    }

        //    for (int i = 0; i < a1.Length; i++)
        //    {
        //        if (a1[i] != b1[i])
        //        {
        //            return false;
        //        }
        //    }
        //    return true;
        //}

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}