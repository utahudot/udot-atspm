using ATSPM.ReportApi.Business.Common;

namespace ATSPM.ReportApi.Business.TimeSpaceDiagram
{
    public class TimeSpaceDiagramOption : BasePhaseOptions
    {
        public int RouteId { get; set; }
        //public string ChartType { get; set; }
        //public bool OpposingPhase { get; set; }
        public int ?SpeedLimit { get; set; }
    }
}
