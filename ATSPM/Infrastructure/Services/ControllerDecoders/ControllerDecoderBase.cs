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
        private readonly IServiceProvider _serviceProvider;
        protected readonly IOptions<SignalControllerDownloaderConfiguration> _options;

        #endregion

        public ControllerDecoderBase(ILogger log, IServiceProvider serviceProvider, IOptions<SignalControllerDownloaderConfiguration> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);


        #region Properties

        public abstract SignalControllerType ControllerType { get; }

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

        public async Task<HashSet<ControllerEventLog>> ExecuteAsync(FileInfo parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            cancelToken.ThrowIfCancellationRequested();

            if (parameter == null && !parameter.Exists)
                return await Task.FromException<HashSet<ControllerEventLog>>(new ArgumentNullException($"parameter"));

            //return value
            HashSet<ControllerEventLog> returnValue = new HashSet<ControllerEventLog>(new ControllerEventLogEqualityComparer());

            if (CanExecute(parameter))
            {
                //convert file to stream
                var memoryStream = parameter.ToMemoryStream();

                //check if stream is compressed
                memoryStream = IsCompressed(memoryStream) ? (MemoryStream)Decompress(memoryStream) : memoryStream;

                try
                {
                    returnValue = await DecodeAsync(parameter.Directory.Name, memoryStream, progress, cancelToken);
                }
                catch (TaskCanceledException e)
                {
                    e.LogE();
                    return await Task.FromCanceled<HashSet<ControllerEventLog>>(cancelToken);
                }
                catch (Exception e)
                {
                    e.LogE();
                    return await Task.FromException<HashSet<ControllerEventLog>>(e);
                }
            }
            else
            {
                return await Task.FromException<HashSet<ControllerEventLog>>(new ExecuteException());
            }

            return returnValue;
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

        public abstract Task<HashSet<ControllerEventLog>> DecodeAsync(string signalId, Stream stream, IProgress<int> progress = null, CancellationToken cancelToken = default);

        //public override void Dispose()
        //{
        //    //throw new NotImplementedException();
        //}

        #endregion
    }
}
