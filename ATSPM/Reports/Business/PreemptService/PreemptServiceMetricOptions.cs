using System;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.PreemptService

{
    [DataContract]
    public class PreemptServiceMetricOptions
    {
        public string SignalIdentifier { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}