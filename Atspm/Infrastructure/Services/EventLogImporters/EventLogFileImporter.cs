﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Services.EventLogImporters/EventLogFileImporter.cs
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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using Utah.Udot.Atspm.Common;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Infrastructure.Services.EventLogImporters
{
    ///<inheritdoc cref="IEventLogImporter"/>
    public class EventLogFileImporter : ExecutableServiceWithProgressAsyncBase<Tuple<Device, FileInfo>, Tuple<Device, EventLogModelBase>, ControllerDecodeProgress>, IEventLogImporter
    {
        #region Fields

        private readonly IEnumerable<IEventLogDecoder> _decoders;
        protected readonly ILogger _log;
        protected readonly EventLogImporterConfiguration _options;

        #endregion

        ///<inheritdoc cref="IEventLogImporter"/>
        public EventLogFileImporter(IEnumerable<IEventLogDecoder> decoders, ILogger<IEventLogImporter> log, IOptionsSnapshot<EventLogImporterConfiguration> options) : base(true)
        {
            _decoders = decoders;
            _log = log;
            _options = options?.Get(GetType().Name) ?? options?.Value;
        }

        #region Properties

        #endregion

        #region Methods

        //public override void Initialize()
        //{
        //}

        private bool IsAcceptableDateRange(EventLogModelBase log)
        {
            return log.Timestamp <= DateTime.Now && log.Timestamp > _options.EarliestAcceptableDate;
        }

        /// <inheritdoc/>
        public override bool CanExecute(Tuple<Device, FileInfo> parameter)
        {
            return parameter != null && parameter.Item2.Exists;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="ExecuteException"></exception>
        public override async IAsyncEnumerable<Tuple<Device, EventLogModelBase>> Execute(Tuple<Device, FileInfo> parameter, IProgress<ControllerDecodeProgress> progress = null, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            var device = parameter.Item1;
            var file = parameter.Item2;
            var decoders = parameter.Item1.DeviceConfiguration.Decoders.ToList();

            if (file == null)
                throw new ArgumentNullException(nameof(file), $"FileInfo file can not be null");

            if (!file.Exists)
                throw new FileNotFoundException($"File not found {file.FullName}", file.FullName);

            if (CanExecute(parameter))
            {
                var logMessages = new EventLogDecoderLogMessages(_log, file);

                foreach (IEventLogDecoder decoder in _decoders.Where(w => decoders.Contains(w.GetType().Name)))
                {
                    List<EventLogModelBase> decodedLogs = new();

                    var memoryStream = file.ToMemoryStream();

                    memoryStream = decoder.IsCompressed(memoryStream) ? (MemoryStream)decoder.Decompress(memoryStream) : memoryStream;

                    try
                    {
                        logMessages.DecodeLogFileMessage(file.FullName);

                        decodedLogs = decoder.Decode(device, memoryStream, cancelToken).ToList();

                        if (_options.DeleteFile)
                            file.Delete();
                    }
                    catch (EventLogDecoderException e)
                    {
                        logMessages.DecodeLogFileException(file.FullName, e);
                    }
                    catch (OperationCanceledException e)
                    {
                        logMessages.OperationCancelledException(file.FullName, e);
                    }

                    memoryStream.Dispose();

                    logMessages.DecodedLogsMessage(file.FullName, decodedLogs.Count);

                    foreach (var log in decodedLogs)
                    {
                        if (IsAcceptableDateRange(log))
                        {
                            //TODO: add this back in
                            //progress?.Report(new ControllerDecodeProgress(log, decodedLogs.Count - 1, decodedLogs.Count));

                            yield return Tuple.Create(device, log);
                        }
                    }
                }
            }
            else
            {
                throw new ExecuteException();
            }
        }



        #endregion
    }

    /////<inheritdoc cref="IEventLogImporter{T}"/>
    //public abstract class EventLogFileImporter<T> : ExecutableServiceWithProgressAsyncBase<Tuple<Device, FileInfo>, Tuple<Device, T>, ControllerDecodeProgress>, IEventLogImporter<T> where T : EventLogModelBase
    //{
    //    #region Fields

    //    protected readonly ILogger _log;
    //    protected readonly SignalControllerDecoderConfiguration _options;

    //    #endregion

    //    ///<inheritdoc cref="IEventLogImporter{T}"/>
    //    public EventLogFileImporter(ILogger log, IOptionsSnapshot<SignalControllerDecoderConfiguration> options) : base(true)
    //    {
    //        _log = log;
    //        _options = options?.Get(this.GetType().Name) ?? options?.Value;
    //    }

    //    #region Properties

    //    #endregion

    //    #region Methods

    //    //public override void Initialize()
    //    //{
    //    //}

    //    private bool IsAcceptableDateRange(T log)
    //    {
    //        return log.Timestamp <= DateTime.Now && log.Timestamp > _options.EarliestAcceptableDate;
    //    }

    //    /// <inheritdoc/>
    //    public override bool CanExecute(Tuple<Device, FileInfo> parameter)
    //    {
    //        return parameter != null && parameter.Item1.DeviceConfiguration.Decoders == typeof(T) && parameter.Item2.Exists;
    //    }

    //    /// <inheritdoc/>
    //    /// <exception cref="ArgumentNullException"></exception>
    //    /// <exception cref="FileNotFoundException"></exception>
    //    /// <exception cref="ExecuteException"></exception>
    //    public override async IAsyncEnumerable<Tuple<Device, T>> Execute(Tuple<Device, FileInfo> parameter, IProgress<ControllerDecodeProgress> progress = null, [EnumeratorCancellation] CancellationToken cancelToken = default)
    //    {
    //        var device = parameter.Item1;
    //        var file = parameter.Item2;

    //        if (file == null)
    //            throw new ArgumentNullException(nameof(file), $"FileInfo file can not be null");

    //        if (!file.Exists)
    //            throw new FileNotFoundException($"File not found {file.FullName}", file.FullName);

    //        if (CanExecute(parameter))
    //        {
    //            var logMessages = new EventLogDecoderLogMessages(_log, file);

    //            HashSet<T> decodedLogs = new();

    //            var memoryStream = file.ToMemoryStream();

    //            memoryStream = IsCompressed(memoryStream) ? (MemoryStream)Decompress(memoryStream) : memoryStream;

    //            try
    //            {
    //                logMessages.DecodeLogFileMessage(file.FullName);

    //                decodedLogs = new HashSet<T>(Decode(device, memoryStream));

    //                if (_options.DeleteFile)
    //                    file.Delete();
    //            }
    //            catch (EventLogDecoderException e)
    //            {
    //                logMessages.DecodeLogFileException(file.FullName, e);
    //            }
    //            catch (OperationCanceledException e)
    //            {
    //                logMessages.OperationCancelledException(file.FullName, e);
    //            }

    //            memoryStream.Dispose();

    //            logMessages.DecodedLogsMessage(file.FullName, decodedLogs.Count);

    //            foreach (var log in decodedLogs)
    //            {
    //                if (IsAcceptableDateRange(log))
    //                {
    //                    //progress?.Report(new ControllerDecodeProgress(log, decodedLogs.Count - 1, decodedLogs.Count));

    //                    yield return Tuple.Create(device, log);
    //                }
    //            }
    //        }
    //        else
    //        {
    //            throw new ExecuteException();
    //        }
    //    }

    //    /// <inheritdoc/>
    //    public virtual bool IsCompressed(Stream stream)
    //    {
    //        return stream.IsCompressed();
    //    }

    //    /// <inheritdoc/>
    //    public virtual bool IsEncoded(Stream stream)
    //    {
    //        MemoryStream memoryStream = (MemoryStream)stream;
    //        var bytes = memoryStream.ToArray();

    //        //ASCII doesn't have anything above 0x80
    //        return bytes.Any(b => b >= 0x80);
    //    }

    //    /// <inheritdoc/>
    //    public virtual Stream Decompress(Stream stream)
    //    {
    //        return stream.GZipDecompressToStream();
    //    }

    //    /// <inheritdoc/>
    //    public abstract IEnumerable<T> Decode(Device device, Stream stream, CancellationToken cancelToken = default);

    //    #endregion
    //}
}