using System;
using System.Collections.Generic;

#nullable disable

namespace ControllerLogger.Models
{
    public partial class DetectionHardware
    {
        public DetectionHardware()
        {
            Detectors = new HashSet<Detector>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Detector> Detectors { get; set; }
    }
}
