using System;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.PreemptService

{
    [DataContract]
    public class PreemptServiceMetricOptions
    {
        public PreemptServiceMetricOptions()
        {
            
        }

        public string SignalId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        
    }
}