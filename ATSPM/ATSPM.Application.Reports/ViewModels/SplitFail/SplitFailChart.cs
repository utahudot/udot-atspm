using ATSPM.Application.Reports.ViewModels;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.ViewModels.SplitFail
{
    /// <summary>
    /// Split Fail chart
    /// </summary>
    public class SplitFailChart
    {
        public SplitFailChart(
            string chartName,
            string signalId,
            string signalLocation,
            int phaseNumber,
            string phaseDescription,
            DateTime start,
            DateTime end,
            int totalSplitFails,
            ICollection<SplitFailPlan> plans,
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

        public string ChartName { get; internal set; }
        public string SignalId { get; internal set; }
        public string SignalLocation { get; internal set; }
        public int PhaseNumber { get; internal set; }
        public string PhaseDescription { get; internal set; }
        public DateTime Start { get; internal set; }
        public DateTime End { get; internal set; }
        public int TotalSplitFails { get; internal set; }
        public ICollection<SplitFailPlan> Plans { get; internal set; }
        public ICollection<FailLine> FailLines { get; internal set; }
        public ICollection<GapOutGreenOccupancy> GapOutGreenOccupancies { get; internal set; }
        public ICollection<GapOutRedOccupancy> GapOutRedOccupancies { get; internal set; }
        public ICollection<ForceOffGreenOccupancy> ForceOffGreenOccupancies { get; internal set; }
        public ICollection<ForceOffRedOccupancy> ForceOffRedOccupancies { get; internal set; }
        public ICollection<AverageGor> AverageGor { get; internal set; }
        public ICollection<AverageRor> AverageRor { get; internal set; }
        public ICollection<PercentFail> PercentFails { get; internal set; }
    }
}