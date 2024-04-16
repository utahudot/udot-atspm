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
