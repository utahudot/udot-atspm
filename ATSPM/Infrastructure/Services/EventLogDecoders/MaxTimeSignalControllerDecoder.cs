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
using ATSPM.Application.Configuration;
using ATSPM.Application.Exceptions;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ATSPM.Infrastructure.Services.ControllerDecoders
{
    /// <inheritdoc/>
    public class MaxTimeLocationControllerDecoder : EventLogDecoderBase<IndianaEvent>
    {

        /// <inheritdoc/>
        public MaxTimeLocationControllerDecoder(ILogger<MaxTimeLocationControllerDecoder> log, IOptionsSnapshot<SignalControllerDecoderConfiguration> options) : base(log, options) { }

        #region Properties

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override bool CanExecute(Tuple<Device, FileInfo> parameter)
        {
            var device = parameter.Item1;
            var file = parameter.Item2;

            return file.Exists && (file.Extension == ".xml" || file.Extension == ".XML");
        }

        /// <inheritdoc/>
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
                try
                {
                    var log = new IndianaEvent()
                    {
                        LocationIdentifier = locationId,
                        EventCode = Convert.ToInt16(l.Attribute("EventTypeID").Value),
                        EventParam = Convert.ToByte(l.Attribute("Parameter").Value),
                        Timestamp = Convert.ToDateTime(l.Attribute("TimeStamp").Value)
                    };

                    decodedLogs.Add(log);
                }
                catch (Exception e)
                {
                    throw new ControllerLoggerDecoderException($"Exception decoding {locationId}", e);
                }
            }

            return decodedLogs;
        }

        #endregion
    }
}