using ATSPM.Application.Business.Common;
using System.Runtime.Serialization;

namespace ATSPM.Application.Business.PurdueCoordinationDiagram
{
    [DataContract]
    public class PurdueCoordinationDiagramOptions : OptionsBase
    {
        public int BinSize { get; set; }
        public bool GetVolume { get; set; } = true;
        public bool ShowPlanStatistics { get; set; }
    }
}
