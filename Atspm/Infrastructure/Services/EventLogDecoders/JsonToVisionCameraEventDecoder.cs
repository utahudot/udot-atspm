using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Infrastructure.Services.EventLogDecoders;


namespace Utah.Udot.ATSPM.Infrastructure.Services.EventLogDecoders
{
    public class JsonToVisionCameraEventDecoder : EventLogDecoderBase<VisionCameraStatisticsEvent>
    {
        public override IEnumerable<VisionCameraStatisticsEvent> Decode(Device device, Stream stream, CancellationToken cancelToken = default)
        {
            var memoryStream = (MemoryStream)stream;
            var rootStats = memoryStream.ToArray().FromEncodedJson<Root>().statistics;

            return rootStats.Select(x => new VisionCameraStatisticsEvent
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
