#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.ATSPM.Infrastructure.Services.EventLogDecoders/VisionCameraJsonToEnhancedVehicleEventDecoder.cs
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

using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Infrastructure.Services.EventLogDecoders;


namespace Utah.Udot.ATSPM.Infrastructure.Services.EventLogDecoders
{
    public class VisionCameraJsonToEnhancedVehicleEventDecoder : EventLogDecoderBase<EnhancedEventLog>
    {
        public override IEnumerable<EnhancedEventLog> Decode(Device device, Stream stream, CancellationToken cancelToken = default)
        {
            var memoryStream = (MemoryStream)stream;
            var rootStats = memoryStream.ToArray().FromEncodedJson<Root>().detections;

            var response = rootStats.Select(x => new EnhancedEventLog
            {
                LocationIdentifier = device.Location.LocationIdentifier,
                Timestamp = DateTime.SpecifyKind(x.time.ToLocalTime(), DateTimeKind.Unspecified),
                DetectorId = "device.Id",
                Mph = (int)Math.Round(x.speed),
                Kph = ConvertMphToKph(x.speed),
                ZoneId = x.zoneId,
                ZoneName = x.zoneName,
                ObjectType = x.objectType,
                Length = x.length,
                Direction = x.direction

            });
            return response;
        }

        private class Root
        {
            public List<VisionCameraDetections> detections { get; set; }
        }

        private class VisionCameraDetections
        {
            public int zoneId { get; set; }
            public string zoneName { get; set; }
            public DateTime time { get; set; }
            public string objectType { get; set; }
            public double speed { get; set; }
            public double length { get; set; }
            public string direction { get; set; }
        }

        public int ConvertMphToKph(double mph)
        {
            return (int)Math.Round(mph * 1.60934);
        }
    }
}