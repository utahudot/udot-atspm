using System;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.PreemptService

{
    [DataContract]
    public class PreemptServiceMetricOptions
    {
        public PreemptServiceMetricOptions(string signalId, DateTime startDate, DateTime endDate, double yAxisMax)
        {
            SignalId = signalId;
            Start = startDate;
            End = endDate;
        }

        public string SignalId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        
    }
}