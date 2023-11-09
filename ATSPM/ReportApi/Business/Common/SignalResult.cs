using System;

namespace ATSPM.ReportApi.Business.Common
{
    public class SignalResult : BaseResult
    {
        public SignalResult()
        {
                
        }

        public string SignalIdentifier { get; set; }
        public string SignalDescription { get; set; }

        public SignalResult(string signalId, DateTime start, DateTime end) : base(start, end)
        {
            SignalIdentifier = signalId;
        }
    }
}
