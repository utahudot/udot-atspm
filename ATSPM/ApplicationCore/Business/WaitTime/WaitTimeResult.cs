using ATSPM.Application.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.WaitTime
{

    /// <summary>
    /// Wait Time chart
    /// </summary>
    public class WaitTimeResult : ApproachResult
    {
        public WaitTimeResult(string locationId,
                             int approachId,
                             string approachDescription,
                             int phaseNumber,
                             DateTime start,
                             DateTime end,
                             string detectionTypes,
                             IReadOnlyList<PlanWaitTime> plans,
                             IReadOnlyList<DataPointForDouble> gapOuts,
                             IReadOnlyList<DataPointForDouble> maxOuts,
                             IReadOnlyList<DataPointForDouble> forceOffs,
                             IReadOnlyList<DataPointForDouble> unknowns,
                             IReadOnlyList<DataPointForDouble> average,
                             IReadOnlyList<DataPointForInt> volumes,
                             IReadOnlyList<DataPointForDouble> splits
                             ) : base(approachId, locationId, start, end)
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
            PlanSplits = splits;
        }
        public string ApproachDescription { get; internal set; }
        public int PhaseNumber { get; internal set; }
        public string DetectionTypes { get; internal set; }
        public IReadOnlyList<PlanWaitTime> Plans { get; internal set; }
        public IReadOnlyList<DataPointForDouble> GapOuts { get; internal set; }
        public IReadOnlyList<DataPointForDouble> MaxOuts { get; internal set; }
        public IReadOnlyList<DataPointForDouble> ForceOffs { get; internal set; }
        public IReadOnlyList<DataPointForDouble> Unknowns { get; internal set; }
        public IReadOnlyList<DataPointForDouble> Average { get; internal set; }
        public IReadOnlyList<DataPointForInt> Volumes { get; internal set; }
        public IReadOnlyList<DataPointForDouble> PlanSplits { get; internal set; }
    }
}