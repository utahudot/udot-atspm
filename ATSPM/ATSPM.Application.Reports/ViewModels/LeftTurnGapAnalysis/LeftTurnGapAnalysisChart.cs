using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.ViewModels.LeftTurnGapAnalysis
{
    /// <summary>
    /// Left Turn Gap Analysis chart
    /// </summary>
    public class LeftTurnGapAnalyisiChart
    {
        public LeftTurnGapAnalyisiChart(
            string chartName,
            string signalId,
            string signalLocation,
            int phaseNumber,
            string phaseDescription,
            DateTime start,
            DateTime dateTime,
            string detectionTypeDescription,
            int numberGaps,
            double percentOfGreenTime,
            double gap1,
            ICollection<Gap1Count> gap1Count,
            double gap2,
            ICollection<Gap2Count> gap2Count,
            double gap3,
            ICollection<Gap3Count> gap3Count,
            double gap4,
            ICollection<Gap4Count> gap4Count,
            ICollection<PercentTurnableSeries> percentTurnableSeries)
        {
            ChartName = chartName;
            SignalId = signalId;
            SignalLocation = signalLocation;
            PhaseNumber = phaseNumber;
            PhaseDescription = phaseDescription;
            Start = start;
            DateTime = dateTime;
            DetectionTypeDescription = detectionTypeDescription;
            NumberGaps = numberGaps;
            PercentOfGreenTime = percentOfGreenTime;
            Gap1 = gap1;
            Gap1Count = gap1Count;
            Gap2 = gap2;
            Gap2Count = gap2Count;
            Gap3 = gap3;
            Gap3Count = gap3Count;
            Gap4 = gap4;
            Gap4Count = gap4Count;
            PercentTurnableSeries = percentTurnableSeries;
        }

        public string ChartName { get; internal set; }
        public string SignalId { get; internal set; }
        public string SignalLocation { get; internal set; }
        public int PhaseNumber { get; internal set; }
        public string PhaseDescription { get; internal set; }
        public DateTime Start { get; internal set; }
        public DateTime DateTime { get; internal set; }
        public string DetectionTypeDescription { get; internal set; }
        public int NumberGaps { get; internal set; }
        public double PercentOfGreenTime { get; internal set; }
        public double Gap1 { get; internal set; }
        public ICollection<Gap1Count> Gap1Count { get; internal set; }
        public double Gap2 { get; internal set; }
        public ICollection<Gap2Count> Gap2Count { get; internal set; }
        public double Gap3 { get; internal set; }
        public ICollection<Gap3Count> Gap3Count { get; internal set; }
        public double Gap4 { get; internal set; }
        public ICollection<Gap4Count> Gap4Count { get; internal set; }
        public ICollection<PercentTurnableSeries> PercentTurnableSeries { get; internal set; }

    }
}