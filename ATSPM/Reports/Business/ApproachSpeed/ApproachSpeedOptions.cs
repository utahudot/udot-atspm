using System;

namespace ATSPM.Application.Reports.Business.ApproachSpeed
{
    public class ApproachSpeedOptions
    {
        public int SelectedBinSize { get; set; }
        public int MetricTypeId { get; } = 10;
        public string SignalIdentifier { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool UsePermissivePhase { get; set; }
        public bool GetPermissivePhase { get; internal set; }
    }
}