using ATSPM.ReportApi.Business.Common;
using System.Runtime.Serialization;

namespace ATSPM.ReportApi.Business.PurdueCoordinationDiagram
{
    [DataContract]
    public class PurdueCoordinationDiagramOptions : OptionsBase
    {
        public int SelectedBinSize { get; set; }
        public bool ShowVolumes { get; set; }
    }
}
