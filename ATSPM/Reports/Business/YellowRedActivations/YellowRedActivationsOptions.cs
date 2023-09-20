using System;

namespace ATSPM.Application.Reports.Business.YellowRedActivations
{
    public class YellowRedActivationsOptions
    {
        public double SevereLevelSeconds { get; set; }
        public string SignalIdentifier { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int MetricTypeId { get; set; } = 11;
    }
}