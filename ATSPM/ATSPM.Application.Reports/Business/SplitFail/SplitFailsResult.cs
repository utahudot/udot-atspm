using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.SplitFail
{
    public class SplitFailsResult
    {
        public SplitFailsResult(
            string chartName,
            string signalId,
            string signalLocation,
            int phaseNumber,
            string phaseDescription,
            DateTime start,
            DateTime end,
            int totalSplitFails,
            ICollection<PlanSplitFail> plans,
            ICollection<FailLine> failLines,
            ICollection<GapOutGreenOccupancy> gapOutGreenOccupancies,
            ICollection<GapOutRedOccupancy> gapOutRedOccupancies,
            ICollection<ForceOffGreenOccupancy> forceOffGreenOccupancies,
            ICollection<ForceOffRedOccupancy> forceOffRedOccupancies,
            ICollection<AverageGor> averageGor,
            ICollection<AverageRor> averageRor,
            ICollection<PercentFail> percentFails)
        {
            ChartName = chartName;
            SignalId = signalId;
            SignalLocation = signalLocation;
            PhaseNumber = phaseNumber;
            PhaseDescription = phaseDescription;
            Start = start;
            End = end;
            TotalSplitFails = totalSplitFails;
            Plans = plans;
            FailLines = failLines;
            GapOutGreenOccupancies = gapOutGreenOccupancies;
            GapOutRedOccupancies = gapOutRedOccupancies;
            ForceOffGreenOccupancies = forceOffGreenOccupancies;
            ForceOffRedOccupancies = forceOffRedOccupancies;
            AverageGor = averageGor;
            AverageRor = averageRor;
            PercentFails = percentFails;
        }

        public string ChartName { get; set; }
        public string SignalId { get; set; }
        public string SignalLocation { get; set; }
        public int PhaseNumber { get; set; }
        public string PhaseDescription { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int TotalSplitFails { get; set; }
        public ICollection<PlanSplitFail> Plans { get; set; }
        public ICollection<FailLine> FailLines { get; set; }
        public ICollection<GapOutGreenOccupancy> GapOutGreenOccupancies { get; set; }
        public ICollection<GapOutRedOccupancy> GapOutRedOccupancies { get; set; }
        public ICollection<ForceOffGreenOccupancy> ForceOffGreenOccupancies { get; set; }
        public ICollection<ForceOffRedOccupancy> ForceOffRedOccupancies { get; set; }
        public ICollection<AverageGor> AverageGor { get; set; }
        public ICollection<AverageRor> AverageRor { get; set; }
        public ICollection<PercentFail> PercentFails { get; set; }
    }
}
