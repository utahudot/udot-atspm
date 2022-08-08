using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class ActionLog
    {
        public ActionLog()
        {
            ActionActions = new HashSet<Action>();
            MetricTypeMetrics = new HashSet<MetricType>();
        }

        public int ActionLogId { get; set; }
        public DateTime Date { get; set; }
        public int AgencyId { get; set; }
        public string? Comment { get; set; }
        public string SignalId { get; set; } = null!;
        public string Name { get; set; } = null!;

        public virtual Agency Agency { get; set; } = null!;

        public virtual ICollection<Action> ActionActions { get; set; }
        public virtual ICollection<MetricType> MetricTypeMetrics { get; set; }
    }
}
