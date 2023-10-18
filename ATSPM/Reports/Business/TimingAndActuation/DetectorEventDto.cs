using System.Collections.Generic;

namespace Reports.Business.TimingAndActuation
{
    public class DetectorEventDto
    {
        public DetectorEventDto(string name, List<DetectorEventBase> events) {
            Name = name;
            Events = events;
        }

        public string Name { get; set; }
        public List<DetectorEventBase> Events { get; set; }
    }
}
