using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.ApproachSpeed
{
    public class ApproachSpeedOptions
    {
        public int SelectedBinSize { get; set; }
        public int MetricTypeId { get; } = 10;
        public string SignalId { get; set; }
        public int ApproachId { get; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool UsePermissivePhase { get; set; }
    }
}