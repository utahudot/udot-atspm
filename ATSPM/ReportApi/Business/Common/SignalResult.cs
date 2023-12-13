using System;

namespace ATSPM.ReportApi.Business.Common
{
    public class LocationResult : BaseResult
    {
        public LocationResult()
        {
                
        }

        public string locationIdentifier { get; set; }
        public string LocationDescription { get; set; }

        public LocationResult(string locationId, DateTime start, DateTime end) : base(start, end)
        {
            locationIdentifier = locationId;
        }
    }
}
