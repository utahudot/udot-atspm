using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.YellowRedActivations
{
    public class YellowRedActivationsOptions
    {
        public double SevereLevelSeconds { get; set; }
        public int ApproachId { get; private set; }
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }
        public bool UsePermissivePhase { get; internal set; }

      
    }
}