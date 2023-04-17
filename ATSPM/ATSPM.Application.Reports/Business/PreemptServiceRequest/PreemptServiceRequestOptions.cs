using System;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.PreemptServiceRequest
{
    [DataContract]
    public class PreemptServiceRequestOptions
    {
        public PreemptServiceRequestOptions(string signalId, DateTime startDate, DateTime endDate)
        {
            SignalId = signalId;
            Start = startDate;
            End = endDate;
        }

        public string SignalId { get;  set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}