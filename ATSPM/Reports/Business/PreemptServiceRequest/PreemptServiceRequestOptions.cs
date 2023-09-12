using System;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.PreemptServiceRequest
{
    [DataContract]
    public class PreemptServiceRequestOptions
    {
        public PreemptServiceRequestOptions()
        {
        }

        public string SignalId { get;  set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}