﻿using ATSPM.Data.Models;
using ATSPM.ReportApi.Business.Common;

namespace ATSPM.ReportApi.Business.PurdueCoordinationDiagram
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
            List<DataPointForDouble> volume = new List<DataPointForDouble>();
            if (options.ShowVolumes)
            {
                volume = signalPhase.Volume.Items.ConvertAll(v => new DataPointForDouble(v.StartTime, v.HourlyVolume));
            }
            var plans = signalPhase.Plans.Select(p => new PerdueCoordinationPlanViewModel(
                p.PlanNumber.ToString(),
                p.Start,
                p.End,
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