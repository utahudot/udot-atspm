using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.WaitTime
{

    /// <summary>
    /// Wait Time chart
    /// </summary>
    public class WaitTimeResult
    {
        public WaitTimeResult(string chartName,
                             int approachId,
                             string approachDescription,
                             int phaseNumber,
                             DateTime start,
                             DateTime end,
                             string detectionTypes,
                             ICollection<PlanWaitTime> plans,
                             ICollection<WaitTimePoint> gapOuts,
                             ICollection<WaitTimePoint> maxOuts,
                             ICollection<WaitTimePoint> forceOffs,
                             ICollection<WaitTimePoint> unknowns,
                             ICollection<WaitTimePoint> average,
                             ICollection<Volume> volumes)
        {
            ChartName = chartName;
            ApproachId = approachId;
            ApproachDescription = approachDescription;
            PhaseNumber = phaseNumber;
            Start = start;
            End = end;
            DetectionTypes = detectionTypes;
            Plans = plans;
            GapOuts = gapOuts;
            MaxOuts = maxOuts;
            ForceOffs = forceOffs;
            Unknowns = unknowns;
            Average = average;
            Volumes = volumes;
        }

        public string ChartName { get; internal set; }
        public int ApproachId { get; internal set; }
        public string ApproachDescription { get; internal set; }
        public int PhaseNumber { get; internal set; }
        public DateTime Start { get; internal set; }
        public DateTime End { get; internal set; }
        public string DetectionTypes { get; internal set; }
        public ICollection<PlanWaitTime> Plans { get; internal set; }
        public ICollection<WaitTimePoint> GapOuts { get; internal set; }
        public ICollection<WaitTimePoint> MaxOuts { get; internal set; }
        public ICollection<WaitTimePoint> ForceOffs { get; internal set; }
        public ICollection<WaitTimePoint> Unknowns { get; internal set; }
        public ICollection<WaitTimePoint> Average { get; internal set; }
        public ICollection<Volume> Volumes { get; internal set; }
        public ICollection<PlanSplit> PlanSplits { get; internal set; }
    }
}