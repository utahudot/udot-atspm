using Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.WaitTime
{

    /// <summary>
    /// Wait Time chart
    /// </summary>
    public class WaitTimeResult:ApproachResult
    {
        public WaitTimeResult(string signalId,
                             int approachId,
                             string approachDescription,
                             int phaseNumber,
                             DateTime start,
                             DateTime end,
                             string detectionTypes,
                             IReadOnlyList<PlanWaitTime> plans,
                             IReadOnlyList<WaitTimePoint> gapOuts,
                             IReadOnlyList<WaitTimePoint> maxOuts,
                             IReadOnlyList<WaitTimePoint> forceOffs,
                             IReadOnlyList<WaitTimePoint> unknowns,
                             IReadOnlyList<WaitTimePoint> average,
                             IReadOnlyList<Volume> volumes):base(approachId, signalId, start, end)
        {
            ApproachId = approachId;
            ApproachDescription = approachDescription;
            PhaseNumber = phaseNumber;
            DetectionTypes = detectionTypes;
            Plans = plans;
            GapOuts = gapOuts;
            MaxOuts = maxOuts;
            ForceOffs = forceOffs;
            Unknowns = unknowns;
            Average = average;
            Volumes = volumes;
        }
        public string ApproachDescription { get; internal set; }
        public int PhaseNumber { get; internal set; }
        public string DetectionTypes { get; internal set; }
        public IReadOnlyList<PlanWaitTime> Plans { get; internal set; }
        public IReadOnlyList<WaitTimePoint> GapOuts { get; internal set; }
        public IReadOnlyList<WaitTimePoint> MaxOuts { get; internal set; }
        public IReadOnlyList<WaitTimePoint> ForceOffs { get; internal set; }
        public IReadOnlyList<WaitTimePoint> Unknowns { get; internal set; }
        public IReadOnlyList<WaitTimePoint> Average { get; internal set; }
        public IReadOnlyList<Volume> Volumes { get; internal set; }
        public IReadOnlyList<PlanSplit> PlanSplits { get; internal set; }
    }
}