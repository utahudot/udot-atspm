using Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.SplitFail
{
    public class SplitFailsResult:ApproachResult
    {
        public SplitFailsResult(
            string signalId,
            int approachId,
            int phaseNumber,
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
            ICollection<PercentFail> percentFails):base(approachId, signalId, start, end)
        {
            PhaseNumber = phaseNumber;
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
        public int PhaseNumber { get; set; }
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
