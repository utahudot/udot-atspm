#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.PhaseTermination/Phase.cs
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
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.PhaseTermination;

public class Phase
{
    public Phase(
        int phaseNumber,
        ICollection<DateTime> gapOuts,
        ICollection<DateTime> maxOuts,
        ICollection<DateTime> forceOffs,
        ICollection<DateTime> pedWalkBegins,
        ICollection<DateTime> unknownTerminations)
    {
        PhaseNumber = phaseNumber;
        GapOuts = gapOuts;
        MaxOuts = maxOuts;
        ForceOffs = forceOffs;
        PedWalkBegins = pedWalkBegins;
        UnknownTerminations = unknownTerminations;
    }

    public int PhaseNumber { get; set; }
    public ICollection<DateTime> GapOuts { get; internal set; }
    public ICollection<DateTime> MaxOuts { get; internal set; }
    public ICollection<DateTime> ForceOffs { get; internal set; }
    public ICollection<DateTime> PedWalkBegins { get; internal set; }
    public ICollection<DateTime> UnknownTerminations { get; internal set; }
}
