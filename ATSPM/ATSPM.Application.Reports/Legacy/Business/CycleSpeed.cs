using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Legacy.Common.Business
{
    public class CycleSpeed : RedToRedCycle
    {
        public CycleSpeed(DateTime firstRedEvent, DateTime greenEvent, DateTime yellowEvent, DateTime lastRedEvent) :
            base(firstRedEvent, greenEvent, yellowEvent, lastRedEvent)
        {
        }

        public List<SpeedEvent> SpeedEvents { get; set; }

        public void FindSpeedEventsForCycle(List<SpeedEvent> speeds)
        {
            SpeedEvents = speeds.Where(s =>
                s.Timestamp >= GreenEvent.AddSeconds(15) && s.Timestamp < YellowEvent && s.Mph >= 5).ToList();
        }
    }
}