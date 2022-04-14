using ATSPM.Application.Common;
using ATSPM.Application.Common.EqualityComparers;
using ATSPM.Application.Configuration;
using ATSPM.Application.Enums;
using ATSPM.Application.Extensions;
using ATSPM.Application.Models;
using ATSPM.Application.Services.SignalControllerProtocols;
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ATSPM.Infrasturcture.Services.ControllerDecoders
{
    public abstract class ControllerDecoderBase : ServiceObjectBase, ISignalControllerDecoder
    {
        public event EventHandler CanExecuteChanged;

        #region Fields

        private readonly ILogger _log;
        protected readonly IOptions<SignalControllerDecoderConfiguration> _options;

        #endregion

        public ControllerDecoderBase(ILogger log, IOptions<SignalControllerDecoderConfiguration> options) => (_log, _options) = (log, options);

        #region Properties

        #endregion

        #region Methods

        //public override void Initialize()
        //{
        //}

        public abstract bool CanExecute(FileInfo parameter);

        public Task<HashSet<ControllerEventLog>> ExecuteAsync(FileInfo parameter, CancellationToken cancelToken = default)
        {
            return ExecuteAsync(parameter, cancelToken);
        }

        public async Task<HashSet<ControllerEventLog>> ExecuteAsync(FileInfo parameter, IProgress<ControllerDecodeProgress> progress = null, CancellationToken cancelToken = default)
        {
            if (parameter == null)
                return await Task.FromException<HashSet<ControllerEventLog>>(new ArgumentNullException(nameof(parameter), $"FileInfo parameter can not be null"));

            // TODO: find best exception to throw for null FileInfo
            if (!parameter.Exists)
                return await Task.FromException<HashSet<ControllerEventLog>>(new ArgumentNullException($"parameter"));

            HashSet<ControllerEventLog> decodedLogs = new HashSet<ControllerEventLog>(new ControllerEventLogEqualityComparer());

            if (CanExecute(parameter))
            {
                var memoryStream = parameter.ToMemoryStream();

                memoryStream = IsCompressed(memoryStream) ? (MemoryStream)Decompress(memoryStream) : memoryStream;

                try
                {
                    await foreach (var log in DecodeAsync(parameter.DirectoryName, memoryStream, cancelToken))
                    {
                        decodedLogs.Add(log);

                        progress?.Report(new ControllerDecodeProgress(log, decodedLogs.Count - 1, decodedLogs.Count));
                    }
                }
                catch (TaskCanceledException)
                {
                    return await Task.FromCanceled<HashSet<ControllerEventLog>>(cancelToken);
                }
                catch (Exception e)
                {
                    return await Task.FromException<HashSet<ControllerEventLog>>(e);
                }
            }
            else
            {
                return await Task.FromException<HashSet<ControllerEventLog>>(new ExecuteException());
            }

            return decodedLogs;
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

        public abstract IAsyncEnumerable<ControllerEventLog> DecodeAsync(string signalId, Stream stream, CancellationToken cancelToken = default);

        #endregion
    }
}
