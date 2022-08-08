using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class RoutePhaseDirection
    {
        public int Id { get; set; }
        public int RouteSignalId { get; set; }
        public int Phase { get; set; }
        public int DirectionTypeId { get; set; }
        public bool IsOverlap { get; set; }
        public bool IsPrimaryApproach { get; set; }

        public virtual DirectionType DirectionType { get; set; } = null!;
        public virtual RouteSignal RouteSignal { get; set; } = null!;
    }
}
