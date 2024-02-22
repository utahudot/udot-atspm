using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;

namespace ATSPM.ReportApi.Business.Common
{
    public class ApproachResult : LocationResult
    {
        public ApproachResult()
        {
                
        }

        public int ApproachId { get; set; }
        public string ApproachDescription { get; set; }

        public ApproachResult(int approachId, string locationId, DateTime start, DateTime end) : base(locationId, start, end)
        {
            ApproachId = approachId;
        }
    }
}
