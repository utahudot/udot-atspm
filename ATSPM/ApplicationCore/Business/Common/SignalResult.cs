using System;

namespace ATSPM.Application.Business.Common
{
    public class LocationResult : BaseResult
    {
        public LocationResult()
        {

        }

        public LocationResult(string locationId, DateTime start, DateTime end) : base(start, end)
        {
            locationIdentifier = locationId;
        }

        public string locationIdentifier { get; set; }
        public string LocationDescription { get; set; }
    }
}
