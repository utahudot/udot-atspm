using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class LaneType
    {
        public LaneType()
        {
            Detectors = new HashSet<Detector>();
        }

        public int LaneTypeId { get; set; }
        public string Description { get; set; } = null!;
        public string Abbreviation { get; set; } = null!;

        public virtual ICollection<Detector> Detectors { get; set; }
    }
}
