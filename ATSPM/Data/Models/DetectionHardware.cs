using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class DetectionHardware
    {
        public DetectionHardware()
        {
            Detectors = new HashSet<Detector>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<Detector> Detectors { get; set; }
    }
}
