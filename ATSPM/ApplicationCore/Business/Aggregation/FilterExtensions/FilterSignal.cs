using System.Collections.Generic;

namespace ATSPM.Application.Business.Aggregation.FilterExtensions
{
    public class FilterSignal
    {
        public string LocationIdentifier { get; set; }
        public bool Exclude { get; set; }
        public List<FilterApproach> Approaches { get; set; } = new List<FilterApproach>();
    }
}