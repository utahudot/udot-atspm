using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class MetricType
    {
        public MetricType()
        {
            ActionLogActionLogs = new HashSet<ActionLog>();
            DetectionTypeDetectionTypes = new HashSet<DetectionType>();
            MetricCommentComments = new HashSet<MetricComment>();
        }

        public int MetricId { get; set; }
        public string ChartName { get; set; } = null!;
        public string Abbreviation { get; set; } = null!;
        public bool ShowOnWebsite { get; set; }
        public bool ShowOnAggregationSite { get; set; }
        public int DisplayOrder { get; set; }

        public virtual ICollection<ActionLog> ActionLogActionLogs { get; set; }
        public virtual ICollection<DetectionType> DetectionTypeDetectionTypes { get; set; }
        public virtual ICollection<MetricComment> MetricCommentComments { get; set; }
    }
}
