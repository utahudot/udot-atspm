using ATSPM.Application.Repositories;
using ATSPM.Application.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Legacy.Common.Business
{
    public class AnalysisPhaseCollection
    {
        public List<AnalysisPhase> Items = new List<AnalysisPhase>();
        private readonly IControllerEventLogRepository _controllerEventLogRepository;
        private readonly ISignalRepository _signalRepository;

        public AnalysisPhaseCollection(
            string signalId,
            DateTime startTime,
            DateTime endTime,
            int consecutivecount,
            IControllerEventLogRepository controllerEventLogRepository,
            ISignalRepository signalRepository)
        {
            SignalId = signalId;
            _controllerEventLogRepository = controllerEventLogRepository;
            _signalRepository = signalRepository;
            var ptedt = _controllerEventLogRepository.GetSignalEventsByEventCodes(
                SignalId,
                startTime,
                endTime,
                new List<int> { 1, 11, 4, 5, 6, 7, 21, 23 });
            var dapta = _controllerEventLogRepository.GetSignalEventsByEventCodes(
                SignalId,
                startTime,
                endTime,
                new List<int> { 1 });
            ptedt = ptedt.OrderByDescending(i => i.Timestamp).ToList();
            var phasesInUse = dapta.Where(r => r.EventCode == 1).Select(r => r.EventParam).Distinct();
            Plans = PlanService.GetSplitMonitorPlans(startTime, endTime, SignalId);
            foreach (var row in phasesInUse)
            {
                var aPhase = new AnalysisPhase(row, ptedt, consecutivecount, _signalRepository);
                Items.Add(aPhase);
            }
            OrderPhases();
            MaxPhaseInUse = FindMaxPhase(Items);
        }

        public AnalysisPhaseCollection(string signalId, DateTime startTime, DateTime endTime)
        {
            var cel = ControllerEventLogRepositoryFactory.Create();
            var ptedt = cel.GetSignalEventsByEventCodes(signalId, startTime, endTime,
                new List<int> {1, 11, 4, 5, 6, 7, 21, 23});
            var dapta = cel.GetSignalEventsByEventCodes(signalId, startTime, endTime, new List<int> {1});
            var phasesInUse = dapta.Where(d => d.EventCode == 1).Select(d => d.EventParam).Distinct();
            Plans = PlanService.GetSplitMonitorPlans(startTime, endTime, signalId);
            foreach (var row in phasesInUse)
            {
                var aPhase = new AnalysisPhase(row, signalId, ptedt);
                Items.Add(aPhase);
            }
            OrderPhases();
        }

        public List<PlanSplitMonitor> Plans { get; }
        public int MaxPhaseInUse { get; }
        public string SignalId { get; }

        private void OrderPhases()
        {
            Items = Items.OrderBy(i => i.PhaseNumber).ToList();
        }

        private int FindMaxPhase(List<AnalysisPhase> analysisPhases)
        {
            return analysisPhases.Select(phase => phase.PhaseNumber).Concat(new[] {0}).Max();
        }
    }
}