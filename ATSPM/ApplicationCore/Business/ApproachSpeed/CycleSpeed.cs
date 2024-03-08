
using ATSPM.Application.Business.Common;
using ATSPM.Data.Models.EventLogModels;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.ApproachSpeed
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
            if (speeds.IsNullOrEmpty() || speeds.Count == 0)
                SpeedEvents = new List<SpeedEvent>();
            else
                SpeedEvents = speeds.Where(s =>
                    s.Timestamp >= GreenEvent.AddSeconds(15) && s.Timestamp < YellowEvent && s.Mph >= 5).ToList();
        }
    }
}