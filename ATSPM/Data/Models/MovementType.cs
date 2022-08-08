using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class MovementType
    {
        public MovementType()
        {
            Detectors = new HashSet<Detector>();
        }

        public int MovementTypeId { get; set; }
        public string Description { get; set; } = null!;
        public string Abbreviation { get; set; } = null!;
        public int DisplayOrder { get; set; }

        public virtual ICollection<Detector> Detectors { get; set; }
    }
}
