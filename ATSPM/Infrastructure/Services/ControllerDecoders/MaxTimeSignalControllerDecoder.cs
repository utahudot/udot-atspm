using ATSPM.Application.Common.EqualityComparers;
using ATSPM.Application.Configuration;
using ATSPM.Application.Enums;
using ATSPM.Application.Extensions;
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
using System.Xml.Linq;

namespace ATSPM.Infrasturcture.Services.ControllerDecoders
{
    public class MaxTimeSignalControllerDecoder : ControllerDecoderBase
    {
        public MaxTimeSignalControllerDecoder(ILogger<MaxTimeSignalControllerDecoder> log, IOptions<SignalControllerDecoderConfiguration> options) : base(log, options) { }

        #region Properties

        #endregion

        #region Methods

        public override bool CanExecute(FileInfo parameter)
        {
            return parameter.Exists && (parameter.Extension == ".xml" || parameter.Extension == ".XML");
        }

        public override IAsyncEnumerable<ControllerEventLog> DecodeAsync(string signalId, Stream stream, CancellationToken cancelToken = default)
        {
            throw new NotImplementedException();


            //cancelToken.ThrowIfCancellationRequested();

            //if (string.IsNullOrEmpty(signalId))
            //    throw new ArgumentNullException(nameof(signalId));

            //if (stream?.Length == 0)
            //    throw new InvalidDataException("Stream is empty");

            //HashSet<ControllerEventLog> logList = new HashSet<ControllerEventLog>(new ControllerEventLogEqualityComparer());

            //stream.Position = 0;

            //try
            //{
            //    XDocument xml = XDocument.Load(stream);

            //    foreach (var a in xml.Descendants().Where(d => d.Name == "Event"))
            //    {
            //        var log = new ControllerEventLog()
            //        {
            //            SignalId = signalId,
            //            EventCode = Convert.ToInt32(a.Attribute("EventTypeID").Value),
            //            EventParam = Convert.ToInt32(a.Attribute("Parameter").Value),
            //            Timestamp = Convert.ToDateTime(a.Attribute("TimeStamp").Value)
            //        };

            //        if (log.Timestamp <= DateTime.Now && log.Timestamp > _options.Value.EarliestAcceptableDate)
            //        {
            //            logList.Add(log);

            //            //report progress
            //            progress?.Report(logList.Count);

            //            //_log.LogDebug("Decoded {ListCount} items from {SignalID}", logList.Count, signalId);
            //        }
            //    }

            //    progress?.Report(logList.Count);
            //    return Task.FromResult(logList);
            //}
            //catch (Exception e)
            //{
            //    e.LogE();

            //    return Task.FromException<HashSet<ControllerEventLog>>(e);
            //}

            //throw new InvalidDataException($"Decoding error, not a valid file or stream format {signalId}");
        }

        //public override void Dispose()
        //{
        //    //throw new NotImplementedException();
        //}

        #endregion
    }
}