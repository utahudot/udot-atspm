using System;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.PerdueCoordinationDiagram
{
    [DataContract]
    public class PurdueCoordinationDiagramOptions
    {
        public int SelectedBinSize { get; set; }
        public bool ShowVolumes { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string SignalIdentifier { get; set; }
    }
}
