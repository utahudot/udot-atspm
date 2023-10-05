using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Reports.Business.PerdueCoordinationDiagram;
using ATSPM.Data.Models;
using Reports.Business.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Reports.Business.PurdueCoordinationDiagram
{
    public class PurdueCoordinationDiagramService
    {

        public PurdueCoordinationDiagramService()
        {
        }

        public PurdueCoordinationDiagramResult GetChartData(
            PurdueCoordinationDiagramOptions options,
            Approach approach,
            SignalPhase signalPhase)
        {
            List<VolumePerHour> volume = new List<VolumePerHour>();
            if (options.ShowVolumes)
            {
                volume = signalPhase.Volume.Items.ConvertAll(v => new VolumePerHour(v.StartTime, v.HourlyVolume));
            }
            var plans = signalPhase.Plans.Select(p => new PerdueCoordinationPlanViewModel(
                p.PlanNumber.ToString(),
                p.Start,
                p.EndTime,
                p.PercentGreenTime,
                p.PercentArrivalOnGreen,
                p.PlatoonRatio)).ToList();
            var redSeries = signalPhase.Cycles.Select(c => new DataPointForDouble(c.EndTime, c.RedLineY));
            var yellowSeries = signalPhase.Cycles.Select(c => new DataPointForDouble(c.EndTime, c.YellowLineY));
            var greenSeries = signalPhase.Cycles.Select(c => new DataPointForDouble(c.EndTime, c.GreenLineY));
            var detectorEvents = signalPhase.Cycles.SelectMany(c => c.DetectorEvents.Select(d => new DataPointForDouble(d.TimeStamp, d.YPointSeconds)));

            return new PurdueCoordinationDiagramResult(
                options.SignalIdentifier,
                approach.Id,
                approach.ProtectedPhaseNumber,
                approach.Description,
                options.Start,
                options.End,
                Convert.ToInt32(signalPhase.TotalArrivalOnGreen),
                Convert.ToInt32(signalPhase.TotalVolume),
                signalPhase.PercentArrivalOnGreen,
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
