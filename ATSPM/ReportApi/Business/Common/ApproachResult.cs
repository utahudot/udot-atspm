using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;

namespace ATSPM.ReportApi.Business.Common
{
    public class ApproachResult : SignalResult
    {
        public ApproachResult()
        {
                
        }

        public int ApproachId { get; set; }
        public string ApproachDescription { get; set; }

        public ApproachResult(int approachId, string signalId, DateTime start, DateTime end) : base(signalId, start, end)
        {
            ApproachId = approachId;
        }
    }
}
