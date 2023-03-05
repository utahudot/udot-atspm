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
            StartDate = startDate;
            EndDate = endDate;
        }

        public string SignalId { get;  set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}