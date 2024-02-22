using ATSPM.Application.Business.Common;

namespace ATSPM.Application.Business.TimeSpaceDiagram
{
    public class TimeSpaceDiagramOptions : BasePhaseOptions
    {
        public int RouteId { get; set; }
        public int ?SpeedLimit { get; set; }
    }
}
