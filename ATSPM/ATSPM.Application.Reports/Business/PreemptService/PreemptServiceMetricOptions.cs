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
            StartDate = startDate;
            EndDate = endDate;
        }

        public string SignalId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        
    }
}