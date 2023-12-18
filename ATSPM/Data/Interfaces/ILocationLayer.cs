using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Data.Interfaces
{
    /// <summary>
    /// For objects in the location layer
    /// </summary>
    public interface ILocationLayer
    {
        /// <summary>
        /// Identifier of location
        /// </summary>
        string LocationIdentifier { get; set; }
    }
}
