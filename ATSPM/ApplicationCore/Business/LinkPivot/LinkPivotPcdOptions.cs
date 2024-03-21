using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Application.Business.LinkPivot
{
    public class LinkPivotPcdOptions
    {
        public string LocationIdentifier { get; set; }
        public string DownLocationIdentifier { get; set; }
        [Required]
        public int Delta { get; set; }
        public string DownDirection { get; set; }
        public string UpstreamDirection { get; set; }
        public DateOnly StartDate { get; set; }
        public DateTime? SelectedEndDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public DateOnly EndDate { get; set; }
    }
}
