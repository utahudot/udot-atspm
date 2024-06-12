#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.PurdueCoordinationDiagram/PurdueCoordinationDiagramService.cs
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
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.PurdueCoordinationDiagram
{
    public class PurdueCoordinationDiagramService
    {

        public PurdueCoordinationDiagramService()
        {
        }

        public PurdueCoordinationDiagramResult GetChartData(
            PurdueCoordinationDiagramOptions options,
            Approach approach,
            LocationPhase LocationPhase)
        {
            List<DataPointForDouble> volume = new List<DataPointForDouble>();
            if (options.GetVolume)
            {
                volume = LocationPhase.Volume.Items.ConvertAll(v => new DataPointForDouble(v.StartTime, v.HourlyVolume));
            }
            var plans = LocationPhase.Plans.Select(p => new PerdueCoordinationPlanViewModel(
                p.PlanNumber.ToString(),
                p.Start,
                p.End,
                p.PercentGreenTime,
                p.PercentArrivalOnGreen,
                p.PlatoonRatio)).ToList();
            var redSeries = LocationPhase.Cycles.Select(c => new DataPointForDouble(c.EndTime, c.RedLineY));
            var yellowSeries = LocationPhase.Cycles.Select(c => new DataPointForDouble(c.EndTime, c.YellowLineY));
            var greenSeries = LocationPhase.Cycles.Select(c => new DataPointForDouble(c.EndTime, c.GreenLineY));
            var detectorEvents = LocationPhase.Cycles.SelectMany(c => c.DetectorEvents.Select(d => new DataPointForDouble(d.TimeStamp, d.YPointSeconds)));

            return new PurdueCoordinationDiagramResult(
                options.LocationIdentifier,
                approach.Id,
                approach.ProtectedPhaseNumber,
                approach.Description,
                options.Start,
                options.End,
                Convert.ToInt32(LocationPhase.TotalArrivalOnGreen),
                Convert.ToInt32(LocationPhase.TotalVolume),
                LocationPhase.PercentArrivalOnGreen,
                plans,
                volume,
                redSeries.ToList(),
                yellowSeries.ToList(),
                greenSeries.ToList(),
                detectorEvents.ToList()
                );
        }
    }
}
