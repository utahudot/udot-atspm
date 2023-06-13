using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.PhaseTermination
{
    public class PhaseTerminationOptions
    {      
        public int SelectedConsecutiveCount { get; set; }
        public string SignalId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}