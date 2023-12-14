using ATSPM.Application.Common;
using ATSPM.Application.Common.EqualityComparers;
using ATSPM.Application.Configuration;
using ATSPM.Application.Exceptions;
using ATSPM.Application.LogMessages;
using ATSPM.Application.Services.LocationControllerProtocols;
using ATSPM.Data.Models;
using ATSPM.Domain.BaseClasses;
using ATSPM.Domain.Common;
using ATSPM.Domain.Exceptions;
using ATSPM.Domain.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ATSPM.Infrastructure.Services.ControllerDecoders
{
    public abstract class ControllerDecoderBase : ServiceObjectBase, ILocationControllerDecoder
    {
        public event EventHandler CanExecuteChanged;

        #region Fields

        private readonly ILogger _log;
        //protected readonly IOptions<SignalControllerDecoderConfiguration> _options;
        protected readonly SignalControllerDecoderConfiguration _options;

        #endregion

        public ControllerDecoderBase(ILogger log, IOptionsSnapshot<SignalControllerDecoderConfiguration> options)
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

        private bool IsAcceptableDateRange(ControllerEventLog log)
        {
            return log.Timestamp <= DateTime.Now && log.Timestamp > _options.EarliestAcceptableDate;
        }

        public abstract bool CanExecute(FileInfo parameter);

        public Task<HashSet<ControllerEventLog>> ExecuteAsync(FileInfo parameter, CancellationToken cancelToken = default)
        {
            return ExecuteAsync(parameter, default, cancelToken);
        }

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="ExecuteException"></exception>
        public async Task<HashSet<ControllerEventLog>> ExecuteAsync(FileInfo parameter, IProgress<ControllerDecodeProgress> progress = null, CancellationToken cancelToken = default)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter), $"FileInfo parameter can not be null");

            if (!parameter.Exists)
                throw new FileNotFoundException($"File not found {parameter.FullName}", parameter.FullName);

            if (CanExecute(parameter))
            {
                var logMessages = new ControllerLoggerDecoderLogMessages(_log, parameter);

                HashSet<ControllerEventLog> decodedLogs = new HashSet<ControllerEventLog>(new ControllerEventLogEqualityComparer());

                var memoryStream = parameter.ToMemoryStream();

                memoryStream = IsCompressed(memoryStream) ? (MemoryStream)Decompress(memoryStream) : memoryStream;

                try
                {
                    logMessages.DecodeLogFileMessage(parameter.FullName);

                    await foreach (var log in DecodeAsync(parameter.Directory.Name, memoryStream, cancelToken))
                    {
                        if (IsAcceptableDateRange(log))
                        {
                            decodedLogs.Add(log);

                            progress?.Report(new ControllerDecodeProgress(log, decodedLogs.Count - 1, decodedLogs.Count));
                        }                     
                    }

                    if (_options.DeleteFile)
                        parameter.Delete();
                }
                catch (ControllerLoggerDecoderException e)
                {
                    logMessages.DecodeLogFileException(parameter.FullName, e);
                }

                logMessages.DecodedLogsMessage(parameter.FullName, decodedLogs.Count);

                return decodedLogs;
            }
            else
            {
                throw new ExecuteException();
            }
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
        public abstract IAsyncEnumerable<ControllerEventLog> DecodeAsync(string locationId, Stream stream, CancellationToken cancelToken = default);

        #endregion
    }
}
