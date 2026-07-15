#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.ATSPM.Infrastructure.Services.EventLogDecoders/JsonToVisionCameraStatisticEventDecoder.cs
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
    public class JsonToVisionCameraStatisticEventDecoder : EventLogDecoderBase<VisionCameraStatisticsEvent>
    {
        public override IEnumerable<VisionCameraStatisticsEvent> Decode(Device device, Stream stream, CancellationToken cancelToken = default)
        {
            var memoryStream = (MemoryStream)stream;
            var rootStats = memoryStream.ToArray().FromEncodedJson<Root>().statistics;

            var response = rootStats.Select(x => new VisionCameraStatisticsEvent
            {
                LocationIdentifier = device.Location.LocationIdentifier,
                ZoneId = x.zoneId,
                ZoneName = x.zoneName,
                Timestamp = x.time,
                AverageSpeed = x.averageSpeed,
                Volume = x.volume,
                Occupancy = x.occupancy,
                ThroughCount = x.throughCount,
                RightTurnCount = x.rightTurnCount,
                RightToLeftCount = x.rightToLeftCount,
                LeftTurnCount = x.leftTurnCount,
                LeftToRightCount = x.leftToRightCount
            });
            return response;
        }

        private class Root
        {
            public List<Statistics> statistics { get; set; }
        }

        private class Statistics
        {
            public int zoneId { get; set; }
            public string zoneName { get; set; }
            public DateTime time { get; set; }
            public double averageSpeed { get; set; }
            public int volume { get; set; }
            public double occupancy { get; set; }
            public int throughCount { get; set; }
            public int rightTurnCount { get; set; }
            public int leftTurnCount { get; set; }
            public int leftToRightCount { get; set; }
            public int rightToLeftCount { get; set; }
        }
    }
}
