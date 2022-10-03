using ATSPM.Application.Reports.ViewModels.YellowRedActivations;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.ViewModels.WaitTime
{

    /// <summary>
    /// Wait Time chart
    /// </summary>
    public class WaitTimeChart
    {
        public WaitTimeChart(string chartName,
                             string signalId,
                             string signalLocation,
                             int phaseNumber,
                             string phaseDescription,
                             DateTime start,
                             DateTime end,
                             string detectionTypes,
                             ICollection<YellowRedActivationsPlan> plans,
                             ICollection<WaitTimeGapOut> gapOuts,
                             ICollection<WaitTimeMaxOut> maxOuts,
                             ICollection<WaitTimeForceOff> forceOffs,
                             ICollection<WaitTimeUnknown> unknowns,
                             ICollection<Average> average)
        {
            ChartName = chartName;
            SignalId = signalId;
            SignalLocation = signalLocation;
            PhaseNumber = phaseNumber;
            PhaseDescription = phaseDescription;
            Start = start;
            End = end;
            DetectionTypes = detectionTypes;
            Plans = plans;
            GapOuts = gapOuts;
            MaxOuts = maxOuts;
            ForceOffs = forceOffs;
            Unknowns = unknowns;
            Average = average;
        }

        public string ChartName { get; internal set; }
        public string SignalId { get; internal set; }
        public string SignalLocation { get; internal set; }
        public int PhaseNumber { get; internal set; }
        public string PhaseDescription { get; internal set; }
        public DateTime Start { get; internal set; }
        public DateTime End { get; internal set; }
        public string DetectionTypes { get; internal set; }
        public System.Collections.Generic.ICollection<YellowRedActivationsPlan> Plans { get; internal set; }
        public System.Collections.Generic.ICollection<WaitTimeGapOut> GapOuts { get; internal set; }
        public System.Collections.Generic.ICollection<WaitTimeMaxOut> MaxOuts { get; internal set; }
        public System.Collections.Generic.ICollection<WaitTimeForceOff> ForceOffs { get; internal set; }
        public System.Collections.Generic.ICollection<WaitTimeUnknown> Unknowns { get; internal set; }
        public System.Collections.Generic.ICollection<Average> Average { get; internal set; }
    }
}