using ATSPM.Application.Common;
using ATSPM.Application.Configuration;
using ATSPM.Application.Exceptions;
using ATSPM.Application.LogMessages;
using ATSPM.Application.Services;
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
    public abstract class ControllerDecoderBase : ExecutableServiceWithProgressAsyncBase<FileInfo, EventLogModelBase, ControllerDecodeProgress>, ILocationControllerDecoder
    {
        public event EventHandler CanExecuteChanged;

        #region Fields

        private readonly ILogger _log;
        //protected readonly IOptions<SignalControllerDecoderConfiguration> _options;
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

        private bool IsAcceptableDateRange(EventLogModelBase log)
        {
            return log.Timestamp <= DateTime.Now && log.Timestamp > _options.EarliestAcceptableDate;
        }

        public abstract bool CanExecute(FileInfo parameter);

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="ExecuteException"></exception>
        public override async IAsyncEnumerable<EventLogModelBase> Execute(FileInfo parameter, IProgress<ControllerDecodeProgress> progress = null, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter), $"FileInfo parameter can not be null");

            if (!parameter.Exists)
                throw new FileNotFoundException($"File not found {parameter.FullName}", parameter.FullName);

            if (CanExecute(parameter))
            {
                var logMessages = new ControllerLoggerDecoderLogMessages(_log, parameter);

                HashSet<EventLogModelBase> decodedLogs = new();

                var memoryStream = parameter.ToMemoryStream();

                memoryStream = IsCompressed(memoryStream) ? (MemoryStream)Decompress(memoryStream) : memoryStream;

                try
                {
                    logMessages.DecodeLogFileMessage(parameter.FullName);

                    decodedLogs = Decode(parameter.Directory.Name, memoryStream);

                    if (_options.DeleteFile)
                        parameter.Delete();
                }
                catch (ControllerLoggerDecoderException e)
                {
                    logMessages.DecodeLogFileException(parameter.FullName, e);
                }

                memoryStream.Dispose();

                logMessages.DecodedLogsMessage(parameter.FullName, decodedLogs.Count);

                foreach (var log in decodedLogs)
                {
                    if (IsAcceptableDateRange(log))
                    {
                        //TODO: add this back in
                        //progress?.Report(new ControllerDecodeProgress(log, decodedLogs.Count - 1, decodedLogs.Count));

                        yield return log;
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
        public abstract HashSet<EventLogModelBase> Decode(string locationId, Stream stream);

        #endregion
    }
}
