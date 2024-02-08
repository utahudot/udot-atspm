using ATSPM.Application.Common.EqualityComparers;
using ATSPM.Application.Configuration;
using ATSPM.Application.Enums;
using ATSPM.Application.Exceptions;
using ATSPM.Application.Extensions;
using ATSPM.Application.ValueObjects;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
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

namespace ATSPM.Infrastructure.Services.ControllerDecoders
{
    public class MaxTimeLocationControllerDecoder : ControllerDecoderBase
    {
        public MaxTimeLocationControllerDecoder(ILogger<MaxTimeLocationControllerDecoder> log, IOptionsSnapshot<SignalControllerDecoderConfiguration> options) : base(log, options) { }

        #region Properties

        #endregion

        #region Methods

        public override bool CanExecute(FileInfo parameter)
        {
            return parameter.Exists && (parameter.Extension == ".xml" || parameter.Extension == ".XML");
        }

        public override HashSet<EventLogModelBase> Decode(string locationId, Stream stream)
        {
            //cancelToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(locationId))
                throw new ControllerLoggerDecoderException("locationId can not be null", new ArgumentNullException(nameof(locationId)));

            if (stream?.Length == 0)
                throw new ControllerLoggerDecoderException("Stream is empty", new InvalidDataException(nameof(stream)));

            stream.Position = 0;

            IEnumerable<XElement> logs;

            HashSet<EventLogModelBase> decodedLogs = new();

            try
            {
                var xml = XDocument.Load(stream);
                logs = xml.Descendants().Where(d => d.Name == "Event");
            }
            catch (Exception e)
            {
                throw new ControllerLoggerDecoderException($"Exception decoding {locationId}", e);
            }

            foreach (var l in logs)
            {
                IndianaEvent log;

                try
                {
                    log = new IndianaEvent()
                    {
                        LocationIdentifier = locationId,
                        EventCode = (DataLoggerEnum)Convert.ToInt16(l.Attribute("EventTypeID").Value),
                        EventParam = Convert.ToByte(l.Attribute("Parameter").Value),
                        Timestamp = Convert.ToDateTime(l.Attribute("TimeStamp").Value)
                    };
                }
                catch (Exception e)
                {
                    throw new ControllerLoggerDecoderException($"Exception decoding {locationId}", e);
                }

                decodedLogs.Add(log);
            }

            return decodedLogs;
        }

        //public override void Dispose()
        //{
        //    //throw new NotImplementedException();
        //}

        #endregion
    }
}