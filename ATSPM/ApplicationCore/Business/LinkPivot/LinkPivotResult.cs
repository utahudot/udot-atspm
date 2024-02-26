using ATSPM.Application.Business.Common;
using System.Collections.Generic;

namespace ATSPM.Application.Business.LinkPivot
{
    public class LinkPivotResult : BaseResult
    {
        public List<AdjustmentObject> Adjustments { get; set; }
    }
}