using ATSPM.Data.Models.EventLogModels;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.Common
{
    /// <summary>
    ///     Data that represents a red to red cycle for a Location phase
    /// </summary>
    public class WaitTimeCycle
    {
        public WaitTimeCycle(DateTime redEvent, DateTime greenEvent)
        {
            PhaseRegisterDroppedCalls = new List<IndianaEvent>();
            RedEvent = redEvent;
            GreenEvent = greenEvent;
        }

        public List<IndianaEvent> PhaseRegisterDroppedCalls { get; set; }
        public DateTime RedEvent { get; }
        public DateTime GreenEvent { get; }
    }
}