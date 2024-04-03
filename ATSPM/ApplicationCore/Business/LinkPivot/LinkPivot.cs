using ATSPM.Application.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.LinkPivot
{
    public class LinkPivot : BaseResult
    {
        public LinkPivot(DateTime start, DateTime end) : base(start, end) {
        }
        
        public List<AdjustmentObject> Adjustments { get; set; }

        public List<LinkPivotPair> PairedApproaches { get; set; } = new List<LinkPivotPair>();
    }
}