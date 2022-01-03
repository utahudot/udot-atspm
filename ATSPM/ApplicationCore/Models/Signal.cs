using System;
using System.Collections.Generic;

#nullable disable

namespace ATSPM.Application.Models
{
    public partial class Signal : ATSPMModelBase
    {
        public string SignalId { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string PrimaryName { get; set; }
        public string SecondaryName { get; set; }
        public string Ipaddress { get; set; }
        public int RegionId { get; set; }
        public int ControllerTypeId { get; set; }
        public bool Enabled { get; set; }
        public int VersionId { get; set; }
        public int VersionActionId { get; set; }
        public string Note { get; set; }
        public DateTime Start { get; set; }

        public virtual ControllerType ControllerType { get; set; }
        public virtual Region Region { get; set; }
        public virtual VersionAction VersionAction { get; set; }
        public virtual ICollection<Approach> Approaches { get; set; }

        public override string ToString()
        {
            return $"{SignalId}";
        }
    }
}
