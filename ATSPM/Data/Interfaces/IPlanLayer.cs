﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Data.Interfaces
{
    /// <summary>
    /// For objects in the plan layer
    /// </summary>
    public interface IPlanLayer : ISignalLayer
    {
        /// <summary>
        /// Plan number as derrived from the event parameter on <see cref="DataLoggerEnum.CoordPatternChange"/> event
        /// </summary>
        int PlanNumber { get; set; }
    }
}