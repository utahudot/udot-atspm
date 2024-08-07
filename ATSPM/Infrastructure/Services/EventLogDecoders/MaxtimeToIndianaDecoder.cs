#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Services.EventLogDecoders/MaxtimeToIndianaDecoder.cs
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

using ATSPM.Application.Exceptions;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace ATSPM.Infrastructure.Services.EventLogDecoders
{
    /// <inheritdoc/>
    public class MaxtimeToIndianaDecoder : EventLogDecoderBase<IndianaEvent>
    {

        /// <inheritdoc/>
        //public MaxTimeEventLogDecoder(ILogger<MaxTimeEventLogDecoder> log, IOptionsSnapshot<SignalControllerDecoderConfiguration> options) : base(log, options) { }

        #region Properties

        #endregion

        #region Methods

        /// <inheritdoc/>
        //public override bool CanExecute(Tuple<Device, FileInfo> parameter)
        //{
        //    var device = parameter.Item1;
        //    var file = parameter.Item2;

        //    return base.CanExecute(parameter) && (file.Extension == ".xml" || file.Extension == ".XML");
        //}

        /// <inheritdoc/>
        public override IEnumerable<IndianaEvent> Decode(Device device, Stream stream, CancellationToken cancelToken = default)
        {
            cancelToken.ThrowIfCancellationRequested();

            if (device == null)
                throw new ArgumentNullException(nameof(device), "Device can not be null");

            if (stream?.Length == 0)
                throw new InvalidDataException("Stream is empty");

            var locationIdentifider = device.Location.LocationIdentifier;

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
                throw new EventLogDecoderException(e);
            }

            foreach (var l in logs)
            {
                cancelToken.ThrowIfCancellationRequested();

                try
                {
                    var log = new IndianaEvent()
                    {
                        LocationIdentifier = locationIdentifider,
                        EventCode = Convert.ToInt16(l.Attribute("EventTypeID").Value),
                        EventParam = Convert.ToInt16(l.Attribute("Parameter").Value),
                        Timestamp = Convert.ToDateTime(l.Attribute("TimeStamp").Value)
                    };

                    decodedLogs.Add(log);
                }
                catch (Exception e)
                {
                    throw new EventLogDecoderException(e);
                }
            }

            return decodedLogs;
        }

        #endregion
    }
}