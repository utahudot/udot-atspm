using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class Signal
    {
        public Signal()
        {
            Areas = new HashSet<Area>();
        }

        public string SignalId { get; set; } = null!;
        public string Latitude { get; set; } = null!;
        public string Longitude { get; set; } = null!;
        public string PrimaryName { get; set; } = null!;
        public string SecondaryName { get; set; } = null!;
        public string Ipaddress { get; set; } = null!;
        public int RegionId { get; set; }
        public int ControllerTypeId { get; set; }
        public bool Enabled { get; set; }
        public int VersionId { get; set; }
        public int VersionActionId { get; set; }
        public string Note { get; set; } = null!;
        public DateTime Start { get; set; }
        public int JurisdictionId { get; set; }
        public bool Pedsare1to1 { get; set; }

        public virtual ControllerType ControllerType { get; set; } = null!;
        public virtual Jurisdiction Jurisdiction { get; set; } = null!;
        public virtual Region Region { get; set; } = null!;

        public virtual ICollection<Area> Areas { get; set; }
    }
}
