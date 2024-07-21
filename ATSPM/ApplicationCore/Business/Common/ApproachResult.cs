using System;

namespace ATSPM.Application.Business.Common
{
    public class ApproachResult : LocationResult
    {
        public ApproachResult()
        {

        }

        public ApproachResult(int approachId, string locationId, DateTime start, DateTime end) : base(locationId, start, end)
        {
            ApproachId = approachId;
        }

        public int ApproachId { get; set; }
        public string ApproachDescription { get; set; }
    }
}
