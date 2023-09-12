using System;

namespace Reports.Business.Common
{
    public class SignalResult : BaseResult
    {
        public string SignalId { get; set; }
        public string SignalDescription { get; set; }

        public SignalResult(string signalId, DateTime start, DateTime end) : base(start, end)
        {
            SignalId = signalId;
        }
    }
}
