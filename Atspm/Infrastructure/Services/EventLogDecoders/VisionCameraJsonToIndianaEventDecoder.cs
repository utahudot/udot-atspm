using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Infrastructure.Services.EventLogDecoders;


namespace Utah.Udot.ATSPM.Infrastructure.Services.EventLogDecoders
{
    public class VisionCameraJsonToIndianaEventDecoder : EventLogDecoderBase<IndianaEvent>
    {
        public override IEnumerable<IndianaEvent> Decode(Device device, Stream stream, CancellationToken cancelToken = default)
        {
            var memoryStream = (MemoryStream)stream;
            var rootStats = memoryStream.ToArray().FromEncodedJson<Root>().detections;
            List<Detector> list = device.Location.Approaches.SelectMany(x => x.Detectors).ToList();
            var response = rootStats.Select(visionCameraDetection =>
            {
                var matchingDetector = list.Where(x => x.DectectorIdentifier == visionCameraDetection.zoneName).FirstOrDefault();
                var timestamp = visionCameraDetection.time;
                short eventParam = (short)matchingDetector.DetectorChannel;
                short eventCode = 82; //TODO I think this is correct, but need to verify

                // Create and return the IndianaEvent
                return new IndianaEvent
                {
                    LocationIdentifier = device.Location.LocationIdentifier,
                    Timestamp = timestamp,
                    EventCode = eventCode,
                    EventParam = eventParam
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
    }
}