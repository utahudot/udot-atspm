using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ATSPM.Data.Models
{
    public class PhasePedAggregation
    {
        public DateTime BinStartTime { get; set; }

        public string locationId { get; set; }

        public int ApproachId { get; set; }

        public int PhaseNumber { get; set; }

        public int PedCycles { get; set; }

        public int PedDelaySum { get; set; }

        public int MinPedDelay { get; set; }

        public int MaxPedDelay { get; set; }

        public int PedRequests { get; set; }

        public int ImputedPedCallsRegistered { get; set; }

        public int UniquePedDetections { get; set; }

        public int PedBeginWalkCount { get; set; }

        public int PedCallsRegisteredCount { get; set; }
    }
}
