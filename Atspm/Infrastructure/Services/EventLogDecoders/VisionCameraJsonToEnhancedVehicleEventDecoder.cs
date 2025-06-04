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
                Timestamp = x.time,
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