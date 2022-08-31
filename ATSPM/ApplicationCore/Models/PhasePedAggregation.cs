﻿using System;
using System.Collections.Generic;

#nullable disable

namespace ATSPM.Application.Models
{
    public partial class PhasePedAggregation : ATSPMModelBase
    {
        public DateTime BinStartTime { get; set; }
        public string SignalId { get; set; }
        public int PhaseNumber { get; set; }
        public int PedCycles { get; set; }
        public int PedDelaySum { get; set; }
        public int MinPedDelay { get; set; }
        public int MaxPedDelay { get; set; }
        public int PedActuations { get; set; }
    }
}
