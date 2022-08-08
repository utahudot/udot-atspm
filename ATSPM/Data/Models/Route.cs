using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class Route
    {
        public Route()
        {
            RouteSignals = new HashSet<RouteSignal>();
        }

        public int Id { get; set; }
        public string RouteName { get; set; } = null!;

        public virtual ICollection<RouteSignal> RouteSignals { get; set; }
    }
}
