using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class Area
    {
        public Area()
        {
            SignalVersions = new HashSet<Signal>();
        }

        public int Id { get; set; }
        public string? AreaName { get; set; }

        public virtual ICollection<Signal> SignalVersions { get; set; }
    }
}
