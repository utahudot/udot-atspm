using System;
using System.Collections.Generic;

#nullable disable

namespace ControllerLogger.Models
{
    public partial class DetectionTypeMetricType
    {
        public int DetectionTypeDetectionTypeId { get; set; }
        public int MetricTypeMetricId { get; set; }

        public virtual DetectionType DetectionTypeDetectionType { get; set; }
        public virtual MetricType MetricTypeMetric { get; set; }
    }
}
