#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.SplitMonitor/SplitMonitorResult.cs
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

namespace ATSPM.Application.Business.SplitMonitor;

/// <summary>
/// Split Monitor chart
/// </summary>
public class SplitMonitorResult : LocationResult
{
    public SplitMonitorResult(int phaseNumber, string phaseDescription, string locationId, DateTime start, DateTime end) : base(locationId, start, end)
    {
        PhaseNumber = phaseNumber;
        PhaseDescription = phaseDescription;
        Plans = new List<PlanSplitMonitorDTO>();
        ProgrammedSplits = new List<DataPointForDouble>();
        GapOuts = new List<DataPointForDouble>();
        MaxOuts = new List<DataPointForDouble>();
        ForceOffs = new List<DataPointForDouble>();
        Unknowns = new List<DataPointForDouble>();
        Peds = new List<DataPointForDouble>();
    }

    public SplitMonitorResult(
        string locationId,
        DateTime start,
        DateTime end,
        int phaseNumber,
        int percentileSplit,
        ICollection<PlanSplitMonitorDTO> plans,
        ICollection<DataPointForDouble> programmedSplits,
        ICollection<DataPointForDouble> gapOuts,
        ICollection<DataPointForDouble> maxOuts,
        ICollection<DataPointForDouble> forceOffs,
        ICollection<DataPointForDouble> unknowns,
        ICollection<DataPointForDouble> peds) : base(locationId, start, end)
    {
        PhaseNumber = phaseNumber;
        PercentileSplit = percentileSplit;
        Plans = plans;
        ProgrammedSplits = programmedSplits;
        GapOuts = gapOuts;
        MaxOuts = maxOuts;
        ForceOffs = forceOffs;
        Unknowns = unknowns;
        Peds = peds;
    }
    public int PhaseNumber { get; internal set; }
    public int PercentileSplit { get; internal set; }
    public string PhaseDescription { get; internal set; }
    public ICollection<PlanSplitMonitorDTO> Plans { get; internal set; }
    public ICollection<DataPointForDouble> ProgrammedSplits { get; internal set; }
    public ICollection<DataPointForDouble> GapOuts { get; internal set; }
    public ICollection<DataPointForDouble> MaxOuts { get; internal set; }
    public ICollection<DataPointForDouble> ForceOffs { get; internal set; }
    public ICollection<DataPointForDouble> Unknowns { get; internal set; }
    public ICollection<DataPointForDouble> Peds { get; internal set; }
}
