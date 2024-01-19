using ATSPM.ReportApi.Business.Common;

namespace ATSPM.ReportApi.Business.TimeSpaceDiagram
{
    public class TimeSpaceDiagramOptions : BasePhaseOptions
    {
        public int RouteId { get; set; }
        public int ?SpeedLimit { get; set; }
    }
}
