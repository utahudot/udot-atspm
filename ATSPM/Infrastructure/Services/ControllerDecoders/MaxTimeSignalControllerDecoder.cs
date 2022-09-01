using ATSPM.Application.Common.EqualityComparers;
using ATSPM.Application.Configuration;
using ATSPM.Application.Enums;
using ATSPM.Application.Exceptions;
using ATSPM.Application.Extensions;
using ATSPM.Application.Services.SignalControllerProtocols;
using ATSPM.Application.ValueObjects;
using ATSPM.Data.Models;
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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

namespace ATSPM.Infrasturcture.Services.ControllerDecoders
{
    public class MaxTimeSignalControllerDecoder : ControllerDecoderBase
    {
        public MaxTimeSignalControllerDecoder(ILogger<MaxTimeSignalControllerDecoder> log, IOptionsSnapshot<SignalControllerDecoderConfiguration> options) : base(log, options) { }

        #region Properties

        #endregion

        #region Methods

        public override bool CanExecute(FileInfo parameter)
        {
            return parameter.Exists && (parameter.Extension == ".xml" || parameter.Extension == ".XML");
        }

        public override async IAsyncEnumerable<ControllerEventLog> DecodeAsync(string SignalId, Stream stream, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            //cancelToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(SignalId))
                throw new ControllerLoggerDecoderException("SignalId can not be null", new ArgumentNullException(nameof(SignalId)));

            if (stream?.Length == 0)
                throw new ControllerLoggerDecoderException("Stream is empty", new InvalidDataException(nameof(stream)));

            stream.Position = 0;

            IEnumerable<XElement> logs;

            try
            {
                var xml = XDocument.Load(stream);
                logs = xml.Descendants().Where(d => d.Name == "Event");
            }
            catch (Exception e)
            {
                throw new ControllerLoggerDecoderException($"Exception decoding {SignalId}", e);
            }

            foreach (var l in logs)
            {
                ControllerEventLog log;

                try
                {
                    log = new ControllerEventLog()
                    {
                        SignalId = SignalId,
                        EventCode = Convert.ToInt32(l.Attribute("EventTypeID").Value),
                        EventParam = Convert.ToInt32(l.Attribute("Parameter").Value),
                        Timestamp = Convert.ToDateTime(l.Attribute("TimeStamp").Value)
                    };
                }
                catch (Exception e)
                {
                    throw new ControllerLoggerDecoderException($"Exception decoding {SignalId}", e);
                }

                yield return log;
            }
        }

        //public override void Dispose()
        //{
        //    //throw new NotImplementedException();
        //}

        #endregion
    }
}