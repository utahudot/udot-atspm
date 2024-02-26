using ATSPM.Application.Business.Common;

namespace ATSPM.Application.Business.LinkPivot
{
    public class LinkPivotOptions : OptionsBase
    {
        public int RouteId { get; set; }
        public int CycleTime { get; set; }
        public string Direction { get; set; }
        public double Bias { get; set; }
        public string BiasDirection { get; set; }
        public bool Sunday { get; set; }
        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
    }
}