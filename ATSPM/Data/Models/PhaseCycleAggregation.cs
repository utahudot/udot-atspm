﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using ATSPM.Data.Interfaces;
using System;
using System.Collections.Generic;

namespace ATSPM.Data.Models
{
    public partial class PhaseCycleAggregation : ATSPMAggregationBase, ISignalLayer
    {
        public string SignalIdentifier { get; set; }
        public int ApproachId { get; set; }
        public int PhaseNumber { get; set; }
        public int RedTime { get; set; }
        public int YellowTime { get; set; }
        public int GreenTime { get; set; }
        public int TotalRedToRedCycles { get; set; }
        public int TotalGreenToGreenCycles { get; set; }
    }
}