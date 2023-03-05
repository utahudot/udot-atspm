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

        public PerdueCoordinationDiagramResult GetChartData(PerdueCoordinationDiagramOptions options)//, Data.Models.Approach approach, System.Collections.Generic.IReadOnlyList<Data.Models.ControllerEventLog> events)
        {
            var approach = approachRepository.Lookup(options.ApproachId);
            var signalPhase = signalPhaseService.GetSignalPhaseData(
                options.StartDate,
                options.EndDate,
                false,
                options.ShowVolumes,
                0,
                options.SelectedBinSize,
                6,
                approach
                );
            var volume = signalPhase.Volume.Items.ConvertAll(v => new VolumePerHour(v.StartTime, v.YAxis));
            return new PerdueCoordinationDiagramResult(
                "Perdue Coordination Diagram",
                options.SignalId,
                approach.Signal.SignalDescription(),
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
