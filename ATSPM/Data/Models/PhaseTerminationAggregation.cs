﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using ATSPM.Data.Interfaces;
using System;
using System.Collections.Generic;

namespace ATSPM.Data.Models
{
    public partial class PhaseTerminationAggregation : ATSPMAggregationBase, ISignalLayer
    {
        public string SignalIdentifier { get; set; }
        public int PhaseNumber { get; set; }
        public int GapOuts { get; set; }
        public int ForceOffs { get; set; }
        public int MaxOuts { get; set; }
        public int Unknown { get; set; }
    }
}