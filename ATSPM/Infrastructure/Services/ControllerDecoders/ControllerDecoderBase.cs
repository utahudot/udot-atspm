#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Services.ControllerDecoders/ControllerDecoderBase.cs
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
using ATSPM.Application.Common;
using ATSPM.Application.Configuration;
using ATSPM.Application.Exceptions;
using ATSPM.Application.LogMessages;
using ATSPM.Application.Services;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using ATSPM.Domain.BaseClasses;
using ATSPM.Domain.Exceptions;
using ATSPM.Domain.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ATSPM.Infrastructure.Services.ControllerDecoders
{
    public abstract class ControllerDecoderBase<T> : ExecutableServiceWithProgressAsyncBase<Tuple<Device, FileInfo>, Tuple<Device, T>, ControllerDecodeProgress>, ILocationControllerDecoder<T> where T : EventLogModelBase
    {
        public event EventHandler CanExecuteChanged;

        #region Fields

        private readonly ILogger _log;
        protected readonly SignalControllerDecoderConfiguration _options;

        #endregion

        public ControllerDecoderBase(ILogger log, IOptionsSnapshot<SignalControllerDecoderConfiguration> options) : base(true)
        {
            _log = log;
            _options = options?.Get(this.GetType().Name) ?? options?.Value;
        }

        #region Properties

        #endregion

        #region Methods

        //public override void Initialize()
        //{
        //}

        private bool IsAcceptableDateRange(T log)
        {
            return log.Timestamp <= DateTime.Now && log.Timestamp > _options.EarliestAcceptableDate;
        }

        public abstract bool CanExecute(Tuple<Device, FileInfo> parameter);

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="ExecuteException"></exception>
        public override async IAsyncEnumerable<Tuple<Device, T>> Execute(Tuple<Device, FileInfo> parameter, IProgress<ControllerDecodeProgress> progress = null, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            var device = parameter.Item1;
            var file = parameter.Item2;
            
            if (file == null)
                throw new ArgumentNullException(nameof(file), $"FileInfo file can not be null");

            if (!file.Exists)
                throw new FileNotFoundException($"File not found {file.FullName}", file.FullName);

            if (CanExecute(parameter))
            {
                var logMessages = new ControllerLoggerDecoderLogMessages(_log, file);

                HashSet<T> decodedLogs = new();

                var memoryStream = file.ToMemoryStream();

                memoryStream = IsCompressed(memoryStream) ? (MemoryStream)Decompress(memoryStream) : memoryStream;

                try
                {
                    logMessages.DecodeLogFileMessage(file.FullName);

                    decodedLogs = new HashSet<T>(Decode(device, memoryStream));

                    if (_options.DeleteFile)
                        file.Delete();
                }
                catch (ControllerLoggerDecoderException e)
                {
                    logMessages.DecodeLogFileException(file.FullName, e);
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
            else
            {
                throw new ExecuteException();
            }
        }

        public virtual bool IsCompressed(Stream stream)
        {
            return stream.IsCompressed();
        }

        public virtual bool IsEncoded(Stream stream)
        {
            MemoryStream memoryStream = (MemoryStream)stream;
            var bytes = memoryStream.ToArray();

            //ASCII doesn't have anything above 0x80
            return bytes.Any(b => b >= 0x80);
        }

        public virtual Stream Decompress(Stream stream)
        {
            return stream.GZipDecompressToStream();
        }

        /// <exception cref="ControllerLoggerDecoderException"></exception>
        public abstract IEnumerable<T> Decode(Device device, Stream stream);

        #endregion
    }
}
