#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.PurdueCoordinationDiagram/PurdueCoordinationDiagramResult.cs
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

namespace ATSPM.Application.Business.PurdueCoordinationDiagram;

/// <summary>
/// Perdue Coordination Diagram chart
/// </summary>
public class PurdueCoordinationDiagramResult : ApproachResult
{
    public PurdueCoordinationDiagramResult(
        string locationId,
        int approachId,
        int phaseNumber,
        string phaseDescription,
        DateTime start,
        DateTime end,
        int totalOnGreenEvents,
        int totalDetectorHits,
        double percentArrivalOnGreen,
        ICollection<PerdueCoordinationPlanViewModel> plans,
        ICollection<DataPointForDouble> volumePerHour,
        ICollection<DataPointForDouble> redSeries,
        ICollection<DataPointForDouble> yellowSeries,
        ICollection<DataPointForDouble> greenSeries,
        ICollection<DataPointForDouble> detectorEvents) : base(approachId, locationId, start, end)
    {
        PhaseNumber = phaseNumber;
        PhaseDescription = phaseDescription;
        TotalOnGreenEvents = totalOnGreenEvents;
        TotalDetectorHits = totalDetectorHits;
        PercentArrivalOnGreen = percentArrivalOnGreen;
        Plans = plans;
        VolumePerHour = volumePerHour;
        RedSeries = redSeries;
        YellowSeries = yellowSeries;
        GreenSeries = greenSeries;
        DetectorEvents = detectorEvents;
    }
    public int PhaseNumber { get; }
    public string PhaseDescription { get; }
    public int TotalOnGreenEvents { get; }
    public int TotalDetectorHits { get; }
    public double PercentArrivalOnGreen { get; }
    public ICollection<PerdueCoordinationPlanViewModel> Plans { get; }
    public ICollection<DataPointForDouble> VolumePerHour { get; }
    public ICollection<DataPointForDouble> RedSeries { get; }
    public ICollection<DataPointForDouble> YellowSeries { get; }
    public ICollection<DataPointForDouble> GreenSeries { get; }
    public ICollection<DataPointForDouble> DetectorEvents { get; }

}
