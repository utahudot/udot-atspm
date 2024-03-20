using ATSPM.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Data.Interfaces
{
    /// <summary>
    /// For objects in the plan layer
    /// </summary>
    public interface IPlanLayer : ILocationLayer
    {
        /// <summary>
        /// Plan number as derrived from the event parameter on <see cref="IndianaEnumerations.CoordPatternChange"/> event
        /// </summary>
        int PlanNumber { get; set; }
    }
}
