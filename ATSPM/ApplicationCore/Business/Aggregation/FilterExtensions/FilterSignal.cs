using System.Collections.Generic;

namespace ATSPM.Application.Business.Aggregation.FilterExtensions
{
    public class FilterSignal
    {
        public string SignalId { get; set; }
        public bool Exclude { get; set; }
        public List<FilterApproach> FilterApproaches { get; set; } = new List<FilterApproach>();
    }
}