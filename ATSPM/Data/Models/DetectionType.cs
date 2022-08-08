using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class DetectionType
    {
        public DetectionType()
        {
            Ids = new HashSet<Detector>();
            MetricTypeMetrics = new HashSet<MetricType>();
        }

        public int DetectionTypeId { get; set; }
        public string Description { get; set; } = null!;

        public virtual ICollection<Detector> Ids { get; set; }
        public virtual ICollection<MetricType> MetricTypeMetrics { get; set; }
    }
}
