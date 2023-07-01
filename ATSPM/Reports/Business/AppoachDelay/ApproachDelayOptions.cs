using ATSPM.Data.Models;
using System;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.AppoachDelay
{
    public class ApproachDelayOptions
    {
        public int ApproachId { get; set; }
        public bool GetPermissivePhase { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int BinSize { get; set; }
        public bool GetVolume { get; set; }
    }
}