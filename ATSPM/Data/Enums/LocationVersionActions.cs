using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Data.Enums
{
    /// <summary>
    /// Location version actions
    /// </summary>
    public enum LocationVersionActions
    {
        /// <summary>
        /// Version is unknown
        /// </summary>
        Unknown = 0,
       
        /// <summary>
        /// Version is new
        /// </summary>
        New = 1,
        
        /// <summary>
        /// Version is in edit mode
        /// </summary>
        Edit = 2,

        /// <summary>
        /// Version has been deleted
        /// </summary>
        Delete = 3,
        
        /// <summary>
        /// New version has been copied
        /// </summary>
        NewVersion = 4,

        /// <summary>
        /// Initial version
        /// </summary>
        Initial = 10
    }
}
