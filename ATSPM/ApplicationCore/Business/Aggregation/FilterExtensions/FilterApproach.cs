using System.Collections.Generic;

namespace ATSPM.Application.Business.Aggregation.FilterExtensions
{
    public class FilterApproach
    {
        public int ApproachId { get; set; }
        public bool Exclude { get; set; }
        public List<FilterDetector> Detectors { get; set; } = new List<FilterDetector>();
    }
}