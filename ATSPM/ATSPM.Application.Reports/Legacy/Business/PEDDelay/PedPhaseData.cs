﻿using System;
using System.Collections.Generic;
using ATSPM.Data.Models;

namespace Legacy.Common.Business.PEDDelay
{
    public class PedPhaseData
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Approach Approach { get; set; }
        public int ApproachId { get; set; }
        public int PhaseNumber { get; set; }
        public string SignalId { get; set; }
        public List<PedCycle> Cycles { get; set; }
        public List<PedPlan> Plans { get; set; }
        public List<PedHourlyTotal> HourlyTotals { get; set; }
        public double MinDelay { get; set; }
        public double AverageDelay { get; set; }
        public double MaxDelay { get; set; }
        public double TotalDelay { get; set; }
        public int TimeBuffer { get; set; }
        public int PedPresses { get; set; }
        public int UniquePedDetections { get; set; }
        public int PedRequests { get; set; }
        public int ImputedPedCallsRegistered { get; set; }
        public int PedBeginWalkCount { get; set; }
        public List<ControllerEventLog> PedBeginWalkEvents { get; set; }
        public int PedCallsRegisteredCount { get; set; }
        public int BeginWalkEvent { get; set; }
        public int BeginClearanceEvent { get; set; }
    }

}