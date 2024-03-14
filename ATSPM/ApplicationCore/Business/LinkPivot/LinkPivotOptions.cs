using ATSPM.Application.Business.Common;

namespace ATSPM.Application.Business.LinkPivot
{
    public class LinkPivotOptions : OptionsBase
    {
        public int RouteId { get; set; }
        public int CycleLength { get; set; }
        public string Direction { get; set; }
        public double Bias { get; set; }
        public string BiasDirection { get; set; }
        public bool DaysToInclude { get; set; }
    }
}