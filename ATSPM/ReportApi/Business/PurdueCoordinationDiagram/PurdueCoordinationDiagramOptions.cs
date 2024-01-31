using ATSPM.ReportApi.Business.Common;
using System.Runtime.Serialization;

namespace ATSPM.ReportApi.Business.PurdueCoordinationDiagram
{
    [DataContract]
    public class PurdueCoordinationDiagramOptions : OptionsBase
    {
        public int BinSize { get; set; }
        public bool ShowVolumes { get; set; }
        public bool ShowArrivalsOnGreen { get; set; }
        public bool ShowPlanStatistics { get; set; }
    }
}
