using System;
using System.Collections.Generic;

#nullable disable

namespace ControllerLogger.Models
{
    public partial class DetectionTypeDetector
    {
        public int Id { get; set; }
        public int DetectionTypeId { get; set; }

        public virtual DetectionType DetectionType { get; set; }
        public virtual Detector IdNavigation { get; set; }
    }
}
