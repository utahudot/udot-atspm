using System;
using System.Collections.Generic;

#nullable disable

namespace ATSPM.Application.Models
{
    public partial class DetectionType : ATSPMModelBase
    {
        public DetectionType()
        {
            Detectors = new HashSet<Detector>();
            MetricTypes = new HashSet<MetricType>();
        }

        public int DetectionTypeId { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Detector> Detectors { get; set; }
        public virtual ICollection<MetricType> MetricTypes { get; set; }
    }
}
