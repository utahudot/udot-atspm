﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using ATSPM.Data.Interfaces;
using System;
using System.Collections.Generic;

namespace ATSPM.Data.Models
{
    public partial class PriorityAggregation : ATSPMAggregationBase, ISignalLayer
    {
        public string SignalIdentifier { get; set; }
        public int PriorityNumber { get; set; }
        public int PriorityRequests { get; set; }
        public int PriorityServiceEarlyGreen { get; set; }
        public int PriorityServiceExtendedGreen { get; set; }
    }
}