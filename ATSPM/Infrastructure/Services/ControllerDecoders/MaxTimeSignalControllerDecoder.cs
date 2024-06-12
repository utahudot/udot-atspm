#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Services.ControllerDecoders/MaxTimeSignalControllerDecoder.cs
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
    public class MaxTimeLocationControllerDecoder : ControllerDecoderBase<IndianaEvent>
    {
        public MaxTimeLocationControllerDecoder(ILogger<MaxTimeLocationControllerDecoder> log, IOptionsSnapshot<SignalControllerDecoderConfiguration> options) : base(log, options) { }

        #region Properties

        #endregion

        #region Methods

        public override bool CanExecute(Tuple<Device, FileInfo> parameter)
        {
            var device = parameter.Item1;
            var file = parameter.Item2;

            return file.Exists && (file.Extension == ".xml" || file.Extension == ".XML");
        }

        public override IEnumerable<IndianaEvent> Decode(Device device, Stream stream)
        {
            var locationId = device.Location.LocationIdentifier;

            //cancelToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(locationId))
                throw new ControllerLoggerDecoderException("locationId can not be null", new ArgumentNullException(nameof(locationId)));

            if (stream?.Length == 0)
                throw new ControllerLoggerDecoderException("Stream is empty", new InvalidDataException(nameof(stream)));

            stream.Position = 0;

            IEnumerable<XElement> logs;

            HashSet<IndianaEvent> decodedLogs = new();

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
                        EventCode = Convert.ToInt16(l.Attribute("EventTypeID").Value),
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