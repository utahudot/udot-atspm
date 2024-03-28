using ATSPM.Data.Enums;
using ATSPM.Data.Models;

namespace ATSPM.Application.Business.LinkPivot
{
    public class AdjustmentObject
    {
        public double DownstreamVolume { get; set; }
        public double UpstreamVolume { get; set; }
        public int LinkNumber { get; set; }
        public double AogTotalBefore { get; set; }
        public int PAogTotalBefore { get; set; }
        public double AogTotalPredicted { get; set; }
        public int PAogTotalPredicted { get; set; }
        public string ResultChartLocation { get; set; }
        public string DownstreamLocation { get; set; }
        public string DownLocationIdentifier { get; set; }
        public DirectionTypes DownstreamApproachDirection { get; set; }
        public DirectionTypes UpstreamApproachDirection { get; set; }
        public double AOGDownstreamPredicted { get; set; }
        public double AOGUpstreamPredicted { get; set; }
        public double AOGDownstreamBefore { get; set; }
        public double AOGUpstreamBefore { get; set; }
        public int PAOGDownstreamPredicted { get; set; }
        public int PAOGUpstreamPredicted { get; set; }
        public int PAOGDownstreamBefore { get; set; }
        public int PAOGUpstreamBefore { get; set; }
        public int Adjustment { get; set; }
        public int Delta { get; set; }
        public string LocationIdentifier { get; set; }
        public string DownstreamLocationIdentifier { get; set; }
        public string Location { get; set; }

    }
}
