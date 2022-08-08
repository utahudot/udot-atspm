using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class RouteSignal
    {
        public RouteSignal()
        {
            RoutePhaseDirections = new HashSet<RoutePhaseDirection>();
        }

        public int Id { get; set; }
        public int RouteId { get; set; }
        public int Order { get; set; }
        public string SignalId { get; set; } = null!;

        public virtual Route Route { get; set; } = null!;
        public virtual ICollection<RoutePhaseDirection> RoutePhaseDirections { get; set; }
    }
}
