#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Services.EventLogDecoders/MaxtimeToIndianaDecoder.cs
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

using System.Xml.Linq;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Infrastructure.Services.EventLogDecoders
{
    /// <inheritdoc/>
    public class MaxtimeToIndianaDecoder : EventLogDecoderBase<IndianaEvent>
    {
        #region Properties

        #endregion

        #region Methods

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
                    if (Int16.TryParse(l.Attribute("EventTypeID").Value, out short eventCode) && Int16.TryParse(l.Attribute("Parameter").Value, out short eventParam))
                    {
                        var log = new IndianaEvent()
                        {
                            LocationIdentifier = locationIdentifider,
                            EventCode = eventCode,
                            EventParam = eventParam,
                            Timestamp = Convert.ToDateTime(l.Attribute("TimeStamp").Value)
                        };

                        decodedLogs.Add(log);
                    }
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