using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Infrastructure.Services.EventLogDecoders;


namespace Utah.Udot.ATSPM.Infrastructure.Services.EventLogDecoders
{
    public class VisionCameraJsonToEnhancedVehicleEventDecoder : EventLogDecoderBase<EnhancedVehicleEvent>
    {
        public override IEnumerable<EnhancedVehicleEvent> Decode(Device device, Stream stream, CancellationToken cancelToken = default)
        {
            var memoryStream = (MemoryStream)stream;
            var rootStats = memoryStream.ToArray().FromEncodedJson<Root>().detections;

            return rootStats.Select(x => new EnhancedVehicleEvent
            {
                LocationIdentifier = device.Location.LocationIdentifier,
                ZoneId = x.zoneId,
                ZoneName = x.zoneName,
                Timestamp = x.time,
                ObjectType = x.objectType,
                Speed = x.speed,
                Length = x.length,
                Direction = x.direction

            });
        }

        private class Root
        {
            public List<Detections> detections { get; set; }
        }

        private class Detections
        {
            public int zoneId { get; set; }
            public string zoneName { get; set; }
            public DateTime time { get; set; }
            public string objectType { get; set; }
            public double speed { get; set; }
            public double length { get; set; }
            public string direction { get; set; }
        }
    }
}
