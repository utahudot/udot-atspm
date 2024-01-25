#nullable disable
using ATSPM;
using ATSPM.Data.Relationships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Data.Models
{
    public class LocationType : AtspmConfigModelBase<int>, IRelatedLocations
    {
        /// <summary>
        /// Name of location type
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Icon for disquinguishing location type
        /// </summary>
        public string Icon { get; set; }

        #region IRelatedLocations

        /// <inheritdoc/>
        public virtual ICollection<Location> Locations { get; set; } = new HashSet<Location>();

        #endregion
    }
}
