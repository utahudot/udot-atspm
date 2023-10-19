using Reports.Business.Common;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.PerdueCoordinationDiagram
{
    [DataContract]
    public class PurdueCoordinationDiagramOptions : OptionsBase
    {
        public int SelectedBinSize { get; set; }
        public bool ShowVolumes { get; set; }
    }
}
