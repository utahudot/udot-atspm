using System;

namespace ATSPM.Application.Reports.Business.TurningMovementCounts
{
    public class TurningMovementCountsOptions
    {
        public int SelectedBinSize { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string SignalIdentifier { get; set; }
        public int MetricTypeId { get; internal set; } = 5;
    }
}