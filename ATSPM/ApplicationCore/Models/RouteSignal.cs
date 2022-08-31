﻿using System;
using System.Collections.Generic;

#nullable disable

namespace ATSPM.Application.Models
{
    public partial class RouteSignal : ATSPMModelBase
    {
        public RouteSignal()
        {
            RoutePhaseDirections = new HashSet<RoutePhaseDirection>();
        }

        public int Id { get; set; }
        public int RouteId { get; set; }
        public int Order { get; set; }
        public string SignalId { get; set; }

        public virtual Route Route { get; set; }
        public virtual ICollection<RoutePhaseDirection> RoutePhaseDirections { get; set; }
    }
}
