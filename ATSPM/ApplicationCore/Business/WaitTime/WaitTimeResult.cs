#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.WaitTime/WaitTimeResult.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
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