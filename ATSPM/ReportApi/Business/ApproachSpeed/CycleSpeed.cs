﻿using ATSPM.Data.Models;
using ATSPM.ReportApi.Business.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.ReportApi.Business.ApproachSpeed
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
                s.TimeStamp >= GreenEvent.AddSeconds(15) && s.TimeStamp < YellowEvent && s.Mph >= 5).ToList();
        }
    }
}