using System;

namespace ATSPM.ReportApi.Business.Common
{
    public class SignalResult : BaseResult
    {
        public SignalResult()
        {
                
        }

        public string locationIdentifier { get; set; }
        public string SignalDescription { get; set; }

        public SignalResult(string locationId, DateTime start, DateTime end) : base(start, end)
        {
            locationIdentifier = locationId;
        }
    }
}
