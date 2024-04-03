using ATSPM.Application.Business.PurdueCoordinationDiagram;
using System.Collections.Generic;

namespace ATSPM.Application.Business.LinkPivot
{
    public class LinkPivotPcdResult
    {
        public LinkPivotPcdResult()
        {
            pcdExisting = new List<PurdueCoordinationDiagramResult>();
            pcdPredicted = new List<PurdueCoordinationDiagramResult>();
        }
        public double ExistingTotalAOG { get; set; }
        public double ExistingTotalPAOG { get; set; } = 0;
        public double PredictedTotalAOG { get; set; }
        public double PredictedTotalPAOG { get; set; } = 0;
        public double PredictedVolume { get; set; }
        public double ExistingVolume { get; set; }
        public List<PurdueCoordinationDiagramResult> pcdExisting { get; set; }
        public List<PurdueCoordinationDiagramResult> pcdPredicted { get; set; }
    }
}
