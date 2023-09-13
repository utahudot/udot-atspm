using ATSPM.Application.Reports.Business.Common;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.PerdueCoordinationDiagram
{
    public class PerdueCoordinationDiagramService
    {

        public PerdueCoordinationDiagramService()
        {
        }

        public PerdueCoordinationDiagramResult GetChartData(
            PerdueCoordinationDiagramOptions options,
            Data.Models.Approach approach,
            SignalPhase signalPhase)
        {
            List<VolumePerHour> volume = new List<VolumePerHour>();
            if (options.ShowVolumes)
            {
                volume = signalPhase.Volume.Items.ConvertAll(v => new VolumePerHour(v.StartTime, v.HourlyVolume));
            }
            return new PerdueCoordinationDiagramResult(
                options.SignalIdentifier,
                approach.Id,
                approach.ProtectedPhaseNumber,
                approach.Description,
                options.Start,
                options.End,
                Convert.ToInt32(signalPhase.TotalArrivalOnGreen),
                Convert.ToInt32(signalPhase.TotalVolume),
                signalPhase.PercentArrivalOnGreen,
                signalPhase.Plans,
                volume,
                signalPhase.Cycles
                );
        }
    }
}
