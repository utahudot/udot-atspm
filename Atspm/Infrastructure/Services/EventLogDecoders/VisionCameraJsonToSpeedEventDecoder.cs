using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Infrastructure.Services.EventLogDecoders;


namespace Utah.Udot.ATSPM.Infrastructure.Services.EventLogDecoders
{
    public class VisionCameraJsonToSpeedEventDecoder : EventLogDecoderBase<SpeedEvent>
    {
        public override IEnumerable<SpeedEvent> Decode(Device device, Stream stream, CancellationToken cancelToken = default)
        {
            var memoryStream = (MemoryStream)stream;
            var rootStats = memoryStream.ToArray().FromEncodedJson<Root>().detections;
            List<Detector> list = device.Location.Approaches.SelectMany(x => x.Detectors).ToList();
            var response = rootStats.Select(visionCameraDetection =>
            {
                var matchingDetector = list.Where(x => x.DectectorIdentifier == visionCameraDetection.zoneName).FirstOrDefault();
                var timestamp = visionCameraDetection.time;
                string detectorId = matchingDetector.Id.ToString();
                var mph = (int)visionCameraDetection.speed;
                var kph = ConvertMphToKph(mph);

                // Create and return the SpeedEvent
                return new SpeedEvent
                {
                    LocationIdentifier = device.Location.LocationIdentifier,
                    Timestamp = timestamp,
                    DetectorId = detectorId,
                    Mph = mph,
                    Kph = kph
                };
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