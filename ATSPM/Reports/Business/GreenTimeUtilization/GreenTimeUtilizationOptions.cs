using System;

namespace ATSPM.Application.Reports.Business.PerdueCoordinationDiagram
{
    public class GreenTimeUtilizationOptions
    {
        public string SignalIdentifier { get; set; }
        public int MetricTypeId { get; set; } = 36;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int SelectedBinSize { get; set; }
        public bool UsePermissivePhase { get; internal set; }
    }
}