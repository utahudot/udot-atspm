using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Repositories;
using System;

namespace ATSPM.Application.Reports.Business.PerdueCoordinationDiagram
{
    public class PerdueCoordinationDiagramService
    {
        private readonly SignalPhaseService signalPhaseService;
        private readonly IApproachRepository approachRepository;

        public PerdueCoordinationDiagramService(
            SignalPhaseService signalPhaseService,
            IApproachRepository approachRepository)
        {
            this.signalPhaseService = signalPhaseService;
            this.approachRepository = approachRepository;
        }

        public PerdueCoordinationDiagramResult GetChartData(
            PerdueCoordinationDiagramOptions options,
            Data.Models.Approach approach,
            SignalPhase signalPhase)
        {            
            var volume = signalPhase.Volume.Items.ConvertAll(v => new VolumePerHour(v.StartTime, v.HourlyVolume));
            return new PerdueCoordinationDiagramResult(
                options.SignalId,
                approach.Id,
                approach.ProtectedPhaseNumber,
                approach.Description,
                options.StartDate,
                options.EndDate,
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
